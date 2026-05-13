# ITSMS Backend - Quick Reference Guide

## 📌 OVERVIEW
IT Service Management System built with ASP.NET Core MVC featuring service ticketing, technician assignment, feedback, and analytics.

---

## 🎯 KEY API FUNCTIONS

### Authentication Module
| Function | Endpoint | Authorization | Purpose |
|----------|----------|---|---------|
| Login | POST /Auth/Login | Public | User authentication with reCAPTCHA |
| Register | POST /Auth/Register | Public | New account creation |
| Logout | GET /Auth/Logout | [Authorize] | Session termination |

**Security:** PBKDF2 password hashing, Cookie-based auth, 8-hour session timeout

---

### Service Request Management
| Function | Endpoint | Authorization | Purpose |
|----------|----------|---|---------|
| Create Request | POST /ServiceRequests/Create | Employee,Admin | Submit new IT ticket |
| List Requests | GET /ServiceRequests/Index | [Authorize] | View requests (role-filtered) |
| View Details | GET /ServiceRequests/Details/{id} | [Authorize] | Full request details + history |
| Update Status | POST /ServiceRequests/Edit/{id} | [Authorize] | Change status/priority |
| Close Request | POST /ServiceRequests/Close/{id} | [Authorize] | Mark request as closed |

**Data Model:**
```
RequestId | RequestNumber | Title | Description | Category | Priority | Status | AssignedTechnicianId | CreatedAt
1         | REQ-000001   | Title | Details... | Hardware | High    | Pending | null                | 2024-05-07
```

**Status Enum:** Pending → InProgress → Resolved → Closed (or OnHold)
**Priority Enum:** Low | Medium | High | Critical

---

### Technician Assignment
| Function | Endpoint | Authorization | Purpose |
|----------|----------|---|---------|
| Assign Tech | POST /Assignments/Assign/{id} | Admin | Assign technician to request |
| View Workload | GET /Assignments/Workload | Admin,Technician | Tech capacity metrics |
| Assignment History | GET /Assignments/History/{id} | [Authorize] | Track all assignments |

**Workload Metrics:**
- Pending requests per tech
- In-progress count
- Resolved count
- Total assigned

---

### Customer Feedback
| Function | Endpoint | Authorization | Purpose |
|----------|----------|---|---------|
| Submit Feedback | POST /Feedback/Create/{id} | Employee | Rate request (1-5 stars) |
| View Statistics | GET /Feedback/Statistics | Admin | Satisfaction analytics |

**Feedback Data:**
- Rating (1-5)
- Comments (optional)
- ProvidedBy (user)
- ProvidedAt (timestamp)

**One feedback per request (unique constraint)**

---

### Analytics & Reports
| Function | Endpoint | Authorization | Purpose |
|----------|----------|---|---------|
| Dashboard | GET /Reports/Dashboard | Admin | Main KPI dashboard |
| Category Analysis | GET /Reports/CategoryAnalysis | Admin | Breakdown by category |
| Priority Analysis | GET /Reports/PriorityAnalysis | Admin | Breakdown by priority |
| Technician Workload | GET /Reports/TechnicianWorkload | Admin,Tech | Load distribution |

**Dashboard KPIs:**
```
Total Requests | Pending | In Progress | Resolved | Closed
Critical Issues | Avg Resolution Time (hours)
Requests by Category (chart data)
Requests by Priority (chart data)
Top 5 Technicians (by completed count)
```

---

### User Management
| Function | Endpoint | Authorization | Purpose |
|----------|----------|---|---------|
| Create User | POST /Users/Create | Admin | Add new user account |
| Edit User | POST /Users/Edit/{id} | Admin | Update user details |
| Deactivate | POST /Users/Deactivate/{id} | Admin | Soft-delete (IsActive=false) |
| Reactivate | POST /Users/Reactivate/{id} | Admin | Re-enable user |

**Roles:** SuperAdmin | Admin | Technician | Employee

---

## 🔐 SECURITY FEATURES

