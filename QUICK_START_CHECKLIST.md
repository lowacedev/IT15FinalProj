# IT Service Management System - Quick Start Checklist

## Pre-Implementation Requirements ✓

- [ ] .NET 9 SDK installed
- [ ] Visual Studio 2022 / VS Code installed
- [ ] MySQL Server 5.7+ installed and running
- [ ] Git installed (optional)

---

## Step 1: Project Setup (15 mins)

- [ ] Create new ASP.NET Core MVC project (net9.0)
- [ ] Install NuGet packages:
  - [ ] EntityFrameworkCore
  - [ ] EntityFrameworkCore.Design
  - [ ] Pomelo.EntityFrameworkCore.MySql
  - [ ] AspNetCore.Authentication.Cookies
  - [ ] AspNetCore.Identity
- [ ] Configure MySQL connection string in `appsettings.json`

---

## Step 2: Create Database & Models (30 mins)

### Create Models folder and add:
- [ ] Role.cs
- [ ] User.cs
- [ ] Category.cs
- [ ] ServiceRequest.cs
- [ ] Assignment.cs
- [ ] Feedback.cs
- [ ] ActivityLog.cs

### Create Data folder and add:
- [ ] ApplicationDbContext.cs (with all DbSets and Fluent API config)

### Create migrations:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

## Step 3: Authentication & Services (20 mins)

### Create Services folder and add:
- [ ] IAuthenticationService.cs

### Update Program.cs:
- [ ] Add DbContext
- [ ] Add Authentication (Cookies)
- [ ] Add Authorization
- [ ] Add Session (optional)
- [ ] Configure middleware (UseAuthentication, UseAuthorization)

---

## Step 4: Create Controllers (45 mins)

Replace/Add to `Controllers/`:
- [ ] AuthController.cs (login/register/logout)
- [ ] ServiceRequestsController.cs (CRUD + status management)
- [ ] AssignmentsController.cs (assign technicians)
- [ ] FeedbackController.cs (customer feedback)
- [ ] ReportsController.cs (analytics/dashboard)
- [ ] UsersController.cs (user management - admin only)

---

## Step 5: Create Views (1 hour)

### Create Auth Views (`Views/Auth/`):
- [ ] Login.cshtml
- [ ] Register.cshtml

### Create ServiceRequests Views (`Views/ServiceRequests/`):
- [ ] Index.cshtml (list)
- [ ] Details.cshtml (view single)
- [ ] Create.cshtml (new request form)
- [ ] Edit.cshtml (update status)

### Create Feedback Views (`Views/Feedback/`):
- [ ] Create.cshtml (feedback form)

### Create Shared Views (`Views/Shared/`):
- [ ] AccessDenied.cshtml
- [ ] Update _Layout.cshtml (add auth menu)

---

## Step 6: Update Home Controller & Views (20 mins)

### HomeController.cs:
```csharp
public IActionResult Index()
{
    // Add role-based dashboard routing
    if (User.IsInRole("Admin"))
        return RedirectToAction("Dashboard", "Reports");
    
    return View();
}
```

### Views/Home/Index.cshtml:
- [ ] Add welcome message
- [ ] Add links to appropriate dashboard

---

## Step 7: Testing & Verification (30 mins)

### Run Application:
```bash
dotnet run
```

### Test User Flows:

#### 1. Registration & Login
- [ ] Register new account at `/Auth/Register`
- [ ] Login at `/Auth/Login`
- [ ] Verify session created
- [ ] Logout and verify session cleared

#### 2. Create Service Request (as Client)
- [ ] Login as Client
- [ ] Navigate to `/ServiceRequests`
- [ ] Click "New Request"
- [ ] Fill form and submit
- [ ] Verify RequestNumber generated (REQ-XXXXXX)

#### 3. Assign Request (as Admin)
- [ ] Login as Admin
- [ ] Go to request details
- [ ] Click "Assign Technician"
- [ ] Select technician and assign
- [ ] Verify status changed to "In Progress"

