# IT Service Management System - Visual System Overview

## 🏗️ System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         USER LAYER                              │
│  [Browser] → Login/Register → Dashboard → Service Requests     │
└──────────────────────┬──────────────────────────────────────────┘
                       │ HTTPS / Secure Cookies
┌──────────────────────▼──────────────────────────────────────────┐
│              AUTHENTICATION & AUTHORIZATION                      │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │ Cookie Authentication | Claims-based Identity | RBAC      │ │
│  │ (Admin, Technician, Client roles)                         │ │
│  └────────────────────────────────────────────────────────────┘ │
└──────────────────────┬──────────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────────┐
│          ASP.NET CORE MVC APPLICATION LAYER                    │
├───────────────────────────────────────────────────────────────┤
│                                                                 │
│ ┌──────────────────────────────────────────────────────────┐   │
│ │              CONTROLLERS (6 Controllers)                │   │
│ │  ┌──────────────┐  ┌─────────────────────────────────┐  │   │
│ │  │ AuthCtrl     │  │ ServiceRequestsController      │  │   │
│ │  │ - Login      │  │ - List (filtered by role)      │  │   │
│ │  │ - Register   │  │ - Create new request           │  │   │
│ │  │ - Logout     │  │ - Update status/priority       │  │   │
│ │  └──────────────┘  │ - Close requests               │  │   │
│ │                    └─────────────────────────────────┘   │   │
│ │  ┌──────────────┐  ┌─────────────────────────────────┐  │   │
│ │  │ Assignments  │  │ Feedback Controller            │   │   │
│ │  │ - Assign     │  │ - Create feedback (1-5 stars)  │   │   │
│ │  │   technician │  │ - View satisfaction stats      │   │   │
│ │  │ - Workload   │  └─────────────────────────────────┘   │   │
│ │  └──────────────┘  ┌─────────────────────────────────┐  │   │
│ │  ┌──────────────┐  │ Reports Controller             │   │   │
│ │  │ Users        │  │ - Dashboard                    │   │   │
│ │  │ (Admin Only) │  │ - Analytics & Charts Data      │   │   │
│ │  │ - Create     │  │ - Workload analysis            │   │   │
│ │  │ - Edit       │  └─────────────────────────────────┘   │   │
│ │  │ - Deactivate │                                       │   │
│ │  └──────────────┘                                       │   │
│ └──────────────────────────────────────────────────────────┘   │
│                                                                 │
│ ┌──────────────────────────────────────────────────────────┐   │
│ │              RAZOR VIEWS (10 Views)                     │   │
│ │  Auth  │ ServiceRequests │ Feedback │ Shared │ Home    │   │
│ │  (2)   │      (4)        │   (1)    │  (2)   │   (2)   │   │
│ └──────────────────────────────────────────────────────────┘   │
│                                                                 │
│ ┌──────────────────────────────────────────────────────────┐   │
│ │        MODELS / ENTITIES (Entity Framework)             │   │
│ │  User  │ Role  │ ServiceRequest │ Category │ Assignment │   │
│ │ Feedback │ ActivityLog                                  │   │
│ └──────────────────────────────────────────────────────────┘   │
│                                                                 │
│ ┌──────────────────────────────────────────────────────────┐   │
│ │      DbContext (ApplicationDbContext.cs)                │   │
│ │  • Fluent API Configuration                            │   │
│ │  • Relationships & Constraints                         │   │
│ │  • Indexes for Performance                             │   │
│ │  • Seed Data (Roles, Categories)                       │   │
│ └──────────────────────────────────────────────────────────┘   │
│                                                                 │
└──────────────────────┬──────────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────────┐
│              DATA ACCESS LAYER (Entity Framework)               │
│  DbSets • Migrations • LINQ Queries • Change Tracking          │
└──────────────────────┬──────────────────────────────────────────┘
                       │ Parameterized Queries
