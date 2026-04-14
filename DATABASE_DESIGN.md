# IT Service Management System (IT-SMS) - Database Design

## Overview
This document describes the complete database schema for the IT Service Management System using MySQL.

---

## Database Tables

### 1. Users Table
Stores system users (Admins, Technicians, Clients).

```sql
CREATE TABLE Users (
    UserId INT PRIMARY KEY AUTO_INCREMENT,
    Username VARCHAR(50) UNIQUE NOT NULL,
    Email VARCHAR(100) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    PhoneNumber VARCHAR(20),
    RoleId INT NOT NULL,
    IsActive TINYINT DEFAULT 1,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);
```

**Columns:**
- `UserId` - Primary Key (Auto-increment)
- `Username` - Unique username for login
- `Email` - Unique email address
- `PasswordHash` - Hashed password using ASP.NET Core Identity PasswordHasher
- `FirstName`, `LastName` - User's full name
- `PhoneNumber` - Contact phone number (nullable)
- `RoleId` - Foreign Key referencing Roles
- `IsActive` - Soft delete flag (0=inactive, 1=active)
- `CreatedAt`, `UpdatedAt` - Audit trail timestamps

---

### 2. Roles Table
System roles (Permissions management).

```sql
CREATE TABLE Roles (
    RoleId INT PRIMARY KEY AUTO_INCREMENT,
    RoleName VARCHAR(50) UNIQUE NOT NULL,
    Description VARCHAR(255),
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Insert default roles
INSERT INTO Roles (RoleName, Description) VALUES 
('Admin', 'IT Administrator with full access'),
('Technician', 'IT Support Technician'),
('Client', 'Employee / Client / Requestor');
```

**Columns:**
- `RoleId` - Primary Key
- `RoleName` - Role name (Admin, Technician, Client)
- `Description` - Role description

---

### 3. Categories Table
Service request categories (Hardware, Software, Network, etc.).

```sql
CREATE TABLE Categories (
    CategoryId INT PRIMARY KEY AUTO_INCREMENT,
    CategoryName VARCHAR(100) NOT NULL,
    Description VARCHAR(255),
    IsActive TINYINT DEFAULT 1,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Insert default categories
INSERT INTO Categories (CategoryName, Description) VALUES 
('Hardware', 'Hardware related issues and requests'),
('Software', 'Software installation and support'),
('Network', 'Network connectivity issues'),
('Email', 'Email and collaboration tools'),
('Security', 'Security related issues'),
('Other', 'Other miscellaneous requests');
```

**Columns:**
- `CategoryId` - Primary Key
- `CategoryName` - Category name
- `Description` - Category description
- `IsActive` - Active status flag

---

### 4. ServiceRequests Table
Main ticketing table for service requests.

```sql
CREATE TABLE ServiceRequests (
    RequestId INT PRIMARY KEY AUTO_INCREMENT,
    RequestNumber VARCHAR(20) UNIQUE NOT NULL,
    Title VARCHAR(150) NOT NULL,
    Description TEXT NOT NULL,
    CategoryId INT NOT NULL,
    RequestorId INT NOT NULL,
    AssignedTechnicianId INT,
    Status ENUM('Open', 'In Progress', 'On Hold', 'Resolved', 'Closed') DEFAULT 'Open' NOT NULL,
    Priority ENUM('Low', 'Medium', 'High', 'Critical') DEFAULT 'Medium' NOT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    ResolvedAt DATETIME,
    ClosedAt DATETIME,
    FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId),
    FOREIGN KEY (RequestorId) REFERENCES Users(UserId),
    FOREIGN KEY (AssignedTechnicianId) REFERENCES Users(UserId),
    INDEX idx_status (Status),
    INDEX idx_priority (Priority),
    INDEX idx_requestor (RequestorId),
    INDEX idx_technician (AssignedTechnicianId)
);
```

**Columns:**
- `RequestId` - Primary Key
- `RequestNumber` - Unique ticket number (e.g., REQ-001, REQ-002)
- `Title` - Request title
- `Description` - Detailed description
- `CategoryId` - Foreign Key to Categories
- `RequestorId` - Foreign Key to Users (who created the request)
- `AssignedTechnicianId` - Foreign Key to Users (assigned technician, nullable until assigned)
- `Status` - ENUM (Open, In Progress, On Hold, Resolved, Closed)
- `Priority` - ENUM (Low, Medium, High, Critical)
- `CreatedAt`, `UpdatedAt` - Audit trail
- `ResolvedAt`, `ClosedAt` - Completion timestamps
- **Indexes** - On frequently queried columns for performance

