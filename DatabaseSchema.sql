-- AreYouOk Database Schema
-- SQL Server Database Creation Script

USE master;
GO

-- Create database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'AreYouOkDb')
BEGIN
    CREATE DATABASE AreYouOkDb;
END
GO

USE AreYouOkDb;
GO

-- Users Table
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PhoneNumber NVARCHAR(20) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    INDEX IX_Users_Email (Email),
    INDEX IX_Users_PhoneNumber (PhoneNumber)
);
GO

-- Journeys Table
CREATE TABLE Journeys (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    StartTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    EndTime DATETIME2 NULL,
    CheckInIntervalHours FLOAT NOT NULL CHECK (CheckInIntervalHours >= 0.5 AND CheckInIntervalHours <= 168),
    LastCheckInTime DATETIME2 NULL,
    Status INT NOT NULL DEFAULT 1, -- 1=Active, 2=Completed, 3=Alert, 4=Cancelled
    Latitude FLOAT NULL,
    Longitude FLOAT NULL,
    LastLocationUpdate DATETIME2 NULL,
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    INDEX IX_Journeys_UserId (UserId),
    INDEX IX_Journeys_Status (Status),
    INDEX IX_Journeys_UserId_Status (UserId, Status)
);
GO

-- FamilyMembers Table
CREATE TABLE FamilyMembers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    JourneyId INT NOT NULL,
    PhoneNumber NVARCHAR(20) NOT NULL,
    UserId INT NULL, -- Nullable - family member might not be a registered user
    DisplayName NVARCHAR(200) NULL,
    DeviceToken NVARCHAR(500) NULL,
    ReceiveNotifications BIT NOT NULL DEFAULT 1,
    AddedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (JourneyId) REFERENCES Journeys(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL,
    INDEX IX_FamilyMembers_JourneyId (JourneyId),
    INDEX IX_FamilyMembers_PhoneNumber (PhoneNumber),
    INDEX IX_FamilyMembers_UserId (UserId),
    INDEX IX_FamilyMembers_JourneyId_PhoneNumber (JourneyId, PhoneNumber)
);
GO

-- SafetyStatuses Table
CREATE TABLE SafetyStatuses (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    JourneyId INT NOT NULL,
    IsSafe BIT NOT NULL,
    Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Latitude FLOAT NULL,
    Longitude FLOAT NULL,
    Message NVARCHAR(500) NULL,
    UpdateType INT NOT NULL, -- 1=ManualSafe, 2=ManualAlert, 3=AutomaticAlert
    
    FOREIGN KEY (JourneyId) REFERENCES Journeys(Id) ON DELETE CASCADE,
    INDEX IX_SafetyStatuses_JourneyId (JourneyId),
    INDEX IX_SafetyStatuses_Timestamp (Timestamp),
    INDEX IX_SafetyStatuses_JourneyId_Timestamp (JourneyId, Timestamp)
);
GO

-- Sample data (optional)
-- Uncomment to insert test data

/*
-- Insert test user
INSERT INTO Users (FirstName, LastName, Email, PhoneNumber, PasswordHash)
VALUES 
    ('John', 'Doe', 'john@example.com', '+1234567890', '$2a$11$examplehashhere'),
    ('Jane', 'Smith', 'jane@example.com', '+9876543210', '$2a$11$examplehashhere');

-- Insert test journey
DECLARE @UserId INT = (SELECT Id FROM Users WHERE Email = 'john@example.com');
INSERT INTO Journeys (UserId, CheckInIntervalHours, LastCheckInTime, Status)
VALUES (@UserId, 10, GETUTCDATE(), 1);

-- Insert family member
DECLARE @JourneyId INT = SCOPE_IDENTITY();
INSERT INTO FamilyMembers (JourneyId, PhoneNumber, DisplayName)
VALUES (@JourneyId, '+9876543210', 'Jane Smith');

-- Insert safety status
INSERT INTO SafetyStatuses (JourneyId, IsSafe, UpdateType)
VALUES (@JourneyId, 1, 1);
*/

GO

PRINT 'Database schema created successfully!';
PRINT 'Tables created: Users, Journeys, FamilyMembers, SafetyStatuses';
GO
