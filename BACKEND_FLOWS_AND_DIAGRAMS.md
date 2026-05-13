# ITSMS Backend - Architecture & Flow Diagrams

## 🏗️ System Architecture Overview

### Complete System Flow
```
┌─────────────────────────────────────────────────────────────────┐
│                       USER INTERFACE LAYER                       │
│   (Razor Views - HTML, Bootstrap 5, CSS, JavaScript)            │
│                                                                   │
│  Authentication UI │ Request Dashboard │ Assignment │ Reports   │
└──────────────────────────────┬──────────────────────────────────┘
                               │ HTTP Requests/Responses
┌──────────────────────────────▼──────────────────────────────────┐
│                  ASP.NET CORE MVC CONTROLLER LAYER              │
│                                                                   │
│  ┌─────────────────┐  ┌──────────────────┐  ┌──────────────┐   │
│  │ AuthController  │  │ ServiceRequests  │  │ Reports      │   │
│  │                 │  │ Controller       │  │ Controller   │   │
│  │ • Login/Register│  │                  │  │              │   │
│  │ • Logout        │  │ • Create         │  │ • Dashboard  │   │
│  │ • Password Hash │  │ • Details        │  │ • Analytics  │   │
│  │ • reCAPTCHA     │  │ • List (Paginated)          │ • Charts │   │
│  └────────┬────────┘  │ • Update Status  │  └──────────────┘   │
│           │           │ • Close Request  │                      │
│  ┌────────▼────────┐  └─────────┬────────┘  ┌──────────────┐   │
│  │ Assignment      │            │           │ Feedback     │   │
│  │ Controller      │            │           │ Controller   │   │
│  │                 │            │           │              │   │
│  │ • Assign Tech   │  ┌─────────▼────────┐  │ • Submit     │   │
│  │ • Workload      │  │ Users Controller │  │ • Statistics │   │
│  │ • History       │  │                  │  │              │   │
│  └─────────────────┘  │ • Create User    │  └──────────────┘   │
│                       │ • Edit           │                      │
│                       │ • Deactivate     │                      │
│                       └──────────────────┘                      │
└──────────────────────────────┬──────────────────────────────────┘
                               │ LINQ Queries
┌──────────────────────────────▼──────────────────────────────────┐
│              ENTITY FRAMEWORK CORE DATA ACCESS LAYER            │
│                                                                   │
│  ApplicationDbContext                                            │
│  ├─ DbSet<User>                                                 │
│  ├─ DbSet<ServiceRequest>                                       │
│  ├─ DbSet<Category>                                             │
│  ├─ DbSet<Assignment>                                           │
│  ├─ DbSet<Feedback>                                             │
│  ├─ DbSet<AuditLog>                                             │
│  └─ DbSet<...> (other entities)                                 │
└──────────────────────────────┬──────────────────────────────────┘
                               │ SQL Statements
┌──────────────────────────────▼──────────────────────────────────┐
│                     MYSQL DATABASE SERVER                        │
│                                                                   │
│  Users Table │ Roles │ ServiceRequests │ Categories             │
│  Assignments │ Feedback │ AuditLogs │ Assets │ Employees        │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🔐 Authentication & Authorization Flow

### Complete Login Process
```
STEP 1: User Visits Login Page
┌──────────────────────────┐
│ User navigates to        │
│ /Auth/Login (GET)        │
└──────────┬───────────────┘
           │
           ▼