┌──────────────────────▼──────────────────────────────────────────┐
│                    MYSQL DATABASE                               │
├───────────────────────────────────────────────────────────────┤
│                                                                 │
│  [USERS] ◄────────────► [ROLES]                                │
│    │                        │                                  │
│    │                        └──→ [ROLE_USERS]                 │
│    │                                                           │
│    ├──→ [SERVICE_REQUESTS] ◄──────► [CATEGORIES]              │
│    │         │                                                 │
│    │         ├──→ [ASSIGNMENTS] ◄──────► [USERS(Tech)]        │
│    │         │                                                 │
│    │         └──→ [FEEDBACK]                                  │
│    │                                                           │
│    └──→ [ACTIVITY_LOG]                                        │
│                                                                 │
│  Indexes: Status, Priority, RequestorId, TechnicianId         │
│  Constraints: PK, FK, Unique, Check                           │
│  Audit: CreatedAt, UpdatedAt, DeletedAt (soft)               │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🗄️ Database Schema

```
USERS TABLE
├─ UserId (PK)
├─ Username (UNIQUE)
├─ Email (UNIQUE)
├─ PasswordHash
├─ FirstName, LastName
├─ RoleId (FK → ROLES)
├─ IsActive
└─ Timestamps

ROLES TABLE
├─ RoleId (PK)
├─ RoleName (Admin, Technician, Client)
└─ Description

SERVICE_REQUESTS TABLE
├─ RequestId (PK)
├─ RequestNumber (UNIQUE) → REQ-001, REQ-002...
├─ Title, Description
├─ CategoryId (FK → CATEGORIES)
├─ RequestorId (FK → USERS)
├─ AssignedTechnicianId (FK → USERS, nullable)
├─ Status (ENUM: Open, InProgress, OnHold, Resolved, Closed)
├─ Priority (ENUM: Low, Medium, High, Critical)
└─ Timestamps

CATEGORIES TABLE
├─ CategoryId (PK)
├─ CategoryName
└─ Description

ASSIGNMENTS TABLE
├─ AssignmentId (PK)
├─ RequestId (FK → SERVICE_REQUESTS)
├─ TechnicianId (FK → USERS)
├─ AssignedBy (FK → USERS)
├─ IsActive (for reassignments)
└─ Notes

FEEDBACK TABLE
├─ FeedbackId (PK)
├─ RequestId (FK → SERVICE_REQUESTS, UNIQUE)
├─ Rating (1-5)
├─ Comments
├─ ProvidedBy (FK → USERS)
└─ ProvidedAt

ACTIVITY_LOG TABLE (Optional)
├─ LogId (PK)
├─ UserId (FK → USERS)
├─ Entity, EntityId, Action
└─ OldValue, NewValue
```

---

## 🔐 Security Flow

```
1. REGISTRATION
   ┌──────────────────────────────────────────────┐
   │ User fills form (username, email, password)  │
   └──────────────────┬──────────────────────────┘
                      │
   ┌──────────────────▼──────────────────────────┐
   │ Validate: unique username/email              │
   └──────────────────┬──────────────────────────┘
                      │
   ┌──────────────────▼──────────────────────────┐
   │ Hash password with PasswordHasher (PBKDF2)   │
   └──────────────────┬──────────────────────────┘
                      │
   ┌──────────────────▼──────────────────────────┐
   │ Save to database (Users table)               │
   └──────────────────┬──────────────────────────┘
                      │
   ┌──────────────────▼──────────────────────────┐
   │ Redirect to login page                       │
   └──────────────────────────────────────────────┘

2. LOGIN
   ┌──────────────────────────────────────────────┐
   │ User submits username & password              │
   └──────────────────┬──────────────────────────┘
                      │
   ┌──────────────────▼──────────────────────────┐
   │ Validate AntiForgeryToken                    │
   └──────────────────┬──────────────────────────┘
                      │
   ┌──────────────────▼──────────────────────────┐
   │ Find user in database                        │
   └──────────────────┬──────────────────────────┘
                      │
   ┌──────────────────▼──────────────────────────┐
   │ Verify password with PasswordHasher          │
   └──────────────────┬──────────────────────────┘
                      │
   ┌──────────────────▼──────────────────────────┐
   │ Create Claims Identity:                      │
   │  - UserId, Username, Email                  │
   │  - FullName, Role                           │
   └──────────────────┬──────────────────────────┘
                      │
   ┌──────────────────▼──────────────────────────┐
   │ Sign Cookie (HttpContext.SignInAsync)        │
   │  - HttpOnly=true (JS can't access)           │
   │  - Secure=true (HTTPS only)                  │
   │  - SameSite=Strict (CSRF protection)         │
   │  - ExpireTimeSpan=8 hours                    │
   └──────────────────┬──────────────────────────┘
                      │
   ┌──────────────────▼──────────────────────────┐
   │ Set HttpContext.User with claims             │
   └──────────────────┬──────────────────────────┘
                      │
   ┌──────────────────▼──────────────────────────┐
   │ Redirect to dashboard (based on role)        │
   └──────────────────────────────────────────────┘

3. AUTHORIZATION
   ┌──────────────────────────────────────────────┐
   │ Http Request arrives with cookie              │
   └──────────────────┬──────────────────────────┘
                      │
   ┌──────────────────▼──────────────────────────┐
   │ Middleware checks [Authorize] attribute      │
   └──────────────────┬──────────────────────────┘
                      │
   ┌──────┬───────────▼──────────┬────────────┐
   │      │                      │            │
   ▼      ▼                      ▼            ▼
[Public] [Authorize] [Authorize  [Authorize
         (all auth   (specific   (multiple
          users)     role)       roles)
         
   ✓ Allow           ✓ Check     ✓ Check if
                       role        any role
                                   matches
```

