# Complete IT Service Management System - Implementation Guide

## 📋 Table of Contents

1. [System Overview](#system-overview)
2. [Architecture](#architecture)
3. [Project Structure](#project-structure)
4. [Database Setup](#database-setup)
5. [Entity Models](#entity-models)
6. [DbContext Configuration](#dbcontext-configuration)
7. [Controllers](#controllers)
8. [Authentication & Authorization](#authentication--authorization)
9. [Views (Razor)](#views-razor)
10. [Setup Instructions](#setup-instructions)
11. [Testing Scenarios](#testing-scenarios)
12. [Deployment](#deployment)
13. [Optional Features](#optional-features)
14. [Troubleshooting](#troubleshooting)

---

## System Overview

### Purpose
A complete IT Service Management System (IT-SMS) for managing, tracking, and resolving IT service requests.

### Key Features
- 🎫 **Ticketing System** - Create and track service requests
- 👥 **Role-Based Access** - Admin, Technician, Client roles
- 📊 **Assignment Management** - Assign technicians to requests
- ⭐ **Feedback System** - Customer satisfaction ratings
- 📈 **Analytics & Reports** - Complete dashboard and analytics
- 🔐 **Authentication** - Cookie-based secure login

### Technology Stack
- **Backend**: ASP.NET Core MVC (.NET 9)
- **Database**: MySQL with Entity Framework Core (Pomelo)
- **Frontend**: Razor Views (MVC)
- **Authentication**: Cookie-based + ASP.NET Identity PasswordHasher
- **UI Framework**: Bootstrap 5
- **Hosting**: MonsterASP.NET (can be deployed anywhere)

---

## Architecture

### System Architecture Diagram

```
┌─────────────────────────────────────────────────────────┐
│                    Browser (Client)                     │
└──────────────────────┬──────────────────────────────────┘
                       │ HTTPS
┌──────────────────────▼──────────────────────────────────┐
│            ASP.NET Core MVC Application                │
├──────────────────────────────────────────────────────────┤
│   Controllers Layer                                     │
│   - AuthController (Login/Register)                     │
│   - ServiceRequestsController                           │
│   - AssignmentsController                               │
│   - FeedbackController                                  │
│   - ReportsController                                   │
│   - UsersController (Admin)                             │
├──────────────────────────────────────────────────────────┤
│   Business Logic / Services Layer                       │
│   - AuthenticationService                               │
│   - ServiceRequestService (Optional)                    │
├──────────────────────────────────────────────────────────┤
│   Data Access Layer (Entity Framework Core)             │
│   - DbContext (ApplicationDbContext)                    │
│   - Entity Models                                       │
│   - DbSets for all tables                               │
├──────────────────────────────────────────────────────────┤
│   Security Layer                                        │
│   - Cookie Authentication                               │
│   - Claims-based Authorization                          │
│   - Anti-Forgery Tokens (CSRF)                          │
└──────────────────────────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────┐
│            MySQL Database Server                        │
├──────────────────────────────────────────────────────────┤
│   Tables:                                               │
│   - Users, Roles                                        │
│   - ServiceRequests, Categories                         │
│   - Assignments, Feedback                               │
│   - ActivityLog                                         │
└──────────────────────────────────────────────────────────┘
```

---

## Project Structure

```
ITSMS/
├── Controllers/
│   ├── AuthController.cs              # Login/Register
│   ├── ServiceRequestsController.cs    # Ticket Management
│   ├── AssignmentsController.cs        # Assignment Logic
│   ├── FeedbackController.cs           # Feedback System
│   ├── ReportsController.cs            # Analytics
│   ├── UsersController.cs              # User Management
│   └── HomeController.cs               # Homepage
│
├── Models/
│   ├── User.cs                         # User entity
│   ├── Role.cs                         # Role entity
│   ├── ServiceRequest.cs               # Ticket entity
│   ├── Category.cs                     # Category entity
│   ├── Assignment.cs                   # Assignment entity
│   ├── Feedback.cs                     # Feedback entity
│   ├── ActivityLog.cs                  # Audit logs
│   └── ErrorViewModel.cs
│
├── Data/
│   └── ApplicationDbContext.cs         # EF Core DbContext
│
├── Services/
│   └── IAuthenticationService.cs       # Auth service interface
│
├── Views/
│   ├── Auth/
│   │   ├── Login.cshtml
│   │   └── Register.cshtml
│   │
│   ├── ServiceRequests/
│   │   ├── Index.cshtml                # List requests
│   │   ├── Details.cshtml              # View request
│   │   ├── Create.cshtml               # New request
│   │   └── Edit.cshtml                 # Update request
│   │
│   ├── Feedback/
│   │   └── Create.cshtml               # Feedback form
│   │
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   ├── AccessDenied.cshtml
│   │   └── Error.cshtml
│   │
│   └── Home/
│       ├── Index.cshtml
│       └── Privacy.cshtml
│
├── wwwroot/
│   ├── css/
│   └── js/
│
├── Program.cs                          # Configuration
├── appsettings.json                    # Settings
├── appsettings.Development.json
└── ITSMS.csproj                        # Project file
```

---

## Database Setup

### Prerequisites
- MySQL Server 5.7+ or MySQL 8.0+
- MySQL Workbench (optional, for database management)

### Database Creation

```sql
-- Create database
CREATE DATABASE itsms_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Use database
USE itsms_db;

-- Tables will be created automatically by Entity Framework migrations
```

### Connection String

**appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=itsms_db;User=root;Password=your_password;Port=3306;"
  }
}
```

### Applying Migrations

```bash
# Create initial migration
dotnet ef migrations add InitialCreate --context ApplicationDbContext

# Apply to database
dotnet ef database update
```

---

## Entity Models

### User Model
```csharp
public class User
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public int RoleId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation
    public Role Role { get; set; }
    public string FullName => $"{FirstName} {LastName}";
}
```

### ServiceRequest Model
```csharp
public class ServiceRequest
{
    public int RequestId { get; set; }
    public string RequestNumber { get; set; }      // REQ-001, REQ-002, etc.
    public string Title { get; set; }
    public string Description { get; set; }
    public int CategoryId { get; set; }
    public int RequestorId { get; set; }           // Who created it
    public int? AssignedTechnicianId { get; set; } // Who is assigned
    public ServiceRequestStatus Status { get; set; }
    public ServiceRequestPriority Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    
    // Navigation
    public Category Category { get; set; }
    public User Requestor { get; set; }
    public User AssignedTechnician { get; set; }
    public ICollection<Assignment> Assignments { get; set; }
    public Feedback Feedback { get; set; }
}

// ENUMs
public enum ServiceRequestStatus
{
    Open, InProgress, OnHold, Resolved, Closed
}

public enum ServiceRequestPriority
{
    Low, Medium, High, Critical
}
```

### Other Models
See [DATABASE_DESIGN.md](./DATABASE_DESIGN.md) for complete model documentation.

---

## DbContext Configuration

### ApplicationDbContext

```csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options) { }

    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<ServiceRequest> ServiceRequests { get; set; }
    public DbSet<Assignment> Assignments { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Relationships and constraints configured in Fluent API
        // See Data/ApplicationDbContext.cs for complete configuration
    }
}
```

**Key Configuration:**
- ✅ Foreign key relationships
- ✅ Cascading deletes where appropriate
- ✅ Unique constraints (Username, Email, RequestNumber)
- ✅ Indexes for performance
- ✅ Default values and seed data

---

## Controllers

### AuthController - Authentication
**Location**: `Controllers/AuthController.cs`

**Actions:**
- `GET /Auth/Login` - Display login form
- `POST /Auth/Login` - Process login
- `GET /Auth/Register` - Display registration form
- `POST /Auth/Register` - Process registration
- `GET /Auth/Logout` - Sign out user

**Key Features:**
- Password hashing using PasswordHasher<T>
- Cookie authentication setup
- Claims-based identity creation

---

### ServiceRequestsController - Core Ticketing
**Location**: `Controllers/ServiceRequestsController.cs`

**Actions:**
| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| GET | `/ServiceRequests` | List requests (filtered by role) | ✓ |
| GET | `/ServiceRequests/Details/{id}` | View request details | ✓ |
| GET | `/ServiceRequests/Create` | Display create form | Client, Admin |
| POST | `/ServiceRequests/Create` | Create new request | Client, Admin |
| GET | `/ServiceRequests/Edit/{id}` | Display edit form | Tech, Admin |
| POST | `/ServiceRequests/Edit/{id}` | Update request | Tech, Admin |
| GET | `/ServiceRequests/Close/{id}` | Display close form | Tech, Admin |
| POST | `/ServiceRequests/Close/{id}` | Close request | Tech, Admin |

---

### AssignmentsController - Technician Assignment
**Location**: `Controllers/AssignmentsController.cs`

**Actions:**
| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| GET | `/Assignments/Assign/{requestId}` | Display assign form | Admin |
| POST | `/Assignments/Assign/{requestId}` | Assign technician | Admin |
| GET | `/Assignments/Workload` | Technician workload view | Admin |
| GET | `/Assignments/History/{requestId}` | Assignment history | Admin |

---

### FeedbackController - Customer Feedback
**Location**: `Controllers/FeedbackController.cs`

**Actions:**
| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| GET | `/Feedback/Create/{requestId}` | Feedback form | Client |
| POST | `/Feedback/Create/{requestId}` | Submit feedback | Client |
| GET | `/Feedback/Edit/{id}` | Edit feedback | Client |
| POST | `/Feedback/Edit/{id}` | Update feedback | Client |
| GET | `/Feedback/Statistics` | Feedback analytics | Admin |

---

### ReportsController - Analytics & Dashboard
**Location**: `Controllers/ReportsController.cs`

**Actions:**
| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| GET | `/Reports/Dashboard` | Main dashboard | Admin |
| GET | `/Reports/TechnicianWorkload` | Workload report | Admin, Tech |
| GET | `/Reports/CategoryAnalysis` | Category breakdown | Admin |
| GET | `/Reports/PriorityAnalysis` | Priority breakdown | Admin |
| GET | `/Reports/CustomerSatisfaction` | Feedback analysis | Admin |
| GET | `/Reports/ResponseTimeAnalysis` | Resolution times | Admin |

---

### UsersController - User Management
**Location**: `Controllers/UsersController.cs`

**Actions:**
| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| GET | `/Users` | List all users | Admin |
| GET | `/Users/Details/{id}` | User details | Admin |
| GET | `/Users/Create` | Create form | Admin |
| POST | `/Users/Create` | Create user | Admin |
| GET | `/Users/Edit/{id}` | Edit form | Admin |
| POST | `/Users/Edit/{id}` | Update user | Admin |
| GET | `/Users/Deactivate/{id}` | Deactivate form | Admin |
| POST | `/Users/Deactivate/{id}` | Deactivate user | Admin |
| POST | `/Users/Reactivate/{id}` | Reactivate user | Admin |

---

## Authentication & Authorization

### Authentication Flow

1. **Registration**
   ```
   User fills form → Validate → Hash password → Save to DB → Navigate to login
   ```

2. **Login**
   ```
   User submits credentials → Find user → Verify password → 
   Create claims → Sign cookie → Set HttpContext.User → Redirect to dashboard
   ```

3. **Protected Routes**
   ```
   Request arrives → Check cookie → Extract claims → 
   Check [Authorize] attribute → Check role if specified → Allow/Deny
   ```

4. **Logout**
   ```
   User clicks logout → Destroy cookie → Clear session → Redirect to home
   ```

### Role-Based Access Control

| Feature | Admin | Technician | Client |
|---------|-------|-----------|--------|
| View all requests | ✓ | Assigned only | Own only |
| Create request | ✓ | ✗ | ✓ |
| Update request | ✓ | ✓ | ✗ |
| Assign technician | ✓ | ✗ | ✗ |
| Provide feedback | ✗ | ✗ | ✓ |
| View reports | ✓ | ✓ | ✗ |
| Manage users | ✓ | ✗ | ✗ |

### Implementation

```csharp
// Protect entire controller
[Authorize]
public class ServiceRequestsController { }

// Protect specific role
[Authorize(Roles = "Admin")]
public IActionResult ManageUsers() { }

// Multiple roles
[Authorize(Roles = "Admin,Technician")]
public IActionResult ViewReports() { }

// Resource-based authorization
private bool CanViewRequest(ServiceRequest request)
{
    var userId = GetCurrentUserId();
    var role = User.FindFirst(ClaimTypes.Role)?.Value;
    
    return role == "Admin" || 
           (role == "Client" && request.RequestorId == userId) ||
           (role == "Technician" && request.AssignedTechnicianId == userId);
}
```

---

## Views (Razor)

### Folder Structure

```
Views/
├── Auth/
│   ├── Login.cshtml          (Authentication form)
│   └── Register.cshtml       (Registration form)
├── ServiceRequests/
│   ├── Index.cshtml          (List all requests)
│   ├── Details.cshtml        (View single request)
│   ├── Create.cshtml         (New request form)
│   └── Edit.cshtml           (Update request form)
├── Feedback/
│   └── Create.cshtml         (Feedback form)
├── Shared/
│   ├── _Layout.cshtml        (Master layout)
│   ├── AccessDenied.cshtml   (Permission denied)
│   └── Error.cshtml          (Error page)
└── Home/
    ├── Index.cshtml          (Dashboard)
    └── Privacy.cshtml
```

### Sample View - Index.cshtml

```html
@model List<ITSMS.Models.ServiceRequest>

<div class="container-fluid mt-4">
    <h2>Service Requests</h2>
    
    <table class="table table-hover">
        <thead>
            <tr>
                <th>Request #</th>
                <th>Title</th>
                <th>Status</th>
                <th>Priority</th>
                <th>Actions</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var request in Model)
            {
                <tr>
                    <td>@request.RequestNumber</td>
                    <td>@request.Title</td>
                    <td><span class="badge bg-primary">@request.Status</span></td>
                    <td>@request.Priority</td>
                    <td>
                        <a href="@Url.Action("Details", new { id = request.RequestId })" 
                           class="btn btn-sm btn-primary">View</a>
                    </td>
                </tr>
            }
        </tbody>
    </table>
</div>
```

---

## Setup Instructions

### 1. Prerequisites
- Visual Studio 2022 or VS Code
- .NET 9 SDK
- MySQL Server 5.7+
- Git (optional)

### 2. Clone/Create Project

```bash
# Create new ASP.NET Core MVC project
dotnet new mvc -n ITSMS

cd ITSMS
```

### 3. Install NuGet Packages

```bash
# Entity Framework Core
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Pomelo.EntityFrameworkCore.MySql

# Authentication
dotnet add package Microsoft.AspNetCore.Authentication.Cookies
dotnet add package Microsoft.AspNetCore.Identity

# UI
dotnet add package Bootstrap
dotnet add package jquery-validation-unobtrusive
```

### 4. Configure Connection String

**appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=itsms_db;User=root;Password=your_password;"
  }
}
```

### 5. Update Program.cs

Copy the configuration from [PROGRAM_CS_CONFIGURATION.md](./PROGRAM_CS_CONFIGURATION.md)

### 6. Create Models

Add all model classes from `Models/` folder

### 7. Create DbContext

Add `Data/ApplicationDbContext.cs`

### 8. Create Migrations

```bash
# Initial migration
dotnet ef migrations add InitialCreate

# Apply to database
dotnet ef database update
```

### 9. Create Controllers

Add all controller classes from the template

### 10. Create Views

Add all Razor views from the template

### 11. Run Application

```bash
dotnet run

# Access at https://localhost:5001
```

---

## Testing Scenarios

### Test Case 1: User Registration & Login

**Steps:**
1. Navigate to `/Auth/Register`
2. Fill in registration form
   - Username: `testuser`
   - Email: `test@example.com`
   - Password: `TestPass123!`
   - First Name: `John`
   - Last Name: `Doe`
3. Click Register
4. Navigate to `/Auth/Login`
5. Enter credentials
6. Verify logged in (user menu visible)

**Expected Result**: ✓ User created and logged in successfully

---

### Test Case 2: Create Service Request (Client)

**Precondition**: Logged in as Client

**Steps:**
1. Navigate to `/ServiceRequests`
2. Click "New Request"
3. Fill form:
   - Title: `Cannot connect to network`
   - Category: `Network`
   - Description: `My computer cannot connect to company network`
   - Priority: `High`
4. Click Submit
5. Verify request created (see RequestNumber)

**Expected Result**: ✓ Request created with unique number (REQ-000001)

---

### Test Case 3: Assign Request (Admin)

**Precondition**: Request created, logged in as Admin

**Steps:**
1. Navigate to request Details
2. Click "Assign Technician"
3. Select technician from dropdown
4. Add optional notes
5. Click Submit
6. Verify request now assigned

**Expected Result**: ✓ Assignment created, request status changed to "In Progress"

---

### Test Case 4: Provide Feedback (Client)

**Precondition**: Request resolved, logged in as requestor

**Steps:**
1. Navigate to resolved request
2. Click "Provide Feedback"
3. Rate 1-5 stars
4. Add comments
5. Click Submit
6. Verify feedback appears on request

**Expected Result**: ✓ Feedback created and displayed

---

### Test Case 5: Access Control

**Steps:**
1. Log in as Client
2. Try to access `/Users` (Admin only)
3. Verify Access Denied page

**Expected Result**: ✓ Client cannot access admin pages

---

## Deployment

### Deployment to MonsterASP.NET

1. **Build for Production**
   ```bash
   dotnet publish -c Release -o ./publish
   ```

2. **Prepare for Upload**
   - Zip the `publish` folder
   - Upload to MonsterASP.NET FTP

3. **Configure on Server**
   - Set Application Pool to .NET 9
   - Configure connection string
   - Set web.config (if needed)
   - Enable HTTPS

4. **Database Preparation**
   - Create MySQL database on server
   - Run migrations: `dotnet ef database update`

5. **Test**
   - Access deployed application
   - Test login and core functionality

---

## Optional Features

### 1. Email Notifications
Send email when request created/updated/assigned

```csharp
// Use SendGrid or SMTP
public interface IEmailService
{
    Task SendAsync(string to, string subject, string body);
}
```

### 2. Dashboard Charts
Visualize statistics with Chart.js

```html
<script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
<canvas id="requestsChart"></canvas>
```

### 3. Activity Audit Log
Track all user actions

```csharp
public class ActivityLog
{
    public int LogId { get; set; }
    public int UserId { get; set; }
    public string Entity { get; set; }
    public string Action { get; set; }
    public DateTime LoggedAt { get; set; }
}
```

### 4. File Attachments
Allow users to upload files with requests

```csharp
public class Attachment
{
    public int AttachmentId { get; set; }
    public int RequestId { get; set; }
    public string FileName { get; set; }
    public byte[] FileContent { get; set; }
    public string ContentType { get; set; }
}
```

### 5. Priority Escalation
Auto-escalate old requests

```csharp
// Escalate requests older than 24 hours at Low/Medium
if ((DateTime.UtcNow - request.CreatedAt).TotalHours > 24)
{
    request.Priority = ServiceRequestPriority.High;
}
```

### 6. SLA Tracking
Track Service Level Agreements

```csharp
public class SLA
{
    public int SLAId { get; set; }
    public ServiceRequestPriority Priority { get; set; }
    public int ResolutionTimeHours { get; set; } // 4 hrs for Critical, etc.
}
```

---

## Troubleshooting

### Issue: DbContext not found
**Solution**: Ensure ApplicationDbContext registered in Program.cs
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ...));
```

### Issue: Migration fails
**Solution**: Check database connection string and MySQL is running
```bash
# Verify connection
dotnet ef dbcontext validate
```

### Issue: Authentication cookie not set
**Solution**: Ensure middleware order is correct in Program.cs
```csharp
app.UseAuthentication();  // BEFORE UseAuthorization
app.UseAuthorization();
```

### Issue: Claims are empty
**Solution**: Verify ClaimsIdentity created correctly at login
```csharp
var claimsIdentity = new ClaimsIdentity(
    claims, 
    CookieAuthenticationDefaults.AuthenticationScheme
);
```

### Issue: CSRF token validation fails
**Solution**: Include @Html.AntiForgeryToken() in all forms
```html
<form method="post">
    @Html.AntiForgeryToken()
    <!-- fields -->
</form>
```

### Issue: Foreign key constraint error
**Solution**: Check cascading deletes in DbContext configuration
```csharp
entity.HasOne(sr => sr.Category)
    .WithMany()
    .HasForeignKey(sr => sr.CategoryId)
    .OnDelete(DeleteBehavior.Restrict);
```

---

## Performance Optimization Tips

1. **Add Indexes** on frequently queried columns:
   ```csharp
   entity.HasIndex(sr => sr.Status);
   entity.HasIndex(sr => sr.RequestorId);
   ```

2. **Use Pagination** for large data sets:
   ```csharp
   requests = requests.Skip((page - 1) * pageSize).Take(pageSize);
   ```

3. **Eager Load** related data:
   ```csharp
   .Include(sr => sr.Category)
   .Include(sr => sr.Requestor)
   .Include(sr => sr.AssignedTechnician)
   ```

4. **Cache** regularly accessed data:
   ```csharp
   builder.Services.AddMemoryCache();
   ```

5. **Monitor** database queries:
   ```csharp
   options.LogTo(Console.WriteLine); // Development only
   ```

---

## Security Checklist

- [ ] Passwords hashed with PasswordHasher
- [ ] Anti-forgery tokens on all forms
- [ ] Input validation on all models
- [ ] HTTPS enforced in production
- [ ] Secure cookie flags set (HttpOnly, Secure, SameSite)
- [ ] Role-based authorization on controllers
- [ ] Resource-based authorization checks
- [ ] SQL injection prevention (using EF Core)
- [ ] XSS prevention (Razor encoder)
- [ ] CORS configured (if needed)
- [ ] Error messages don't expose system details
- [ ] Sensitive data not logged

---

## Grade Enhancement Suggestions

### For B+ / A grade:
1. ✅ Complete RBAC implementation
2. ✅ Audit trail (ActivityLog)
3. ✅ Dashboard with charts
4. ✅ Email notifications
5. ✅ File attachments
6. ✅ SLA tracking
7. ✅ Advanced search/filtering
8. ✅ Export to CSV/PDF

### For A / A+ grade:
1. ✅ All of above +
2. ✅ API endpoints (Web API)
3. ✅ Mobile-responsive design
4. ✅ Real-time notifications (SignalR)
5. ✅ Advanced reporting with visualizations
6. ✅ Workflow automation scripts
7. ✅ Performance optimization
8. ✅ Unit tests & integration tests
9. ✅ CI/CD pipeline
10. ✅ Comprehensive documentation

---

## Contact & Support

For issues or questions:
1. Check [Troubleshooting](#troubleshooting) section
2. Review ASP.NET Core documentation
3. Check Entity Framework Core guide
4. Review database design document

---

## References

- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [Pomelo MySQL Provider](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql)
- [Bootstrap Documentation](https://getbootstrap.com/)
- [OWASP Security Guidelines](https://owasp.org/)

---

**Document Version**: 1.0  
**Last Updated**: 2024  
**System Version**: IT-SMS v1.0

