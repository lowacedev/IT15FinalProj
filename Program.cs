using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using ITSMS.Data;

// ==================== LOAD ENVIRONMENT VARIABLES ====================

// Load .env file if it exists
var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envPath))
{
    foreach (var line in File.ReadAllLines(envPath))
    {
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
            continue;

        var parts = line.Split('=', 2);
        if (parts.Length == 2)
        {
            var key = parts[0].Trim();
            var value = parts[1].Trim();
            if (!string.IsNullOrEmpty(key))
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }
}

var builder = WebApplication.CreateBuilder(args);

// ==================== DATA PROTECTION (Key Persistence) ====================
// Persist Data Protection keys so anti-forgery tokens and auth cookies
// survive app pool recycles on shared hosting (e.g. MonsterASP)
var keysDir = Path.Combine(builder.Environment.ContentRootPath, "keys");
Directory.CreateDirectory(keysDir);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysDir))
    .SetApplicationName("ITSMS");

// ==================== SERVICES REGISTRATION ====================

// Add DbContext with MySQL support (Pomelo)
var dbServer = Environment.GetEnvironmentVariable("DB_SERVER") ?? "127.0.0.1";
var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "3306";
var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "itsms";
var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "root";
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";

// Prefer environment variables (from .env) for local development if DB_SERVER is explicitly set
var connectionString = $"server={dbServer};port={dbPort};database={dbName};uid={dbUser};pwd={dbPassword};";

if (Environment.GetEnvironmentVariable("DB_SERVER") == null)
{
    var configConnString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrEmpty(configConnString))
    {
        connectionString = configConnString;
    }
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    )
);

// Add Authentication (Cookie-based)
var cookieSecurePolicy = CookieSecurePolicy.SameAsRequest;

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        
        // Security settings
        options.Cookie.HttpOnly = true; // Prevent JavaScript access
        options.Cookie.SecurePolicy = cookieSecurePolicy; // HTTP only in dev, HTTPS in prod
        options.Cookie.SameSite = SameSiteMode.Lax; // More compatible than Strict for some hosting environments
        options.Cookie.Name = "ITSMS.Auth";
    });

// Add Authorization
builder.Services.AddAuthorization();

// Add Session (for additional state management)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = cookieSecurePolicy; // HTTP only in dev, HTTPS in prod
});

// Add MVC Controllers and Views
builder.Services.AddControllersWithViews();

// Add SignalR
builder.Services.AddSignalR();

// Register Custom Services
builder.Services.AddScoped<ITSMS.Services.NotificationService>();
builder.Services.AddScoped<ITSMS.Services.TicketCommentService>();
builder.Services.AddScoped<ITSMS.Services.AuditService>();

// Add CORS (if needed for API endpoints)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", builder =>
    {
        builder.WithOrigins("http://localhost:3000", "https://localhost:3001")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

var app = builder.Build();

// ==================== ROTATIVA CONFIGURATION ====================
Rotativa.AspNetCore.RotativaConfiguration.Setup(app.Environment.WebRootPath, "Rotativa");

// ==================== SET LOCALIZATION (PHP CURRENCY) ====================
var cultureInfo = new System.Globalization.CultureInfo("en-PH");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(cultureInfo),
    SupportedCultures = new[] { cultureInfo },
    SupportedUICultures = new[] { cultureInfo }
});

// ==================== MIDDLEWARE CONFIGURATION ====================

// Handle errors in development
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // app.UseHsts(); // Disabled for HTTP-only hosting
}
else
{
    // Development-specific middleware
    app.UseDeveloperExceptionPage();
}

// Force HTTPS only in production
// if (!app.Environment.IsDevelopment())
// {
//     app.UseHttpsRedirection();
// }

// Serve static files
app.UseStaticFiles();

// Add routing
app.UseRouting();

// ==================== SECURITY MIDDLEWARE ====================

// CORS
app.UseCors("AllowLocalhost");

// Authentication must come before Authorization
app.UseAuthentication();

// Authorization
app.UseAuthorization();

// Session
app.UseSession();

// ==================== ROUTE CONFIGURATION ====================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.MapHub<ITSMS.Hubs.NotificationHub>("/notificationHub");

// ==================== DATABASE INITIALIZATION ====================

// Create database and apply migrations automatically
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    try
    {
        // Apply migrations
        dbContext.Database.Migrate();
        
        // Optional: Seed initial data
        if (!dbContext.Roles.Any())
        {
            Console.WriteLine("Database initialized with migrations.");
        }

        // ===== ERP DATA MIGRATION =====
        // Create Employee records for existing Technician/Client users who don't have one
        var usersWithoutEmployee = dbContext.Users
            .Include(u => u.Role)
            .Include(u => u.Employee)
            .Where(u => u.Employee == null && u.IsActive &&
                        (u.Role.RoleName == "Technician" || u.Role.RoleName == "Employee"))
            .ToList();

        if (usersWithoutEmployee.Any())
        {
            var maxEmpId = dbContext.Employees.Any() 
                ? dbContext.Employees.Max(e => e.Id) 
                : 0;

            foreach (var user in usersWithoutEmployee)
            {
                maxEmpId++;
                var employee = new ITSMS.Models.Employee
                {
                    UserId = user.UserId,
                    DepartmentId = 1, // Default: Information Technology
                    EmployeeCode = $"EMP-{maxEmpId:000}",
                    Status = ITSMS.Models.EmployeeStatus.Active
                };
                dbContext.Employees.Add(employee);
            }

            dbContext.SaveChanges();
            Console.WriteLine($"Created {usersWithoutEmployee.Count} Employee records for existing users.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred during database initialization: {ex.Message}");
    }
}

app.Run();