---

## 👥 Role-Based Access Control

```
ADMIN ROLE
├── All requests: CREATE, READ, UPDATE, DELETE
├── User management: CREATE, READ, UPDATE, DEACTIVATE, REACTIVATE
├── View all service requests (employees + technicians)
├── Assign technicians to requests
├── View all reports & analytics
└── Access: /Users, /Reports/Dashboard, /Assignments/Workload

TECHNICIAN ROLE
├── Service requests: READ (assigned only), UPDATE status
├── Cannot create or delete requests
├── Provide technical updates
├── View workload reports
├── View assigned requests
└── Access: /ServiceRequests (assigned), /Reports/TechnicianWorkload

CLIENT ROLE
├── Service requests: CREATE, READ (own only)
├── Cannot edit or delete requests
├── Cannot see other users' requests
├── Provide feedback (1-5 stars) after resolution
├── View own request history
└── Access: /ServiceRequests (own), /Feedback/Create
```

---

## 📊 Data Flow Diagram

```
SERVICE REQUEST CREATION
┌─────────────────────────────────────────────────┐
│ Client: Click "New Request"                     │
└──────────────────┬──────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────┐
│ ServiceRequestsController.Create() [GET]        │
│ -> Display form with categories                 │
└──────────────────┬──────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────┐
│ Client: Fill form                               │
│ - Title, Description, Category, Priority        │
└──────────────────┬──────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────┐
│ ServiceRequestsController.Create() [POST]       │
│ -> Validate model                               │
│ -> Generate RequestNumber (REQ-001)             │
│ -> Set RequestorId from user claims             │
│ -> Set Status = Open                            │
└──────────────────┬──────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────┐
│ Save to database (ServiceRequests table)        │
└──────────────────┬──────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────┐
│ Show success message                            │
│ Redirect to Details page                        │
└─────────────────────────────────────────────────┘

                    ↓

ASSIGNMENT & STATUS UPDATE
┌─────────────────────────────────────────────────┐
│ Admin: View request in list                     │
│ Click "Assign Technician"                       │
└──────────────────┬──────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────┐
│ AssignmentsController.Assign() [GET]            │
│ -> Display technician dropdown                  │
└──────────────────┬──────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────┘
│ Admin: Select technician                        │
└──────────────────┬──────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────┐
│ AssignmentsController.Assign() [POST]           │
│ -> Create new Assignment record                 │
│ -> Mark previous assignment as inactive         │
│ -> Update ServiceRequest.AssignedTechnicianId   │
│ -> Update ServiceRequest.Status = InProgress    │
└──────────────────┬──────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────┐
│ Technician receives assignment notification     │
│ Technician updates status (in my dashboard)     │
└──────────────────┬──────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────┐
│ ServiceRequestsController.Edit() [POST]         │
│ -> Update Status, Priority                      │
│ -> Eventually: Status = Resolved                │
└──────────────────┬──────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────┘

FEEDBACK & CLOSURE
┌─────────────────────────────────────────────────┐
│ Client: Request is resolved                     │
│ Click "Provide Feedback"                        │
└──────────────────┬──────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────┐
│ FeedbackController.Create() [GET]               │
│ -> Display 1-5 star form + comments             │
└──────────────────┬──────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────┐
│ Client: Rate (1-5 stars) + add comments         │
└──────────────────┬──────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────┐
│ FeedbackController.Create() [POST]              │
│ -> Validate rating (1-5)                        │
│ -> Save feedback to database                    │
└──────────────────┬──────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────┘
│ Admin closes request                            │
└──────────────────┬──────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────┐
│ ServiceRequestsController.Close()               │
│ -> Set Status = Closed                          │
│ -> Set ClosedAt timestamp                       │
└─────────────────────────────────────────────────┘
```