#### 4. Provide Feedback (as Client)
- [ ] Login as original requestor
- [ ] Navigate to resolved request
- [ ] Click "Provide Feedback"
- [ ] Submit 1-5 star rating
- [ ] Verify feedback displayed

#### 5. Access Control
- [ ] Test that Client cannot access `/Users` → Should see Access Denied
- [ ] Test that Client cannot access `/Reports` → Should see Access Denied
- [ ] Test that Technician can access `/Reports/TechnicianWorkload`

---

## Step 8: Deployment Preparation (20 mins)

### Build for Release:
```bash
dotnet publish -c Release -o ./publish
```

### Create Deployment Package:
- [ ] Zip `/publish` folder
- [ ] Note down connection string
- [ ] Prepare database on hosting server

### Deploy to MonsterASP.NET:
- [ ] Upload to FTP
- [ ] Configure connection string
- [ ] Enable HTTPS
- [ ] Test on live server

---

## Step 9: Post-Implementation (As Needed)

### Optional Features:
- [ ] Add email notifications (SMTP)
- [ ] Add file attachments
- [ ] Add activity audit logs
- [ ] Add Chart.js dashboards
- [ ] Add search/filtering
- [ ] Add pagination
- [ ] Add export to CSV/PDF

### Security Enhancements:
- [ ] Add rate limiting
- [ ] Add account lockout
- [ ] Add password history
- [ ] Add 2FA (optional)
- [ ] Add GDPR compliance logging

### Performance Optimization:
- [ ] Add database indexes
- [ ] Add query optimization
- [ ] Add caching strategy
- [ ] Test for N+1 queries
- [ ] Monitor slow queries

---

## Test Users

Use these for initial testing:

### Admin User
- Username: `admin`
- Email: `admin@itsms.local`
- Password: `AdminPass123!`
- Role: Admin

### Technician User
- Username: `technician1`
- Email: `tech1@itsms.local`
- Password: `TechPass123!`
- Role: Technician

### Client User
- Username: `client1`
- Email: `client1@itsms.local`
- Password: `ClientPass123!`
- Role: Client

**Note**: Create these manually through registration or insert directly into database:

```sql
-- Insert roles
INSERT INTO Roles (RoleName, Description) VALUES 
('Admin', 'Administrator'),
('Technician', 'Support Technician'),
('Client', 'End User');

-- Insert admin user (password hash example - generate new one)
INSERT INTO Users (Username, Email, PasswordHash, FirstName, LastName, RoleId, IsActive, CreatedAt)
VALUES ('admin', 'admin@itsms.local', '[HASHED_PASSWORD]', 'Admin', 'User', 1, 1, NOW());
```

---

## Troubleshooting Checklist

| Issue | Check | Solution |
|-------|-------|----------|
| DbContext error | Connection string | Verify in appsettings.json |
| Migration fails | MySQL running | Start MySQL and try again |
| Login fails | Database | Check Users table exists |
| Cookies not set | Program.cs | Verify middleware order |
| 404 errors | Routes | Check controller names match |
| Claims empty | AuthController | Verify ClaimsIdentity creation |
| CSRF error | Form | Add @Html.AntiForgeryToken() |
| Access denied | Authorization | Check [Authorize] attribute |

---

## Expected Folder Structure After Setup

```
ITSMS/
├── Controllers/ (6 controllers)
├── Models/ (7 models)
├── Data/ (ApplicationDbContext.cs)
├── Services/ (IAuthenticationService.cs)
├── Views/
│   ├── Auth/ (2 views)
│   ├── ServiceRequests/ (4 views)
│   ├── Feedback/ (1 view)
│   ├── Shared/ (layouts + error pages)
│   └── Home/ (2 views)
├── wwwroot/
├── Program.cs (configured)
├── appsettings.json (configured)
├── appsettings.Development.json (configured)
└── ITSMS.csproj

Estimated: ~20 C# files + ~10 Razor views + configuration files
```

---

## Time Estimates