### 1. Authentication
✅ **Cookie-Based Auth**
- HttpOnly flag (no JS access)
- Secure flag (HTTPS in production)
- SameSite=Lax (CSRF protection)
- 8-hour expiration + sliding renewal

✅ **PBKDF2 Password Hashing**
- 10,000 iterations
- 128-bit random salt
- 256-bit hash
- Plain passwords never stored

✅ **Multi-Factor Controls**
- Username + password required
- Google reCAPTCHA v3 on login
- Rate limiting (via CAPTCHA)

---

### 2. Authorization
✅ **Role-Based Access Control (RBAC)**
```
SuperAdmin: Unrestricted access
Admin: System administration + dashboards
Technician: Ticket assignment + status updates
Employee: Create requests + submit feedback
```

✅ **Claims-Based Identity**
- UserId, Username, Email, FullName, Role
- Extracted from User table on login

✅ **[Authorize] Attributes**
```csharp
[Authorize]                           // Any login required
[Authorize(Roles = "Admin")]          // Admin only
[Authorize(Roles = "Employee,Admin")] // Multiple roles
```

---

### 3. Data Protection
✅ **CSRF Protection**
- AntiForgeryToken on all POST forms
- Validates form token vs cookie token
- Per-session, per-form basis

✅ **SQL Injection Prevention**
- Entity Framework Core parameterized queries
- LINQ-based data access
- No string concatenation in queries

✅ **Input Validation**
- Server-side model validation
- Range constraints (e.g., Rating 1-5)
- String length limits
- Regex patterns (username, email)

✅ **Session Management**
- 1-hour inactivity timeout
- HttpOnly cookie
- Sliding expiration
- Automatic logout

---

### 4. Audit & Logging
✅ **Audit Trail**
```
User: Admin (ID:5)
Action: CREATE
Module: ServiceRequest
Description: Created request REQ-000045
Timestamp: 2024-05-07 15:30:00
```

✅ **Logged Actions**
- User login/logout
- Request creation/update
- Technician assignment
- Feedback submission
- User account changes

---

### 5. Data Privacy
✅ **Key Persistence**
- Data Protection keys stored in `/keys/` directory
- Auto-rotation every 90 days
- Old keys retained for 90 days

✅ **Soft Delete Pattern**
- Users: IsActive flag (not hard deleted)
- Categories: IsActive flag
- Data preserved for audit trail

---

## 📊 DATABASE SCHEMA

### Tables (10 total)

**1. Users**
```
UserId | Username | Email | PasswordHash | FirstName | LastName | PhoneNumber | RoleId | IsActive | CreatedAt | UpdatedAt
```

**2. Roles**
```
RoleId | RoleName | Description
(Data: 1=Admin, 2=Technician, 3=Employee, 4=SuperAdmin)
```

**3. ServiceRequests**
```
RequestId | RequestNumber | Title | Description | CategoryId | RequestorId | AssignedTechnicianId | Priority | Status | CreatedAt | ResolvedAt | ClosedAt
```

**4. Categories**
```
CategoryId | CategoryName | Description | IsActive
(Data: Hardware, Software, Network, Email, Security, Other)
```

**5. Assignments**
```
AssignmentId | RequestId | TechnicianId | AssignedBy | AssignedAt | IsActive | Notes
```

**6. Feedback**
```
FeedbackId | RequestId | Rating | Comments | ProvidedBy | ProvidedAt
(UNIQUE constraint on RequestId)
```

**7. AuditLogs**
```
Id | UserId | Action | Module | Description | CreatedAt
```

**8. Employees**
```
Id | UserId | DepartmentId | EmployeeCode | Status
```

**9. Departments**
```
DepartmentId | DepartmentName
```

**10. Assets**
```
AssetId | AssetTag | AssetName | Status
```

---

## 🏗️ ARCHITECTURE LAYERS

