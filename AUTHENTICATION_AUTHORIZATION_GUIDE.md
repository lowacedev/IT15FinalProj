# Authentication & Authorization Implementation Guide

## Overview
This document describes the authentication and authorization system for the IT Service Management System using ASP.NET Core cookie-based authentication.

---

## Authentication Design

### Cookie-Based Authentication (Preferred for MVC)

**Why Cookies over JWT for MVC?**
- ✅ Built-in CSRF protection with AntiForgeryToken
- ✅ Automatic session management
- ✅ Simpler to implement in Razor Views
- ✅ Secure by default (HttpOnly, Secure flags)
- ✅ No client-side token storage needed

---

## Architecture

### 1. Authentication Flow

```
User Login →
  Validate Credentials →
  Hash & Compare Password →
  Create Claims →
  Sign Cookie →
  Set HttpContext.User →
  Redirect to Dashboard
```

### 2. Authorization Flow

```
HTTP Request →
  Check Cookie Authentication →
  Extract Claims →
  Apply [Authorize] Attribute →
  Check [Authorize(Roles="")] →
  Allow/Deny Access
```

---

## Implementation Components

### A. Program.cs Configuration

```csharp
// Add Authentication Services
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS only in production
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

// Add Authorization
builder.Services.AddAuthorization();

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));
```

### B. Middleware Configuration (Program.cs)

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

---

## Password Hashing

### Using ASP.NET Core Identity PasswordHasher

```csharp
var passwordHasher = new PasswordHasher<User>();

// Hash password during registration/edit
user.PasswordHash = passwordHasher.HashPassword(user, passwordString);

// Verify password during login
var result = passwordHasher.VerifyHashedPassword(user, storedHash, inputPassword);
if (result == PasswordVerificationResult.Success)
{
    // Password is valid
}
```

**Security Features:**
- 🔐 PBKDF2 algorithm (industry standard)
- 🔐 Automatic salt generation
- 🔐 Iteration count for brute-force resistance
- 🔐 One-way hashing (cannot be reversed)

---

## User Roles

### Role Hierarchy

| Role | Permissions | Use Case |
|------|-----------|----------|
| **Admin** | Full system access, user management, assign technicians, view all reports | IT Manager / Administrator |
| **Technician** | View assigned requests, update status, view reports | IT Support Staff |
| **Client** | Create requests, view own requests, provide feedback | Employees / End-users |

---

## Authorization Attributes

### 1. Basic Authorization (Authenticated Users Only)

```csharp
[Authorize]
public class ServiceRequestsController : Controller
{
    [HttpGet]
    public IActionResult Index() { ... }
}
```

### 2. Role-Based Authorization

```csharp
// Admin only
[Authorize(Roles = "Admin")]
public IActionResult Dashboard() { ... }

// Multiple roles
[Authorize(Roles = "Admin,Technician")]
public IActionResult ViewReports() { ... }

// Technician and Admin
[HttpPost]
[Authorize(Roles = "Admin,Technician")]
public async Task<IActionResult> UpdateStatus(int id) { ... }
```

### 3. Resource-Based Authorization

```csharp
// Check if user owns the resource
private bool CanViewRequest(ServiceRequest request)
{
    var userId = GetCurrentUserId();
    var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

    if (userRole == "Admin")
        return true;

    if (userRole == "Client" && request.RequestorId == userId)
        return true;

    if (userRole == "Technician" && request.AssignedTechnicianId == userId)
        return true;

    return false;
}
```

---

## Claims-Based Identity

### Claims in the System

```csharp
var claims = new List<Claim>
{
    // User identity
    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
    new Claim(ClaimTypes.Name, user.Username),
    new Claim(ClaimTypes.Email, user.Email),
    
    // Custom claims
    new Claim("FullName", user.FullName),
    
    // Role claim (required for [Authorize(Roles="")])
    new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "Client")
};
```

### Accessing Claims in Controller

```csharp
// Get user ID
var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

// Get username
var username = User.FindFirst(ClaimTypes.Name)?.Value;

// Get role
var role = User.FindFirst(ClaimTypes.Role)?.Value;

// Get custom claim
var fullName = User.FindFirst("FullName")?.Value;

// Check if user is in role
if (User.IsInRole("Admin")) { ... }
```

### Accessing Claims in Razor Views

```html
<!-- Check if authenticated -->
@if (User.Identity?.IsAuthenticated ?? false)
{
    <p>Welcome, @User.FindFirst("FullName")?.Value</p>
}

<!-- Check role -->
@if (User.IsInRole("Admin"))
{
    <a href="/Users/Index">Manage Users</a>
}

<!-- Check property -->
@if (User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value == "Technician")
{
    <a href="/Reports/TechnicianWorkload">My Workload</a>
}
```

---

## Login Flow (AuthController.cs)

### Step 1: Validate Credentials
```csharp
var user = _context.Users
    .FirstOrDefault(u => u.Username == username && u.IsActive);
```

### Step 2: Verify Password
```csharp
var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash);
if (result != PasswordVerificationResult.Success)
{
    ModelState.AddModelError("", "Invalid credentials");
}
```

### Step 3: Create Claims Identity
```csharp
var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
    new Claim(ClaimTypes.Name, user.Username),
    new Claim(ClaimTypes.Role, user.Role.RoleName),
    // ... more claims
};

var claimsIdentity = new ClaimsIdentity(
    claims, 
    CookieAuthenticationDefaults.AuthenticationScheme
);
```