---

## 📈 Reporting & Analytics

```
DASHBOARD STATISTICS
┌────────────────────────────────────────────────────────┐
│ ┌──────────────────────────────────────────────────┐  │
│ │ CURRENT STATUS BREAKDOWN                         │  │
│ │  Open: 5  │  In Progress: 12  │  Resolved: 28   │  │
│ │  On Hold: 3  │  Closed: 156                      │  │
│ └──────────────────────────────────────────────────┘  │
│                                                        │
│ ┌──────────────────────────────────────────────────┐  │
│ │ PRIORITY BREAKDOWN (Open & In Progress)          │  │
│ │  Critical: 3  │  High: 8  │  Medium: 5  │ Low: 1 │  │
│ └──────────────────────────────────────────────────┘  │
│                                                        │
│ ┌──────────────────────────────────────────────────┐  │
│ │ BY CATEGORY (Top 5)                              │  │
│ │  Software: 45  │  Hardware: 38  │  Network: 32  │  │
│ │  Email: 28  │  Security: 12  │  Other: 8        │  │
│ └──────────────────────────────────────────────────┘  │
│                                                        │
│ ┌──────────────────────────────────────────────────┐  │
│ │ TECHNICIAN WORKLOAD                              │  │
│ │  John Smith: 8 current  │  Sarah Jones: 12      │  │
│ │  Mike Brown: 6 current  │  Lisa White: 9         │  │
│ └──────────────────────────────────────────────────┘  │
│                                                        │
│ ┌──────────────────────────────────────────────────┐  │
│ │ KEY METRICS                                      │  │
│ │  Avg Resolution Time: 24.5 hours                 │  │
│ │  Avg Customer Rating: 4.2/5 stars               │  │
│ │  Requests/Day: 12.3 new                          │  │
│ │  First-Request Resolution: 18%                   │  │
│ └──────────────────────────────────────────────────┘  │
└────────────────────────────────────────────────────────┘
```

---

## 🎯 User Workflows

### CLIENT WORKFLOW
```
1. Register → 2. Login → 3. Create Request → 4. View Progress → 5. Provide Feedback → 6. Logout
```

### TECHNICIAN WORKFLOW
```
1. Login → 2. View Assigned Requests → 3. Update Status → 4. Work Progress → 5. Mark Resolved → 6. Logout
```

### ADMIN WORKFLOW
```
1. Login → 2. View All Requests → 3. Assign Technician → 4. Monitor Progress → 
5. View Reports → 6. Manage Users → 7. Logout
```

---

## ✅ Complete Feature Checklist

| Feature | Details | Status |
|---------|---------|--------|
| User Registration | Email validation, password hashing | ✅ |
| User Login | Cookie authentication, session | ✅ |
| Service Requests | CRUD, auto-numbering (REQ-001) | ✅ |
| Request Status | 5 statuses, auto-update | ✅ |
| Technician Assignment | Assign, reassign, history | ✅ |
| Customer Feedback | 1-5 stars, comments | ✅ |
| Analytics Dashboard | Statistics, metrics | ✅ |
| Role-Based Access | 3 roles (Admin, Tech, Client) | ✅ |
| Security | Encryption, CSRF, validation | ✅ |
| Responsive UI | Bootstrap 5, mobile-friendly | ✅ |
| Error Handling | User-friendly messages | ✅ |
| Audit Trail | CreatedAt, UpdatedAt | ✅ |

---

This visual guide helps you understand the complete system architecture, data flow, security implementation, and how all components work together.

**Start implementing now!** 🚀