```
┌─────────────────────────────────────┐
│  Presentation Layer (Razor Views)  │
│  HTML Forms, Tables, Bootstrap UI  │
└────────────────┬────────────────────┘
                 │ HTTP
┌────────────────▼────────────────────┐
│  API Layer (MVC Controllers)        │
│  Request routing, authorization     │
│  Business logic coordination        │
└────────────────┬────────────────────┘
                 │
┌────────────────▼────────────────────┐
│  Service Layer                      │
│  NotificationService (SignalR)     │
│  AuditService                       │
│  TicketCommentService               │
└────────────────┬────────────────────┘
                 │
┌────────────────▼────────────────────┐
│  Data Access Layer (EF Core)        │
│  ApplicationDbContext               │
│  LINQ queries, DbSet operations    │
└────────────────┬────────────────────┘
                 │
┌────────────────▼────────────────────┐
│  Database Layer (MySQL)             │
│  Tables, relationships, indexes    │
└─────────────────────────────────────┘
```

---

## 🔧 CONFIGURATION

### Program.cs Setup
```csharp
// Database connection (MySQL)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString));

// Authentication (Cookie-based)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.LoginPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

// Authorization
builder.Services.AddAuthorization();

// Session management
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
});

// Services
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<TicketCommentService>();
```

---

## 🚀 DEPLOYMENT CHECKLIST

### Pre-Production
- [ ] Database connection string configured (Production MySQL)
- [ ] reCAPTCHA keys configured (Google Cloud Console)
- [ ] HTTPS enabled (SSL certificate)
- [ ] Cookie.SecurePolicy = Always (HTTPS only)
- [ ] Logging configured (application insights or file)
- [ ] Backup strategy in place
- [ ] Data Protection keys backed up

### Post-Deployment
- [ ] Test authentication flow
- [ ] Verify HTTPS enforcement
- [ ] Check audit logging
- [ ] Validate role-based access
- [ ] Test data persistence
- [ ] Monitor performance metrics

---

## 🧪 TEST SCENARIOS

### Authentication
1. ✅ Login with valid credentials → Dashboard
2. ✅ Login with invalid password → Error
3. ✅ Brute-force attempts → reCAPTCHA
4. ✅ Expired session → Auto-logout
5. ✅ Role-based redirect (Admin→Dashboard, Tech→Dashboard, Employee→Requests)

### Data Management
1. ✅ Create request with validation
2. ✅ Assign technician → Notification sent
3. ✅ Update status → Audit logged
4. ✅ Submit feedback → Analytics updated
5. ✅ View analytics → All metrics calculated

### Security
1. ✅ SQL injection attempt → Prevented
2. ✅ CSRF attack attempt → Prevented
3. ✅ Unauthorized access → 403 Forbidden
4. ✅ Session hijacking → Protected by HttpOnly
5. ✅ Password hash verified → Not plain text

---

## 📈 PERFORMANCE OPTIMIZATION

- **Eager Loading:** `.Include()` for related data
- **Pagination:** Default 10 items per page
- **Indexing:** Foreign keys and frequently queried columns
- **Caching:** Session state for user role
- **Query Optimization:** Materialization before grouping (LINQ)

---

## 🐛 COMMON ISSUES & SOLUTIONS

| Issue | Cause | Solution |
|-------|-------|----------|
| "Invalid username or password" | User not found or hash mismatch | Verify user exists and password is correct |
| "Access Denied" | Insufficient role | Check [Authorize(Roles="")] attribute |
| Session expires quickly | IdleTimeout too short | Increase to 1 hour in Program.cs |
| reCAPTCHA fails | Invalid keys | Check GoogleReCaptcha settings in appsettings.json |
| CSRF token invalid | Form not included @Html.AntiForgeryToken() | Add token to all POST forms |
| Database connection error | Wrong connection string | Verify server, database, user, password |

---

## 📞 SUPPORT RESOURCES

- **Full Documentation:** BACKEND_DOCUMENTATION.md
- **Database Design:** DATABASE_DESIGN.md
- **Authentication Guide:** AUTHENTICATION_AUTHORIZATION_GUIDE.md
- **Setup Instructions:** PROGRAM_CS_CONFIGURATION.md
- **Quick Start:** QUICK_START_CHECKLIST.md