| Phase | Duration | Notes |
|-------|----------|-------|
| Setup & Config | 30 min | NPM packages, connection string |
| Database & Models | 45 min | Includes migrations |
| Controllers | 60 min | All 6 controllers |
| Views | 90 min | All Razor templates |
| Testing | 45 min | User flow testing |
| Fixes & Polish | 30 min | Bug fixes, styling |
| **Total** | **~5 hours** | For core system |

**Add 2-3 hours for optional features (email, charts, etc.)**

---

## Validation Checklist - Before Submission

### Functionality
- [ ] User can register
- [ ] User can login
- [ ] Client can create service request
- [ ] Admin can view all requests
- [ ] Admin can assign technician
- [ ] Technician can update status
- [ ] Client can provide feedback
- [ ] Reports work for Admin
- [ ] Logout works
- [ ] Role-based access control enforced

### Security
- [ ] Passwords are hashed
- [ ] Anti-forgery tokens present
- [ ] HTTPS configured (production)
- [ ] Secure cookies set
- [ ] Authorization attributes in place
- [ ] No sensitive data in logs
- [ ] Input validation working

### Database
- [ ] All tables created
- [ ] Relationships intact
- [ ] Constraints enforced
- [ ] Indexes created
- [ ] Sample data populated
- [ ] Migrations tracked

### Code Quality
- [ ] Code follows C# conventions
- [ ] No hardcoded values
- [ ] Error handling implemented
- [ ] Comments added where needed
- [ ] Consistent naming
- [ ] No unused code

### UI/UX
- [ ] Responsive design (mobile-friendly)
- [ ] Clear navigation
- [ ] Error messages helpful
- [ ] Success messages shown
- [ ] Status indicators visible
- [ ] Consistent styling

### Documentation
- [ ] README created
- [ ] Database schema documented
- [ ] API/Controller documentation
- [ ] Setup instructions clear
- [ ] Deployment steps documented

---

## Grade Improvement Strategy

### For B Grade (Current):
- ✓ Complete core implementation
- ✓ All basic CRUD operations
- ✓ Authentication & authorization
- ✓ Role-based access control
- ✓ Professional UI with Bootstrap

### For B+ / A Grade:
- ✓ Above +
- ✓ Advanced reporting & analytics
- ✓ Email notifications
- ✓ Audit logging
- ✓ File attachments
- ✓ Export functionality (CSV/PDF)
- ✓ Performance optimization
- ✓ Comprehensive documentation

### For A+ Grade:
- ✓ All above +
- ✓ Web API (REST)
- ✓ Real-time updates (SignalR)
- ✓ Advanced filtering & search
- ✓ Mobile app version or responsive PWA
- ✓ Unit tests (50%+ coverage)
- ✓ CI/CD pipeline
- ✓ Desktop client (optional)

---

## Support Resources

1. **Official Documentation**
   - ASP.NET Core: https://docs.microsoft.com/aspnet/core
   - EF Core: https://docs.microsoft.com/ef/core
   - Razor: https://docs.microsoft.com/aspnet/mvc/views/razor

2. **Learning Resources**
   - Microsoft Learn ASP.NET Core modules
   - Ultimate ASP.NET MVC course on Udemy
   - Stack Overflow (tag: asp.net-core)

3. **Tools**
   - Visual Studio Debugger for step-through debugging
   - MySQL Workbench for database review
   - Postman for API testing
   - BrowserLink for live reload

---

## Quick Reference Commands

```bash
# Create project
dotnet new mvc -n ITSMS

# Restore packages
dotnet restore

# Build
dotnet build

# Run
dotnet run

# Create migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Drop database
dotnet ef database drop

# Publish
dotnet publish -c Release -o ./publish

# Run migrations on server
dotnet ef database update --project ITSMS.csproj
```

---

✅ **You are now ready to implement the IT Service Management System!**

Start with Step 1 and follow through systematically. Take breaks as needed, and refer back to the detailed guides for specific sections.

Good luck! 🚀