### Step 4: Sign In User
```csharp
await HttpContext.SignInAsync(
    CookieAuthenticationDefaults.AuthenticationScheme,
    new ClaimsPrincipal(claimsIdentity),
    new AuthenticationProperties
    {
        IsPersistent = true,
        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
    }
);
```

---

## Registration Flow (AuthController.cs)

### Validation Steps
1. ✅ Passwords match
2. ✅ Username not taken
3. ✅ Email not taken
4. ✅ Valid email format

### Implementation
```csharp
[HttpPost]
public async Task<IActionResult> Register(string username, string email, string password, 
    string confirmPassword, string firstName, string lastName)
{
    // Validate
    if (password != confirmPassword)
    {
        ModelState.AddModelError("", "Passwords do not match");
    }

    if (_context.Users.Any(u => u.Username == username || u.Email == email))
    {
        ModelState.AddModelError("", "Username or email already exists");
    }

    // Create new user
    var newUser = new User
    {
        Username = username,
        Email = email,
        FirstName = firstName,
        LastName = lastName,
        RoleId = 3, // Default: Client role
        IsActive = true
    };

    // Hash password
    newUser.PasswordHash = _passwordHasher.HashPassword(newUser, password);

    // Save to database
    _context.Users.Add(newUser);
    await _context.SaveChangesAsync();

    // Redirect to login
    return RedirectToAction("Login");
}
```

---

## Logout Flow

```csharp
[HttpGet]
public async Task<IActionResult> Logout()
{
    // Clear authentication cookie
    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    
    // Clear session (if used)
    HttpContext.Session?.Clear();
    
    // Redirect to home
    return RedirectToAction("Index", "Home");
}
```

---

## CSRF Protection (Anti-Forgery Tokens)

### Implementation in Form

```html
<form method="post" action="/ServiceRequests/Create">
    <!-- Anti-forgery token -->
    @Html.AntiForgeryToken()
    
    <input type="text" name="Title" />
    <textarea name="Description"></textarea>
    <button type="submit">Submit</button>
</form>
```

### Implementation in Controller

```csharp
[HttpPost]
[ValidateAntiForgeryToken] // Validates token
public async Task<IActionResult> Create(ServiceRequest request)
{
    // Process request
    _context.ServiceRequests.Add(request);
    await _context.SaveChangesAsync();
}
```

---

## Security Best Practices

### ✅ Implemented in This System

1. **Password Hashing**
   - Using PasswordHasher<T> with PBKDF2
   - Never store plain-text passwords

2. **CSRF Protection**
   - [ValidateAntiForgeryToken] on all POST/PUT/DELETE
   - @Html.AntiForgeryToken() in all forms

3. **Secure Cookies**
   - HttpOnly = true (prevents JavaScript access)
   - Secure = true (HTTPS only in production)
   - SameSite = Strict (prevents cross-site requests)

4. **Session Management**
   - Configurable expiration (8 hours default)
   - Sliding expiration enabled
   - Login/Logout paths defined

5. **Role-Based Access Control (RBAC)**
   - [Authorize(Roles="")] attributes
   - Resource-level authorization checks
   - Claim-based identity

6. **Input Validation**
   - DataAnnotations on models
   - Server-side validation
   - ModelState.IsValid checks

### 🔐 Additional Recommendations

1. **HTTPS Enforcement**
   ```csharp
   app.UseHsts(); // HTTP Strict Transport Security
   app.UseHttpsRedirection();
   ```

2. **Rate Limiting** (for login attempts)
   ```csharp
   // Implement custom middleware or use packages
   ```

3. **Audit Logging**
   - Log authentication attempts
   - Log authorization failures
   - Track sensitive operations

4. **Password Requirements**
   ```
   - Minimum 8 characters
   - Uppercase letter
   - Number
   - Special character
   ```

5. **Account Lockout** (after failed attempts)
   ```csharp
   int failedAttempts = 0;
   if (failedAttempts >= 5)
   {
       user.IsActive = false; // Lock account
   }
   ```

---

## Testing Authentication

### Manual Testing Scenarios

1. **Register New Account**
   - POST /Auth/Register with form data
   - Verify user created in database
   - Verify password hashed

2. **Login Valid Credentials**
   - POST /Auth/Login with correct credentials
   - Verify cookie set
   - Verify redirected to dashboard

3. **Login Invalid Credentials**
   - POST /Auth/Login with wrong password
   - Verify error message displayed
   - Verify cookie NOT set

4. **Access Protected Route**
   - Logged in: Can access [Authorize] routes
   - Logged out: Redirected to /Auth/Login

5. **Role-Based Access**
   - Admin: Can access /Users/Index
   - Client: Cannot access /Users/Index (Forbid())

6. **Session Expiration**
   - Wait 8 hours (or modify config)
   - Verify user logged out automatically
   - Verify redirected to login

---

## Connection String Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-mysql-server;Database=itsms_db;User=root;Password=your_password;"
  }
}
```

### appsettings.Development.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=itsms_dev;User=root;Password=dev_password;"
  }
}
```

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Cookie not set | Check UseAuthentication() in middleware pipeline |
| [Authorize] not working | Verify DbContext configuration and claims |
| Password verification fails | Ensure using same PasswordHasher instance |
| CSRF token invalid | Verify @Html.AntiForgeryToken() in form |
| Claims empty | Check if claimsIdentity created correctly |
| Role authorization fails | Verify ClaimTypes.Role claim is present |

