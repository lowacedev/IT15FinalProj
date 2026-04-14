# Program.cs Configuration for IT Service Management System

## Complete Program.cs Setup

```csharp
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ITSMS.Data;

var builder = WebApplication.CreateBuilder(args);

// ==================== SERVICES REGISTRATION ====================

// Add DbContext with MySQL support (Pomelo)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")),
        mySqlOptions => mySqlOptions.CharSetBehavior(CharSetBehavior.NeverAppend)
    )
);

// Add Authentication (Cookie-based)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        
        // Security settings
        options.Cookie.HttpOnly = true; // Prevent JavaScript access
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS only
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
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
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

// Force HTTPS
app.UseHttpsRedirection();

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
    pattern: "{controller=Home}/{action=Index}/{id?}");

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
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred during database initialization: {ex.Message}");
    }
}

app.Run();
```

---

## appsettings.json Configuration

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-mysql-server;Database=itsms_db;User=root;Password=your_password;Port=3306;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "AllowedHosts": "*",
  "SessionSettings": {
    "TimeoutMinutes": 60,
    "SlidingExpiration": true
  }
}
```

---

## appsettings.Development.json Configuration

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=itsms_dev;User=root;Password=dev_password;Port=3306;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

---

## ITSMS.csproj Dependencies

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

    <PropertyGroup>
        <TargetFramework>net9.0</TargetFramework>
        <Nullable>enable</Nullable>
        <ImplicitUsings>enable</ImplicitUsings>
    </PropertyGroup>

    <ItemGroup>
        <!-- Entity Framework Core -->
        <PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
        <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0" />
        <PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="9.0.0" />

        <!-- ASP.NET Core -->
        <PackageReference Include="Microsoft.AspNetCore.Authentication.Cookies" Version="9.0.0" />
        <PackageReference Include="Microsoft.AspNetCore.Identity" Version="9.0.0" />

        <!-- UI Bootstrap -->
        <PackageReference Include="Bootstrap" Version="5.3.2" />
        <PackageReference Include="jquery" Version="3.7.1" />
        <PackageReference Include="jquery-validation" Version="1.19.5" />
        <PackageReference Include="jquery-validation-unobtrusive" Version="4.0.0" />
    </ItemGroup>

</Project>
```

---

## Database Migration Commands

```bash
# Add initial migration
dotnet ef migrations add InitialCreate

# Apply migrations to database
dotnet ef database update

# Create new migration (after model changes)
dotnet ef migrations add AddNewTable

# Revert to previous migration
dotnet ef database update PreviousMigrationName

# Drop database
dotnet ef database drop
```

---

## Running the Application

### Development

```bash
# Restore dependencies
dotnet restore

# Build project
dotnet build

# Run application
dotnet run

# Application runs at https://localhost:5001
# Access at https://localhost:5001/Auth/Login
```

### Production (MonsterASP.NET)

1. **Publish the application:**
   ```bash
   dotnet publish -c Release -o ./publish
   ```

2. **Deploy to MonsterASP.NET:**
   - Upload `/publish` folder to server
   - Configure connection string on server
   - Set environment to Production
   - Enable HTTPS

3. **Environment Variables (on server):**
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ConnectionStrings__DefaultConnection=Server=...;Database=...;
   ```

---

## Configuration Verification Checklist

- [ ] DbContext registered with MySQL provider
- [ ] Authentication middleware added
- [ ] Authorization middleware added
- [ ] Cookie settings configured (HttpOnly, Secure, SameSite)
- [ ] Static files serving enabled
- [ ] Session middleware added (if used)
- [ ] Routes configured correctly
- [ ] Database migrations applied
- [ ] Connection string valid and tested
- [ ] HTTPS redirected in production
- [ ] HSTS headers enabled
- [ ] CORS policies configured (if needed)