---

### 5. Assignments Table
Tracks assignment history and current assignments.

```sql
CREATE TABLE Assignments (
    AssignmentId INT PRIMARY KEY AUTO_INCREMENT,
    RequestId INT NOT NULL,
    TechnicianId INT NOT NULL,
    AssignedBy INT NOT NULL,
    AssignedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    IsActive TINYINT DEFAULT 1,
    Notes VARCHAR(255),
    FOREIGN KEY (RequestId) REFERENCES ServiceRequests(RequestId) ON DELETE CASCADE,
    FOREIGN KEY (TechnicianId) REFERENCES Users(UserId),
    FOREIGN KEY (AssignedBy) REFERENCES Users(UserId),
    INDEX idx_request (RequestId),
    INDEX idx_technician (TechnicianId),
    INDEX idx_active (IsActive)
);
```

**Columns:**
- `AssignmentId` - Primary Key
- `RequestId` - Foreign Key to ServiceRequests
- `TechnicianId` - Foreign Key to Users (technician assigned)
- `AssignedBy` - Foreign Key to Users (admin/manager who made assignment)
- `AssignedAt` - Timestamp of assignment
- `IsActive` - Current assignment flag (1=active, 0=reassigned)
- `Notes` - Assignment notes

---

### 6. Feedback Table
Customer feedback on completed service requests.

```sql
CREATE TABLE Feedback (
    FeedbackId INT PRIMARY KEY AUTO_INCREMENT,
    RequestId INT NOT NULL UNIQUE,
    Rating INT CHECK (Rating BETWEEN 1 AND 5),
    Comments TEXT,
    ProvidedBy INT NOT NULL,
    ProvidedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (RequestId) REFERENCES ServiceRequests(RequestId) ON DELETE CASCADE,
    FOREIGN KEY (ProvidedBy) REFERENCES Users(UserId),
    INDEX idx_rating (Rating),
    INDEX idx_date (ProvidedAt)
);
```

**Columns:**
- `FeedbackId` - Primary Key
- `RequestId` - Foreign Key to ServiceRequests (UNIQUE - one feedback per request)
- `Rating` - Rating 1-5 stars
- `Comments` - Feedback comments
- `ProvidedBy` - Foreign Key to Users (who provided feedback)
- `ProvidedAt` - Feedback timestamp

---

### 7. ActivityLog Table (OPTIONAL - For Audit Trail)
Tracks all system activities for audit purposes.

```sql
CREATE TABLE ActivityLog (
    LogId INT PRIMARY KEY AUTO_INCREMENT,
    UserId INT,
    Entity VARCHAR(50),
    EntityId INT,
    Action VARCHAR(50),
    OldValue TEXT,
    NewValue TEXT,
    IPAddress VARCHAR(50),
    LoggedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    INDEX idx_timestamp (LoggedAt),
    INDEX idx_entity (Entity, EntityId)
);
```

---

## Relationships Summary

```
Users ← → Roles (1-to-Many)
         ↓
    ServiceRequests ← → Categories (1-to-Many)
         ↓
    Assignments ← → Users/Technicians (1-to-Many)
         ↓
    Feedback (1-to-1)
```

---

## Normalization Notes

✅ **3NF Compliance:**
- Each table has a single primary key
- All non-key attributes depend on the primary key
- No transitive dependencies
- ENUM fields used for status/priority (best practice for fixed values)

---

## Indexing Strategy

Performance indexes created on:
- `ServiceRequests.Status` - Frequent filtering
- `ServiceRequests.Priority` - Frequent sorting
- `ServiceRequests.RequestorId` - User dashboards
- `ServiceRequests.AssignedTechnicianId` - Technician workload
- `Assignments.IsActive` - Find current assignments
- `Feedback.Rating` - Report generation
- `ActivityLog.LoggedAt` - Time-range queries

---

## Security Considerations

1. **Password Storage** - Passwords stored as hashed values using PasswordHasher
2. **Soft Deletes** - `IsActive` flag for data preservation
3. **Foreign Key Constraints** - Integrity maintained with ON DELETE CASCADE where appropriate
4. **Audit Trail** - CreatedAt/UpdatedAt on all main tables
5. **Access Control** - Enforced in application layer (Authorization attributes)

---

## Sample Data Preparation

After creating the database:
1. Insert roles (Admin, Technician, Client)
2. Insert categories (Hardware, Software, Network, Email, Security, Other)
3. Create test users for each role
4. Generate sample service requests for testing