┌──────────────────────────────────────────┐
│ Server Returns Login Form with:          │
│ • Username field                         │
│ • Password field                         │
│ • Anti-Forgery Token hidden field       │
│ • reCAPTCHA widget embedded             │
└──────────┬───────────────────────────────┘
           │
           ▼
    User Sees:
    ┌────────────────────────────────┐
    │  IT Service Management System  │
    │                                │
    │  Username: [____________]      │
    │  Password: [____________]      │
    │                                │
    │  ☐ Remember Me                │
    │  [  I'm not a robot - reCAPTCHA ] │
    │                                │
    │  [ Login ]  [ Register ]       │
    └────────────────────────────────┘


STEP 2: User Submits Credentials
┌──────────────────────────┐
│ User enters:             │
│ • Username: john.doe     │
│ • Password: (hashed)     │
│ • CAPTCHA response token │
└──────────┬───────────────┘
           │
           ▼
    POST /Auth/Login
    {
      username: "john.doe",
      password: "SomePassword123!",
      g-recaptcha-response: "token_xyz..."
    }


STEP 3: Server-Side Validation
┌─────────────────────────────────────────────────┐
│ 1. VALIDATE ANTI-FORGERY TOKEN                  │
│    └─ Check form token matches cookie token    │
│       (Prevents CSRF attacks)                   │
└────────────────────────┬────────────────────────┘
                         │
                         ▼
         ┌──────────────────────────┐
         │ Token Valid?             │
         └────────┬─────────────────┘
                  │
      ┌───────────┴───────────┐
      │ YES                   │ NO
      ▼                       ▼
   Continue          Return Error:
                     "Invalid request"
                         │
                         ▼
                    Refresh login form


STEP 4: reCAPTCHA Verification
┌─────────────────────────────────────────────────┐
│ 2. VERIFY RECAPTCHA WITH GOOGLE                 │
│                                                  │
│ POST https://www.google.com/recaptcha/api/      │
│   siteverify                                    │
│   secret: SERVER_SECRET_KEY                     │
│   response: token_xyz...                        │
│                                                  │
│ Google Checks:                                  │
│ • Is token valid?                               │
│ • Is token fresh? (< 2 min old)                │
│ • Is user likely human?                         │
└────────────────────────────┬────────────────────┘
                             │
                             ▼
                    ┌─────────────────┐
                    │ Success >= 0.5? │
                    └────────┬────────┘
                             │
                ┌────────────┴────────────┐
                │ YES                    │ NO
                ▼                        ▼
           Continue            Return Error:
                               "CAPTCHA failed"
                                    │
                                    ▼
                               Retry login


STEP 5: Database Lookup
┌──────────────────────────────────────────┐
│ 3. QUERY DATABASE FOR USER               │
│                                           │
│ SELECT * FROM Users u                    │
│ JOIN Roles r ON u.RoleId = r.RoleId     │
│ WHERE u.Username = @Username              │
│   AND u.IsActive = 1                     │
│                                           │
│ Parameters: @Username = "john.doe"       │
│            (Safe - parameterized query)   │
└─────────────────────────┬──────────────────┘
                          │
                          ▼
                    ┌──────────────┐
                    │ User Found?  │
                    └─────┬────────┘
                          │
              ┌───────────┴───────────┐
              │ YES                   │ NO
              ▼                       ▼
           Continue         Return Error:
                           "Invalid username"
                                 │
                                 ▼
                            Retry login


STEP 6: Password Verification
┌──────────────────────────────────────────────────┐
│ 4. VERIFY PASSWORD HASH (PBKDF2)                │
│                                                   │
│ Stored in DB: AQAAAAIAAYag...                   │
│                                                   │
│ PasswordHasher<User>.VerifyHashedPassword(       │
│   user: User object,                            │
│   hash: "AQAAAAIAAYag...",                      │
│   providedPassword: "SomePassword123!"          │
│ )                                               │
│                                                   │
│ Process:                                         │
│ • Extract salt from stored hash                 │
│ • PBKDF2: Hash input password with salt         │
│ • Compare computed hash with stored hash        │
│ • Return Success/Failed                         │
└─────────────────────────┬──────────────────────┘
                          │
                          ▼
                   ┌─────────────┐
                   │ Match?      │
                   └──────┬──────┘
                          │
           ┌──────────────┴──────────────┐
           │ YES                        │ NO
           ▼                            ▼
        Continue               Return Error:
                              "Invalid password"
                                    │
                                    ▼
                               Retry login


STEP 7: Create Claims Identity
┌──────────────────────────────────────────┐
│ 5. EXTRACT USER CLAIMS FROM DB          │
│                                           │
│ var claims = new List<Claim>             │
│ {                                         │
│   new Claim(ClaimTypes.NameIdentifier,   │
│             "42"),                        │ ← UserId
│   new Claim(ClaimTypes.Name,             │
│             "john.doe"),                  │ ← Username
│   new Claim(ClaimTypes.Email,            │
│             "john@example.com"),          │ ← Email
│   new Claim("FullName",                  │
│             "John Doe"),                  │ ← Display name
│   new Claim(ClaimTypes.Role,             │
│             "Technician")                 │ ← User Role
│ };                                        │
└──────────────────────────────────────────┘


STEP 8: Sign Authentication Cookie
┌──────────────────────────────────────────┐
│ 6. CREATE SIGNED AUTHENTICATION COOKIE   │
│                                           │
│ HttpContext.SignInAsync(                 │
│   scheme: "Cookie",                      │
│   principal: ClaimsPrincipal(             │
│     identity: ClaimsIdentity(claims)     │
│   ),                                      │
│   properties: AuthenticationProperties {  │
│     IsPersistent = true,                 │
│     ExpiresUtc = DateTime.UtcNow          │
│                 + 8 hours                │
│   }                                       │
│ )                                         │
│                                           │
│ Resulting Cookie:                        │
│ ┌────────────────────────────────────┐  │
│ │ Name: ITSMS.Auth                   │  │
│ │ Value: encrypted_token_xyz...      │  │
│ │ HttpOnly: true                     │  │
│ │ Secure: true (prod)                │  │
│ │ SameSite: Lax                      │  │
│ │ Expires: 2024-05-07 23:30 UTC     │  │
│ └────────────────────────────────────┘  │
└──────────────────────────────────────────┘


STEP 9: Role-Based Redirect
┌──────────────────────────────────────────┐
│ 7. DETERMINE REDIRECT BASED ON ROLE     │
│                                           │
│ if (User.IsInRole("Admin") ||             │
│     User.IsInRole("SuperAdmin"))          │
│   return RedirectToAction("Dashboard",    │
│                          "Reports")       │
│                                           │
│ else if (User.IsInRole("Technician"))    │
│   return RedirectToAction("Index",        │
│                          "TechnicianDash")│
│                                           │
│ else if (User.IsInRole("Employee"))      │
│   return RedirectToAction("Index",        │
│                          "ServiceRequests")
│                                           │
│ else                                      │
│   return RedirectToAction("Index", "Home")
└──────────────────────────────────────────┘


STEP 10: User Logged In & Dashboard Displayed
                         │
                         ▼
    ┌──────────────────────────────────────┐
    │ User Successfully Logged In          │
    │                                      │
    │ GET /Reports/Dashboard (if Admin)    │
    │ GET /ServiceRequests (if Employee)   │
    │                                      │
    │ Response: 200 OK                     │
    │ Headers: Set-Cookie: ITSMS.Auth...  │
    │ Body: Dashboard HTML                 │
    └──────────────────────────────────────┘
                         │
                         ▼
    User Sees Dashboard Appropriate to Role
    ┌──────────────────────────────────────┐
    │  [ADMIN DASHBOARD]                   │
    │                                      │
    │  Total Requests: 284                 │
    │  Pending: 23 │ Open: 45              │
    │  Critical: 8 │ Resolved: 156         │
    │                                      │
    │  Technician Workload                 │
    │  Category Analysis                   │
    │  Reports                             │
    └──────────────────────────────────────┘
```

---

## 🎫 Service Request Creation Flow

```
USER CREATES REQUEST
        │
        ▼
┌──────────────────────────────────────┐
│ User navigates to                    │
│ /ServiceRequests/Create              │
└────────────┬─────────────────────────┘
             │ [Authorize(Roles = "Employee,Admin")]
             ▼
    ┌─────────────────────┐
    │ Authorized?         │
    ├─────────┬───────────┤
    │ YES     │ NO        │
    ▼         ▼
  Show    Return 403
  Form    Forbidden
    │
    ▼
┌──────────────────────────────────────┐
│ GET /ServiceRequests/Create          │
│                                      │
│ Server Queries:                      │
│ 1. Get Employee record for user      │
│ 2. Get active assets for employee    │
│    SELECT * FROM AssetAssignments    │
│    WHERE EmployeeId = @EmployeeId   │
│      AND ReturnedDate IS NULL       │
│ 3. Get Categories                    │
│    SELECT * FROM Categories          │
│    WHERE IsActive = 1                │
└────────────┬─────────────────────────┘
             │
             ▼
    Display Form:
    ┌──────────────────────────┐
    │ Create Service Request   │
    │                          │
    │ Title: [_____________]   │
    │ Description: [_______]   │
    │ Category: [Hardware ▼]   │
    │ Priority: [High ▼]      │
    │ Asset: [Laptop-001 ▼]   │
    │                          │
    │ @Html.AntiForgeryToken() │
    │ [Submit] [Cancel]       │
    └──────────────────────────┘


USER SUBMITS FORM
        │
        ▼
┌──────────────────────────────────────┐
│ POST /ServiceRequests/Create         │
│                                      │
│ {                                    │
│   Title: "Laptop won't start",       │
│   Description: "...",                │
│   CategoryId: 1,                     │
│   Priority: High,                    │
│   AssetId: 123                       │
│ }                                    │
└────────────┬─────────────────────────┘
             │
             ▼
┌────────────────────────────────────────┐
│ SERVER-SIDE PROCESSING                 │
│                                        │
│ 1. Validate CSRF Token                │
│    └─ Check form token vs cookie      │
│       Fail: 400 Bad Request            │
│                                        │
│ 2. Validate Model                     │
│    └─ Title: required, 5-150 chars    │
│    └─ Description: required, 10+ chars│
│    └─ CategoryId: must exist           │
│    └─ Priority: must be enum value    │
│       Fail: Redisplay form + errors   │
│                                        │
│ 3. Generate Unique RequestNumber      │
│    └─ SELECT MAX(RequestId) ...       │
│    └─ RequestNumber = "REQ-000046"    │
│                                        │
│ 4. Create ServiceRequest Object       │
│    {                                   │
│      RequestId: (auto-increment)      │
│      RequestNumber: "REQ-000046",      │
│      Title: "Laptop won't start",      │
│      Description: "...",               │
│      CategoryId: 1,                    │
│      RequestorId: 42,                 │
│      EmployeeId: (from lookup),       │
│      AssetId: 123,                    │
│      Priority: High,                   │
│      Status: Pending,                 │
│      CreatedAt: NOW(),                │
│      UpdatedAt: NOW()                 │
│    }                                   │
│                                        │
│ 5. Save to Database                   │
│    INSERT INTO ServiceRequests (...)   │
│    VALUES (...)                        │
│                                        │
│ 6. Log to Audit Trail                 │
│    INSERT INTO AuditLogs (...)        │
│    Action: "CREATE"                    │
│    Module: "ServiceRequest"            │
│    Description: "Created REQ-000046"   │
│                                        │
│ 7. Set Success Message                │
│    TempData["Success"] =               │
│    "Request REQ-000046 created!"      │
│                                        │
│ 8. Redirect                           │
│    Redirect to Details page            │
└────────────┬─────────────────────────┘
             │
             ▼
┌──────────────────────────────────────┐
│ Redirect: /ServiceRequests/           │
│ Details/46                            │
│                                       │
│ Response: 302 Found                   │
│ Location: /ServiceRequests/Details/46 │
└────────────┬───────────────────────────┘
             │
             ▼
USER SEES CREATED REQUEST
    ┌─────────────────────────────┐
    │ Service Request Details     │
    │                             │
    │ Request #: REQ-000046       │
    │ Status: Pending             │
    │ Priority: High              │
    │ Category: Hardware          │
    │ Created: Just now           │
    │ Assigned To: (Unassigned)   │
    │                             │
    │ Description:                │
    │ (Full text shown)           │
    │                             │
    │ [✓] Success Message:        │
    │ "Request REQ-000046 created"│
    └─────────────────────────────┘
```

---

## 👤 Role-Based Access Control Matrix

```
┌──────────────────────┬──────────┬────────┬──────────┬──────────┐
│ Feature/Action       │ SuperAdm │ Admin  │ Technics │ Employee │
├──────────────────────┼──────────┼────────┼──────────┼──────────┤
│                                                              │
│ AUTHENTICATION                                              │
├──────────────────────┼──────────┼────────┼──────────┼──────────┤
│ Login                │    ✅    │   ✅   │    ✅    │    ✅    │
│ Logout               │    ✅    │   ✅   │    ✅    │    ✅    │
│ Register             │    ✅    │   ✅   │    ✅    │    ✅    │
│                                                              │
│ SERVICE REQUESTS                                            │
├──────────────────────┼──────────┼────────┼──────────┼──────────┤
│ Create Request       │    ✅    │   ✅   │    ❌    │    ✅    │
│ View Own Requests    │    ✅    │   ✅   │    ✅    │    ✅    │
│ View All Requests    │    ✅    │   ✅   │    ✅    │    ❌    │
│ Update Status        │    ✅    │   ✅   │    ✅    │    ❌    │
│ Update Priority      │    ✅    │   ✅   │    ❌    │    ❌    │
│ Close Request        │    ✅    │   ✅   │    ✅    │    ❌    │
│                                                              │
│ ASSIGNMENT                                                  │
├──────────────────────┼──────────┼────────┼──────────┼──────────┤
│ Assign Technician    │    ✅    │   ✅   │    ❌    │    ❌    │
│ View Workload        │    ✅    │   ✅   │    ✅    │    ❌    │
│ View Assignment Hist │    ✅    │   ✅   │    ✅    │    ✅    │
│                                                              │
│ FEEDBACK                                                    │
├──────────────────────┼──────────┼────────┼──────────┼──────────┤
│ Submit Feedback      │    ✅    │   ✅   │    ✅    │    ✅    │
│ View Statistics      │    ✅    │   ✅   │    ❌    │    ❌    │
│ Edit Feedback        │    ✅    │   ✅   │    ✅    │    ✅    │
│                                                              │
│ ANALYTICS & REPORTS                                         │
├──────────────────────┼──────────┼────────┼──────────┼──────────┤
│ Dashboard            │    ✅    │   ✅   │    ❌    │    ❌    │
│ Category Analysis    │    ✅    │   ✅   │    ❌    │    ❌    │
│ Priority Analysis    │    ✅    │   ✅   │    ❌    │    ❌    │
│ Satisfaction Report  │    ✅    │   ✅   │    ❌    │    ❌    │
│                                                              │
│ USER MANAGEMENT                                             │
├──────────────────────┼──────────┼────────┼──────────┼──────────┤
│ Create User          │    ✅    │   ✅   │    ❌    │    ❌    │
│ Edit User            │    ✅    │   ✅   │    ❌    │    ❌    │
│ Deactivate User      │    ✅    │   ✅   │    ❌    │    ❌    │
│ View All Users       │    ✅    │   ✅   │    ❌    │    ❌    │
│                                                              │
└──────────────────────┴──────────┴────────┴──────────┴──────────┘
```

---

## 🔒 Security Layers

```
┌─────────────────────────────────────────────────────────────┐
│                    ATTACK VECTORS                            │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  1. BRUTE FORCE LOGIN ATTACKS                               │
│     └─ Multiple failed password attempts                    │
│                                                              │
│     🛡️ DEFENSE: Google reCAPTCHA v3                         │
│        └─ Detects bot patterns                              │
│        └─ Requires human verification after failures        │
│                                                              │
│                                                              │
│  2. SQL INJECTION                                           │
│     └─ Malicious SQL in form inputs                         │
│     └─ Example: "'; DROP TABLE Users; --"                   │
│                                                              │
│     🛡️ DEFENSE: Parameterized Queries (EF Core)            │
│        └─ Input treated as data, not SQL                    │
│        └─ Query: SELECT * FROM Users WHERE Username = @p0  │
│        └─ Parameter: @p0 = "'; DROP TABLE Users; --"       │
│        └─ Result: Searches for user with that literal name  │
│                                                              │
│                                                              │
│  3. CROSS-SITE REQUEST FORGERY (CSRF)                       │
│     └─ Attacker tricks user to submit malicious request    │
│     └─ Example: Attacker site contains:                     │
│        <img src="/Users/Delete/123">                        │
│                                                              │
│     🛡️ DEFENSE: Anti-Forgery Tokens                         │
│        └─ @Html.AntiForgeryToken() in forms                │
│        └─ Token unique per request                          │
│        └─ Server validates token before processing          │
│        └─ Attacker can't replicate token                    │
│                                                              │
│                                                              │
│  4. SESSION HIJACKING / COOKIE THEFT                        │
│     └─ Attacker steals session cookie                       │
│     └─ JavaScript reads cookie and sends to attacker        │
│                                                              │
│     🛡️ DEFENSE: HttpOnly & Secure Flags                    │
│        └─ HttpOnly: JavaScript cannot access cookie         │
│        └─ Secure: Only sent over HTTPS (prod)               │
│        └─ SameSite=Lax: Not sent in cross-site requests    │
│                                                              │
│                                                              │
│  5. PASSWORD CRACKING                                       │
│     └─ Attacker obtains password hashes                     │
│     └─ Uses dictionary or brute-force to crack them         │
│                                                              │
│     🛡️ DEFENSE: PBKDF2 Hashing                              │
│        └─ 10,000 iterations (slow algorithm)                │
│        └─ 128-bit random salt per password                  │
│        └─ Same password hashes differently                  │
│        └─ Very expensive to brute-force                     │
│                                                              │
│                                                              │
│  6. UNAUTHORIZED ACCESS (PRIVILEGE ESCALATION)              │
│     └─ User tries to access admin-only pages                │
│     └─ User edits URL to access other user's data           │
│                                                              │
│     🛡️ DEFENSE: Authorization Attributes                    │
│        └─ [Authorize(Roles = "Admin")]                      │
│        └─ [Authorize]                                        │
│        └─ Manual checks: CanViewRequest(request)            │
│        └─ Server-side validation always                     │
│        └─ Client-side checks are bypassable                 │
│                                                              │
│                                                              │
│  7. DATA TAMPERING                                          │
│     └─ User modifies priority or status in DevTools         │
│     └─ User intercepts form and changes values              │
│                                                              │
│     🛡️ DEFENSE: Server-Side Validation                      │
│        └─ Never trust client input                          │
│        └─ Validate all data server-side                     │
│        └─ Enum validation: Priority in [Low,Med,High,Crit]  │
│        └─ Range validation: Rating in [1,5]                 │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## 📊 Data Flow - From Database to View

```
REQUEST FLOW: GET /ServiceRequests/Index

┌──────────────────────────────────────┐
│ 1. CONTROLLER RECEIVES REQUEST       │
│ GET /ServiceRequests/Index           │
│ (page=1, pageSize=10)                │
└────────────┬─────────────────────────┘
             │
             ▼
┌──────────────────────────────────────┐
│ 2. AUTHORIZATION CHECK               │
│ [Authorize] attribute verified       │
│ Check: User.Identity.IsAuthenticated │
│ Result: ✅ Authenticated             │
└────────────┬─────────────────────────┘
             │
             ▼
┌──────────────────────────────────────┐
│ 3. EXTRACT USER INFO                 │
│ UserId = User.FindFirst(...).Value   │
│ UserRole = User.FindFirst(Role).Value│
│ Result:                              │
│   UserId = 42                        │
│   UserRole = "Technician"            │
└────────────┬─────────────────────────┘
             │
             ▼
┌──────────────────────────────────────┐
│ 4. BUILD QUERY (LINQ)                │
│                                      │
│ IQueryable<ServiceRequest> requests  │
│   = _context.ServiceRequests          │
│     .Include(sr => sr.Category)      │
│     .Include(sr => sr.Requestor)     │
│     .AsQueryable();                  │
│                                      │
│ // NOT EXECUTED YET (deferred)       │
└────────────┬─────────────────────────┘
             │
             ▼
┌──────────────────────────────────────┐
│ 5. APPLY ROLE-BASED FILTERING        │
│                                      │
│ if (userRole == "Technician")        │
│   requests = requests.Where(sr =>    │
│     sr.AssignedTechnicianId == 42 || │
│     sr.AssignedTechnicianId == null  │
│   );                                 │
│                                      │
│ // QUERY STILL NOT EXECUTED          │
└────────────┬─────────────────────────┘
             │
             ▼
┌──────────────────────────────────────┐
│ 6. MATERIALIZE FOR PAGINATION        │
│                                      │
│ var totalCount = requests.Count()    │
│ // EXECUTES QUERY HERE (First time!) │
│ // Translates LINQ to SQL:           │
│                                      │
│ SELECT COUNT(1)                      │
│ FROM ServiceRequests sr              │
│ WHERE sr.AssignedTechnicianId = 42   │
│    OR sr.AssignedTechnicianId IS NULL│
│                                      │
│ Result: totalCount = 15              │
└────────────┬─────────────────────────┘
             │
             ▼
┌──────────────────────────────────────┐
│ 7. CALCULATE PAGINATION              │
│                                      │
│ var totalPages = (15 + 9) / 10 = 2   │
│ var skip = (1 - 1) * 10 = 0          │
│ var take = 10                        │
└────────────┬─────────────────────────┘
             │
             ▼
┌──────────────────────────────────────┐
│ 8. EXECUTE PAGINATED QUERY           │
│                                      │
│ var serviceRequests = requests       │
│   .OrderByDescending(sr=>CreatedAt)  │
│   .Skip(0)                           │
│   .Take(10)                          │
│   .ToList(); // EXECUTES!            │
│                                      │
│ Generated SQL:                       │
│ SELECT sr.*, c.*, u.*               │
│ FROM ServiceRequests sr              │
│ JOIN Categories c                    │
│ JOIN Users u                         │
│ WHERE (sr.AssignedTechnicianId = 42  │
│    OR sr.AssignedTechnicianId IS NULL)
│ ORDER BY sr.CreatedAt DESC           │
│ LIMIT 10 OFFSET 0;                   │
│                                      │
│ MySQL Returns: 10 rows               │
└────────────┬─────────────────────────┘
             │
             ▼
┌──────────────────────────────────────┐
│ 9. PASS DATA TO VIEW                 │
│                                      │
│ return View(serviceRequests);        │
│                                      │
│ Model:                               │
│ [                                    │
│   {                                  │
│     RequestId: 1,                    │
│     RequestNumber: "REQ-000001",      │
│     Title: "Laptop issues",          │
│     CategoryName: "Hardware",        │
│     Priority: "High",                │
│     Status: "InProgress",            │
│     CreatedAt: 2024-05-07 09:15:00  │
│   },                                 │
│   ... (9 more)                       │
│ ]                                    │
│                                      │
│ ViewBag:                             │
│ ViewBag.CurrentPage = 1              │
│ ViewBag.TotalPages = 2               │
│ ViewBag.TotalCount = 15              │
└────────────┬─────────────────────────┘
             │
             ▼
┌──────────────────────────────────────┐
│ 10. RENDER RAZOR VIEW                │
│                                      │
│ @model List<ServiceRequest>          │
│                                      │
│ <table>                              │
│   @foreach(var req in Model)         │
│   {                                  │
│     <tr>                             │
│       <td>@req.RequestNumber</td>    │
│       <td>@req.Title</td>            │
│       <td>@req.Priority</td>         │
│       <td>@req.Status</td>           │
│       <td>@req.CreatedAt</td>        │
│     </tr>                            │
│   }                                  │
│ </table>                             │
│                                      │
│ Pagination:                          │
│ Page 1 of 2                          │
│ [← Prev] [1] [2] [Next →]            │
└────────────┬─────────────────────────┘
             │
             ▼
┌──────────────────────────────────────┐
│ 11. SEND RESPONSE TO CLIENT          │
│                                      │
│ Response: 200 OK                     │
│ Content-Type: text/html              │
│ Body: (HTML table rendered)          │
│                                      │
│ Browser displays:                    │
│ ┌────────────────────────────────┐  │
│ │ Service Requests               │  │
│ ├─────────────────────────────────┤ │
│ │ REQ-000001 Laptop issues  High   │ │
│ │ REQ-000002 Printer error  Medium │ │
│ │ ... (8 more rows)               │ │
│ │                                 │ │
│ │ Page 1 of 2                     │ │
│ └────────────────────────────────┘  │
└──────────────────────────────────────┘
```

---

## 🔄 Request Status State Machine

```
Possible Status Transitions:

    ┌──────────┐
    │ PENDING  │ ◄─────── Request created
    └─────┬────┘
          │
          ├──────────────────────┐
          │                      │
          ▼                      ▼
    ┌─────────────┐         ┌────────┐
    │ IN PROGRESS │         │ ON HOLD│
    └─────┬───────┘         └────┬───┘
          │                      │
          │                      │
          └──────────┬───────────┘
                     │
                     ▼
            ┌──────────────┐
            │  RESOLVED    │
            └──────┬───────┘
                   │
                   ▼
            ┌──────────────┐
            │   CLOSED     │
            └──────────────┘

Priority/Status Combinations:
                 PENDING    IN_PROGRESS    RESOLVED    CLOSED
CRITICAL         ├─ 2       ├─ 3           ├─ 2        └─ 1
HIGH             ├─ 5       ├─ 8           ├─ 18       └─ 3
MEDIUM           ├─ 12      ├─ 28          ├─ 92       └─ 24
LOW              ├─ 4       ├─ 6           ├─ 43       └─ 33

Metrics by Status:
┌──────────────┬──────┬────────────────────────┐
│ Status       │ Count│ Avg Resolution Hours   │
├──────────────┼──────┼────────────────────────┤
│ Pending      │ 23   │ N/A (not started)      │
│ In Progress  │ 45   │ N/A (ongoing)          │
│ Resolved     │ 156  │ 24.5 hours             │
│ Closed       │ 60   │ 28.2 hours (total)     │
└──────────────┴──────┴────────────────────────┘
```

---

This comprehensive diagram and flow documentation covers all major backend processes, security layers, and data flow patterns used in your ITSMS system.

