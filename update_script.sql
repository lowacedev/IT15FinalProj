CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;
DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    ALTER DATABASE CHARACTER SET utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE TABLE `Assets` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `AssetTag` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `AssetName` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
        `Category` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Status` int NOT NULL,
        `PurchaseDate` datetime(6) NULL,
        `WarrantyExpiry` datetime(6) NULL,
        CONSTRAINT `PK_Assets` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE TABLE `Categories` (
        `CategoryId` int NOT NULL AUTO_INCREMENT,
        `CategoryName` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Description` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `IsActive` tinyint(1) NOT NULL DEFAULT TRUE,
        `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT `PK_Categories` PRIMARY KEY (`CategoryId`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE TABLE `Departments` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Name` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_Departments` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE TABLE `Roles` (
        `RoleId` int NOT NULL AUTO_INCREMENT,
        `RoleName` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Description` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        CONSTRAINT `PK_Roles` PRIMARY KEY (`RoleId`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE TABLE `Users` (
        `UserId` int NOT NULL AUTO_INCREMENT,
        `Username` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Email` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `PasswordHash` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `FirstName` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `LastName` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `PhoneNumber` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `RoleId` int NOT NULL,
        `IsActive` tinyint(1) NOT NULL DEFAULT TRUE,
        `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
        `UpdatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT `PK_Users` PRIMARY KEY (`UserId`),
        CONSTRAINT `FK_Users_Roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `Roles` (`RoleId`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE TABLE `ActivityLogs` (
        `LogId` int NOT NULL AUTO_INCREMENT,
        `UserId` int NULL,
        `Entity` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `EntityId` int NULL,
        `Action` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `OldValue` longtext CHARACTER SET utf8mb4 NOT NULL,
        `NewValue` longtext CHARACTER SET utf8mb4 NOT NULL,
        `IPAddress` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `LoggedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT `PK_ActivityLogs` PRIMARY KEY (`LogId`),
        CONSTRAINT `FK_ActivityLogs_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`UserId`) ON DELETE SET NULL
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE TABLE `Employees` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `UserId` int NOT NULL,
        `EmployeeCode` varchar(20) CHARACTER SET utf8mb4 NULL,
        `DepartmentId` int NOT NULL,
        `Position` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Status` int NOT NULL,
        CONSTRAINT `PK_Employees` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Employees_Departments_DepartmentId` FOREIGN KEY (`DepartmentId`) REFERENCES `Departments` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_Employees_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`UserId`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE TABLE `AssetAssignments` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `AssetId` int NOT NULL,
        `EmployeeId` int NOT NULL,
        `AssignedDate` datetime(6) NOT NULL,
        `ReturnedDate` datetime(6) NULL,
        CONSTRAINT `PK_AssetAssignments` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_AssetAssignments_Assets_AssetId` FOREIGN KEY (`AssetId`) REFERENCES `Assets` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_AssetAssignments_Employees_EmployeeId` FOREIGN KEY (`EmployeeId`) REFERENCES `Employees` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE TABLE `ServiceRequests` (
        `RequestId` int NOT NULL AUTO_INCREMENT,
        `RequestNumber` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `Title` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
        `Description` longtext CHARACTER SET utf8mb4 NOT NULL,
        `CategoryId` int NOT NULL,
        `RequestorId` int NOT NULL,
        `AssignedTechnicianId` int NULL,
        `AssetId` int NULL,
        `EmployeeId` int NULL,
        `Status` varchar(255) CHARACTER SET utf8mb4 NOT NULL DEFAULT 'Pending',
        `Priority` varchar(255) CHARACTER SET utf8mb4 NOT NULL DEFAULT 'Medium',
        `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
        `UpdatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
        `ResolvedAt` datetime(6) NULL,
        `ClosedAt` datetime(6) NULL,
        CONSTRAINT `PK_ServiceRequests` PRIMARY KEY (`RequestId`),
        CONSTRAINT `FK_ServiceRequests_Asset` FOREIGN KEY (`AssetId`) REFERENCES `Assets` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_ServiceRequests_AssignedTechnician` FOREIGN KEY (`AssignedTechnicianId`) REFERENCES `Users` (`UserId`) ON DELETE SET NULL,
        CONSTRAINT `FK_ServiceRequests_Categories_CategoryId` FOREIGN KEY (`CategoryId`) REFERENCES `Categories` (`CategoryId`) ON DELETE RESTRICT,
        CONSTRAINT `FK_ServiceRequests_Employee` FOREIGN KEY (`EmployeeId`) REFERENCES `Employees` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_ServiceRequests_Requestor` FOREIGN KEY (`RequestorId`) REFERENCES `Users` (`UserId`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE TABLE `Assignments` (
        `AssignmentId` int NOT NULL AUTO_INCREMENT,
        `RequestId` int NOT NULL,
        `TechnicianId` int NOT NULL,
        `AssignedBy` int NOT NULL,
        `AssignedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
        `IsActive` tinyint(1) NOT NULL DEFAULT TRUE,
        `Notes` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_Assignments` PRIMARY KEY (`AssignmentId`),
        CONSTRAINT `FK_Assignments_AssignedBy` FOREIGN KEY (`AssignedBy`) REFERENCES `Users` (`UserId`) ON DELETE RESTRICT,
        CONSTRAINT `FK_Assignments_ServiceRequests_RequestId` FOREIGN KEY (`RequestId`) REFERENCES `ServiceRequests` (`RequestId`) ON DELETE CASCADE,
        CONSTRAINT `FK_Assignments_Technician` FOREIGN KEY (`TechnicianId`) REFERENCES `Users` (`UserId`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE TABLE `Feedbacks` (
        `FeedbackId` int NOT NULL AUTO_INCREMENT,
        `RequestId` int NOT NULL,
        `Rating` int NOT NULL,
        `Comments` varchar(1000) CHARACTER SET utf8mb4 NOT NULL,
        `ProvidedBy` int NOT NULL,
        `ProvidedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT `PK_Feedbacks` PRIMARY KEY (`FeedbackId`),
        CONSTRAINT `FK_Feedbacks_ServiceRequests_RequestId` FOREIGN KEY (`RequestId`) REFERENCES `ServiceRequests` (`RequestId`) ON DELETE CASCADE,
        CONSTRAINT `FK_Feedbacks_Users_ProvidedBy` FOREIGN KEY (`ProvidedBy`) REFERENCES `Users` (`UserId`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    INSERT INTO `Categories` (`CategoryId`, `CategoryName`, `CreatedAt`, `Description`, `IsActive`)
    VALUES (1, 'Hardware', TIMESTAMP '2026-05-04 07:35:13', 'Hardware related issues and requests', TRUE),
    (2, 'Software', TIMESTAMP '2026-05-04 07:35:13', 'Software installation and support', TRUE),
    (3, 'Network', TIMESTAMP '2026-05-04 07:35:13', 'Network connectivity issues', TRUE),
    (4, 'Email', TIMESTAMP '2026-05-04 07:35:13', 'Email and collaboration tools', TRUE),
    (5, 'Security', TIMESTAMP '2026-05-04 07:35:13', 'Security related issues', TRUE),
    (6, 'Other', TIMESTAMP '2026-05-04 07:35:13', 'Other miscellaneous requests', TRUE);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    INSERT INTO `Departments` (`Id`, `Name`)
    VALUES (1, 'Information Technology'),
    (2, 'Human Resources'),
    (3, 'Finance'),
    (4, 'Operations'),
    (5, 'Marketing');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    INSERT INTO `Roles` (`RoleId`, `CreatedAt`, `Description`, `RoleName`)
    VALUES (1, TIMESTAMP '2026-05-04 07:35:12', 'IT Administrator with full access', 'Admin'),
    (2, TIMESTAMP '2026-05-04 07:35:12', 'IT Support Technician', 'Technician'),
    (3, TIMESTAMP '2026-05-04 07:35:12', 'Employee / Client / Requestor', 'Client'),
    (4, TIMESTAMP '2026-05-04 07:35:12', 'System Super Administrator with unrestricted access', 'SuperAdmin');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    INSERT INTO `Users` (`UserId`, `CreatedAt`, `Email`, `FirstName`, `IsActive`, `LastName`, `PasswordHash`, `PhoneNumber`, `RoleId`, `UpdatedAt`, `Username`)
    VALUES (-1, TIMESTAMP '2026-05-04 07:35:12', 'superadmin@itsms.local', 'Super', TRUE, 'Admin', 'AQAAAAIAAYagAAAAEK9NgXj5QNm6UkFBO4Doeh8Iocl1jlqXSHFXAqxCc7DqtKdhH4v17woNqr2CwvaGVA==', '', 4, TIMESTAMP '2026-05-04 07:35:12', 'superadmin');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE INDEX `IX_ActivityLogs_Entity_EntityId` ON `ActivityLogs` (`Entity`, `EntityId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE INDEX `IX_ActivityLogs_LoggedAt` ON `ActivityLogs` (`LoggedAt`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE INDEX `IX_ActivityLogs_UserId` ON `ActivityLogs` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE INDEX `IX_AssetAssignments_AssetId` ON `AssetAssignments` (`AssetId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE INDEX `IX_AssetAssignments_EmployeeId` ON `AssetAssignments` (`EmployeeId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Assets_AssetTag` ON `Assets` (`AssetTag`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE INDEX `IX_Assignments_AssignedBy` ON `Assignments` (`AssignedBy`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE INDEX `IX_Assignments_IsActive` ON `Assignments` (`IsActive`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE INDEX `IX_Assignments_RequestId` ON `Assignments` (`RequestId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE INDEX `IX_Assignments_TechnicianId` ON `Assignments` (`TechnicianId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE INDEX `IX_Employees_DepartmentId` ON `Employees` (`DepartmentId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Employees_UserId` ON `Employees` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE INDEX `IX_Feedbacks_ProvidedAt` ON `Feedbacks` (`ProvidedAt`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE INDEX `IX_Feedbacks_ProvidedBy` ON `Feedbacks` (`ProvidedBy`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE INDEX `IX_Feedbacks_Rating` ON `Feedbacks` (`Rating`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Feedbacks_RequestId` ON `Feedbacks` (`RequestId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Roles_RoleName` ON `Roles` (`RoleName`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE INDEX `IX_ServiceRequests_AssetId` ON `ServiceRequests` (`AssetId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE INDEX `IX_ServiceRequests_AssignedTechnicianId` ON `ServiceRequests` (`AssignedTechnicianId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE INDEX `IX_ServiceRequests_CategoryId` ON `ServiceRequests` (`CategoryId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE INDEX `IX_ServiceRequests_CreatedAt` ON `ServiceRequests` (`CreatedAt`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE INDEX `IX_ServiceRequests_EmployeeId` ON `ServiceRequests` (`EmployeeId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE INDEX `IX_ServiceRequests_Priority` ON `ServiceRequests` (`Priority`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_ServiceRequests_RequestNumber` ON `ServiceRequests` (`RequestNumber`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE INDEX `IX_ServiceRequests_RequestorId` ON `ServiceRequests` (`RequestorId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE INDEX `IX_ServiceRequests_Status` ON `ServiceRequests` (`Status`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Users_Email` ON `Users` (`Email`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE INDEX `IX_Users_RoleId` ON `Users` (`RoleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Users_Username` ON `Users` (`Username`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504073517_InitialCreate') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260504073517_InitialCreate', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504075712_UpdateRoleName') THEN

    UPDATE `Categories` SET `CreatedAt` = TIMESTAMP '2026-05-04 07:57:08'
    WHERE `CategoryId` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504075712_UpdateRoleName') THEN

    UPDATE `Categories` SET `CreatedAt` = TIMESTAMP '2026-05-04 07:57:08'
    WHERE `CategoryId` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504075712_UpdateRoleName') THEN

    UPDATE `Categories` SET `CreatedAt` = TIMESTAMP '2026-05-04 07:57:08'
    WHERE `CategoryId` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504075712_UpdateRoleName') THEN

    UPDATE `Categories` SET `CreatedAt` = TIMESTAMP '2026-05-04 07:57:08'
    WHERE `CategoryId` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504075712_UpdateRoleName') THEN

    UPDATE `Categories` SET `CreatedAt` = TIMESTAMP '2026-05-04 07:57:08'
    WHERE `CategoryId` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504075712_UpdateRoleName') THEN

    UPDATE `Categories` SET `CreatedAt` = TIMESTAMP '2026-05-04 07:57:08'
    WHERE `CategoryId` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504075712_UpdateRoleName') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-05-04 07:57:08'
    WHERE `RoleId` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504075712_UpdateRoleName') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-05-04 07:57:08'
    WHERE `RoleId` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504075712_UpdateRoleName') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-05-04 07:57:08', `Description` = 'Employee / Requestor', `RoleName` = 'Employee'
    WHERE `RoleId` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504075712_UpdateRoleName') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-05-04 07:57:08'
    WHERE `RoleId` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504075712_UpdateRoleName') THEN

    UPDATE `Users` SET `CreatedAt` = TIMESTAMP '2026-05-04 07:57:08', `PasswordHash` = 'AQAAAAIAAYagAAAAEA31Wt1vhrOv8fcB/Lcgk3Fi+nnYER2GJj3V/2imlPavmBknpzg99V55fb+TaKYADw==', `UpdatedAt` = TIMESTAMP '2026-05-04 07:57:08'
    WHERE `UserId` = -1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504075712_UpdateRoleName') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260504075712_UpdateRoleName', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504114157_AddTicketComments') THEN

    CREATE TABLE `ticketcomments` (
        `CommentId` int NOT NULL AUTO_INCREMENT,
        `RequestId` int NOT NULL,
        `AuthorId` int NOT NULL,
        `Body` longtext CHARACTER SET utf8mb4 NOT NULL,
        `IsInternal` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `ServiceRequestRequestId` int NULL,
        CONSTRAINT `PK_ticketcomments` PRIMARY KEY (`CommentId`),
        CONSTRAINT `FK_ticketcomments_ServiceRequests_RequestId` FOREIGN KEY (`RequestId`) REFERENCES `ServiceRequests` (`RequestId`) ON DELETE CASCADE,
        CONSTRAINT `FK_ticketcomments_ServiceRequests_ServiceRequestRequestId` FOREIGN KEY (`ServiceRequestRequestId`) REFERENCES `ServiceRequests` (`RequestId`),
        CONSTRAINT `FK_ticketcomments_Users_AuthorId` FOREIGN KEY (`AuthorId`) REFERENCES `Users` (`UserId`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504114157_AddTicketComments') THEN

    UPDATE `Categories` SET `CreatedAt` = TIMESTAMP '2026-05-04 11:41:54'
    WHERE `CategoryId` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504114157_AddTicketComments') THEN

    UPDATE `Categories` SET `CreatedAt` = TIMESTAMP '2026-05-04 11:41:54'
    WHERE `CategoryId` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504114157_AddTicketComments') THEN

    UPDATE `Categories` SET `CreatedAt` = TIMESTAMP '2026-05-04 11:41:54'
    WHERE `CategoryId` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504114157_AddTicketComments') THEN

    UPDATE `Categories` SET `CreatedAt` = TIMESTAMP '2026-05-04 11:41:54'
    WHERE `CategoryId` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504114157_AddTicketComments') THEN

    UPDATE `Categories` SET `CreatedAt` = TIMESTAMP '2026-05-04 11:41:54'
    WHERE `CategoryId` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504114157_AddTicketComments') THEN

    UPDATE `Categories` SET `CreatedAt` = TIMESTAMP '2026-05-04 11:41:54'
    WHERE `CategoryId` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504114157_AddTicketComments') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-05-04 11:41:54'
    WHERE `RoleId` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504114157_AddTicketComments') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-05-04 11:41:54'
    WHERE `RoleId` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504114157_AddTicketComments') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-05-04 11:41:54'
    WHERE `RoleId` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504114157_AddTicketComments') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-05-04 11:41:54'
    WHERE `RoleId` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504114157_AddTicketComments') THEN

    UPDATE `Users` SET `CreatedAt` = TIMESTAMP '2026-05-04 11:41:54', `PasswordHash` = 'AQAAAAIAAYagAAAAEO/hhjZHBR4IqV1taqdpFDzs5YurJUG9dwxFgvDlc8dHqHXMECgSUk8pwiDh1tl6ig==', `UpdatedAt` = TIMESTAMP '2026-05-04 11:41:54'
    WHERE `UserId` = -1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504114157_AddTicketComments') THEN

    CREATE INDEX `IX_ticketcomments_AuthorId` ON `ticketcomments` (`AuthorId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504114157_AddTicketComments') THEN

    CREATE INDEX `IX_ticketcomments_RequestId` ON `ticketcomments` (`RequestId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504114157_AddTicketComments') THEN

    CREATE INDEX `IX_ticketcomments_ServiceRequestRequestId` ON `ticketcomments` (`ServiceRequestRequestId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504114157_AddTicketComments') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260504114157_AddTicketComments', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504133741_FixTicketCommentRelationship') THEN

    ALTER TABLE `ticketcomments` DROP FOREIGN KEY `FK_ticketcomments_ServiceRequests_ServiceRequestRequestId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504133741_FixTicketCommentRelationship') THEN

    ALTER TABLE `ticketcomments` DROP INDEX `IX_ticketcomments_ServiceRequestRequestId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504133741_FixTicketCommentRelationship') THEN

    ALTER TABLE `ticketcomments` DROP COLUMN `ServiceRequestRequestId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504133741_FixTicketCommentRelationship') THEN

    UPDATE `Categories` SET `CreatedAt` = TIMESTAMP '2026-05-04 13:37:37'
    WHERE `CategoryId` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504133741_FixTicketCommentRelationship') THEN

    UPDATE `Categories` SET `CreatedAt` = TIMESTAMP '2026-05-04 13:37:37'
    WHERE `CategoryId` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504133741_FixTicketCommentRelationship') THEN

    UPDATE `Categories` SET `CreatedAt` = TIMESTAMP '2026-05-04 13:37:37'
    WHERE `CategoryId` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504133741_FixTicketCommentRelationship') THEN

    UPDATE `Categories` SET `CreatedAt` = TIMESTAMP '2026-05-04 13:37:37'
    WHERE `CategoryId` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504133741_FixTicketCommentRelationship') THEN

    UPDATE `Categories` SET `CreatedAt` = TIMESTAMP '2026-05-04 13:37:37'
    WHERE `CategoryId` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504133741_FixTicketCommentRelationship') THEN

    UPDATE `Categories` SET `CreatedAt` = TIMESTAMP '2026-05-04 13:37:37'
    WHERE `CategoryId` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504133741_FixTicketCommentRelationship') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-05-04 13:37:37'
    WHERE `RoleId` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504133741_FixTicketCommentRelationship') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-05-04 13:37:37'
    WHERE `RoleId` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504133741_FixTicketCommentRelationship') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-05-04 13:37:37'
    WHERE `RoleId` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504133741_FixTicketCommentRelationship') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-05-04 13:37:37'
    WHERE `RoleId` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504133741_FixTicketCommentRelationship') THEN

    UPDATE `Users` SET `CreatedAt` = TIMESTAMP '2026-05-04 13:37:37', `PasswordHash` = 'AQAAAAIAAYagAAAAECpna8TzbeXXhAV0Jl+BGZRRnABlZfIKiXVBnR+OQYN0MzX/D2/LV2ZHFWdEv76iuQ==', `UpdatedAt` = TIMESTAMP '2026-05-04 13:37:37'
    WHERE `UserId` = -1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504133741_FixTicketCommentRelationship') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260504133741_FixTicketCommentRelationship', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504160310_AddAuditLogs') THEN

    CREATE TABLE `AuditLogs` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `UserId` int NOT NULL,
        `Action` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Module` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Description` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT `PK_AuditLogs` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_AuditLogs_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`UserId`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504160310_AddAuditLogs') THEN

    UPDATE `Categories` SET `CreatedAt` = TIMESTAMP '2026-05-05 00:03:06'
    WHERE `CategoryId` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504160310_AddAuditLogs') THEN

    UPDATE `Categories` SET `CreatedAt` = TIMESTAMP '2026-05-05 00:03:06'
    WHERE `CategoryId` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504160310_AddAuditLogs') THEN

    UPDATE `Categories` SET `CreatedAt` = TIMESTAMP '2026-05-05 00:03:06'
    WHERE `CategoryId` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504160310_AddAuditLogs') THEN

    UPDATE `Categories` SET `CreatedAt` = TIMESTAMP '2026-05-05 00:03:06'
    WHERE `CategoryId` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504160310_AddAuditLogs') THEN

    UPDATE `Categories` SET `CreatedAt` = TIMESTAMP '2026-05-05 00:03:06'
    WHERE `CategoryId` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504160310_AddAuditLogs') THEN

    UPDATE `Categories` SET `CreatedAt` = TIMESTAMP '2026-05-05 00:03:06'
    WHERE `CategoryId` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504160310_AddAuditLogs') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-05-05 00:03:06'
    WHERE `RoleId` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504160310_AddAuditLogs') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-05-05 00:03:06'
    WHERE `RoleId` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504160310_AddAuditLogs') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-05-05 00:03:06'
    WHERE `RoleId` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504160310_AddAuditLogs') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-05-05 00:03:06'
    WHERE `RoleId` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504160310_AddAuditLogs') THEN

    UPDATE `Users` SET `CreatedAt` = TIMESTAMP '2026-05-05 00:03:06', `PasswordHash` = 'AQAAAAIAAYagAAAAED7SzZUtia6yazrHss+AsDSSc4P6kN5qpUTcCHcxQHLI0p+oAzwTUcF+BXTDY6dhZA==', `UpdatedAt` = TIMESTAMP '2026-05-05 00:03:06'
    WHERE `UserId` = -1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504160310_AddAuditLogs') THEN

    CREATE INDEX `IX_AuditLogs_UserId` ON `AuditLogs` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260504160310_AddAuditLogs') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260504160310_AddAuditLogs', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260511113633_AddFinanceAndPayrollModule') THEN

    ALTER TABLE `Employees` ADD `EmployeeNumber` varchar(20) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260511113633_AddFinanceAndPayrollModule') THEN

    ALTER TABLE `Employees` ADD `EmploymentStatus` varchar(50) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260511113633_AddFinanceAndPayrollModule') THEN

    ALTER TABLE `Employees` ADD `HireDate` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260511113633_AddFinanceAndPayrollModule') THEN

    ALTER TABLE `Employees` ADD `SalaryRate` decimal(18,2) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260511113633_AddFinanceAndPayrollModule') THEN

    CREATE TABLE `FinanceTransactions` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `ServiceRequestId` int NULL,
        `AssetId` int NULL,
        `DepartmentId` int NULL,
        `TransactionType` int NOT NULL,
        `Amount` decimal(18,2) NOT NULL,
        `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
        `TransactionDate` datetime(6) NOT NULL,
        `CreatedByUserId` int NOT NULL,
        CONSTRAINT `PK_FinanceTransactions` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_FinanceTransactions_Assets_AssetId` FOREIGN KEY (`AssetId`) REFERENCES `Assets` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_FinanceTransactions_Departments_DepartmentId` FOREIGN KEY (`DepartmentId`) REFERENCES `Departments` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_FinanceTransactions_ServiceRequests_ServiceRequestId` FOREIGN KEY (`ServiceRequestId`) REFERENCES `ServiceRequests` (`RequestId`) ON DELETE SET NULL,
        CONSTRAINT `FK_FinanceTransactions_Users_CreatedByUserId` FOREIGN KEY (`CreatedByUserId`) REFERENCES `Users` (`UserId`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260511113633_AddFinanceAndPayrollModule') THEN

    CREATE TABLE `Payrolls` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `EmployeeId` int NOT NULL,
        `PayrollMonth` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `BasicSalary` decimal(18,2) NOT NULL,
        `Allowance` decimal(18,2) NOT NULL,
        `Deduction` decimal(18,2) NOT NULL,
        `OvertimePay` decimal(18,2) NOT NULL,
        `NetSalary` decimal(18,2) NOT NULL,
        `PayrollStatus` int NOT NULL,
        CONSTRAINT `PK_Payrolls` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Payrolls_Employees_EmployeeId` FOREIGN KEY (`EmployeeId`) REFERENCES `Employees` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260511113633_AddFinanceAndPayrollModule') THEN

    UPDATE `Categories` SET `CreatedAt` = TIMESTAMP '2026-05-11 19:36:30'
    WHERE `CategoryId` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260511113633_AddFinanceAndPayrollModule') THEN

    UPDATE `Categories` SET `CreatedAt` = TIMESTAMP '2026-05-11 19:36:30'
    WHERE `CategoryId` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260511113633_AddFinanceAndPayrollModule') THEN

    UPDATE `Categories` SET `CreatedAt` = TIMESTAMP '2026-05-11 19:36:30'
    WHERE `CategoryId` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260511113633_AddFinanceAndPayrollModule') THEN

    UPDATE `Categories` SET `CreatedAt` = TIMESTAMP '2026-05-11 19:36:30'
    WHERE `CategoryId` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260511113633_AddFinanceAndPayrollModule') THEN

    UPDATE `Categories` SET `CreatedAt` = TIMESTAMP '2026-05-11 19:36:30'
    WHERE `CategoryId` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260511113633_AddFinanceAndPayrollModule') THEN

    UPDATE `Categories` SET `CreatedAt` = TIMESTAMP '2026-05-11 19:36:30'
    WHERE `CategoryId` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260511113633_AddFinanceAndPayrollModule') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-05-11 19:36:29'
    WHERE `RoleId` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260511113633_AddFinanceAndPayrollModule') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-05-11 19:36:29'
    WHERE `RoleId` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260511113633_AddFinanceAndPayrollModule') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-05-11 19:36:29'
    WHERE `RoleId` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260511113633_AddFinanceAndPayrollModule') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-05-11 19:36:29'
    WHERE `RoleId` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260511113633_AddFinanceAndPayrollModule') THEN

    UPDATE `Users` SET `CreatedAt` = TIMESTAMP '2026-05-11 19:36:29', `PasswordHash` = 'AQAAAAIAAYagAAAAEJss/Y9SZAxlWAwJlyOnu2m8/4ImAXWkJf0St2gmRT/+6uxNj8IanEGySoiFYCcHiA==', `UpdatedAt` = TIMESTAMP '2026-05-11 19:36:29'
    WHERE `UserId` = -1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260511113633_AddFinanceAndPayrollModule') THEN

    CREATE INDEX `IX_FinanceTransactions_AssetId` ON `FinanceTransactions` (`AssetId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260511113633_AddFinanceAndPayrollModule') THEN

    CREATE INDEX `IX_FinanceTransactions_CreatedByUserId` ON `FinanceTransactions` (`CreatedByUserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260511113633_AddFinanceAndPayrollModule') THEN

    CREATE INDEX `IX_FinanceTransactions_DepartmentId` ON `FinanceTransactions` (`DepartmentId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260511113633_AddFinanceAndPayrollModule') THEN

    CREATE INDEX `IX_FinanceTransactions_ServiceRequestId` ON `FinanceTransactions` (`ServiceRequestId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260511113633_AddFinanceAndPayrollModule') THEN

    CREATE INDEX `IX_Payrolls_EmployeeId` ON `Payrolls` (`EmployeeId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260511113633_AddFinanceAndPayrollModule') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260511113633_AddFinanceAndPayrollModule', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

