using Microsoft.AspNetCore.Authentication.Cookies;
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

// ==================== SERVICES REGISTRATION ====================

// Add DbContext with MySQL support (Pomelo)
var dbServer = Environment.GetEnvironmentVariable("DB_SERVER") ?? "127.0.0.1";
var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "3306";
var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "itsms";
var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "root";
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";

var connectionString = $"server={dbServer};port={dbPort};database={dbName};uid={dbUser};pwd={dbPassword};";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    )
);

// Add Authentication (Cookie-based)
var cookieSecurePolicy = builder.Environment.IsDevelopment() 
    ? CookieSecurePolicy.SameAsRequest 
    : CookieSecurePolicy.Always;

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
        options.Cookie.SameSite = SameSiteMode.Strict; // CSRF protection
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

// ==================== MIDDLEWARE CONFIGURATION ====================

// Handle errors in development
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // HTTP Strict Transport Security
}
else
{
    // Development-specific middleware
    app.UseDeveloperExceptionPage();
}

// Force HTTPS only in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

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
