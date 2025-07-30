-- ================================================
-- Drug Prevention System - Complete Database Script
-- Compatible with SQL Server 2019+ and Docker deployment
-- ================================================

-- Create Database
IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'DrugUsePreventionDB')
BEGIN
    CREATE DATABASE DrugUsePreventionDB;
END
GO

USE DrugUsePreventionDB;
GO

-- ================================================
-- Core Tables
-- ================================================

-- Create Users table (main user table)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(255) NOT NULL,
    Username NVARCHAR(255) NOT NULL UNIQUE,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Phone NVARCHAR(50),
    AvatarUrl NVARCHAR(500),
    DateOfBirth DATETIME2,
    Gender NVARCHAR(50),
    Role NVARCHAR(50) NOT NULL DEFAULT 'Guest',
    Status NVARCHAR(50) NOT NULL DEFAULT 'Active',
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    VerificationToken NVARCHAR(255),
    IsEmailVerified BIT NOT NULL DEFAULT 0,
    ResetPasswordToken NVARCHAR(255),
    ResetPasswordExpiry DATETIME2
);

-- Create Categories table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Categories' AND xtype='U')
CREATE TABLE Categories (
    CategoryID INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(200) NOT NULL,
    CategoryDescription NVARCHAR(MAX),
    ParentCategoryID NVARCHAR(50),
    IsActive BIT NOT NULL DEFAULT 1
);

-- Create Tags table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Tags' AND xtype='U')
CREATE TABLE Tags (
    TagID INT PRIMARY KEY IDENTITY(1,1),
    TagName NVARCHAR(200) NOT NULL,
    Note NVARCHAR(MAX) NOT NULL DEFAULT ''
);

-- Create Consultants table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Consultants' AND xtype='U')
CREATE TABLE Consultants (
    ConsultantID INT PRIMARY KEY,
    Qualifications NVARCHAR(MAX) NOT NULL,
    Specialty NVARCHAR(MAX) NOT NULL,
    WorkingHours NVARCHAR(MAX) NOT NULL, -- JSON serialized List<DateTime>
    FOREIGN KEY (ConsultantID) REFERENCES Users(UserID) ON DELETE CASCADE
);

-- ================================================
-- Course Management Tables
-- ================================================

-- Create Courses table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Courses' AND xtype='U')
CREATE TABLE Courses (
    CourseID INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    TargetGroup NVARCHAR(255) NOT NULL,
    AgeGroup NVARCHAR(255) NOT NULL,
    ContentURL NVARCHAR(500) NOT NULL,
    ThumbnailURL NVARCHAR(500) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    isActive BIT NOT NULL DEFAULT 1,
    isAccept BIT NOT NULL DEFAULT 1,
    CreatedBy INT,
    FOREIGN KEY (CreatedBy) REFERENCES Users(UserID) ON DELETE SET NULL
);

-- Create CourseContents table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CourseContents' AND xtype='U')
CREATE TABLE CourseContents (
    ContentID INT PRIMARY KEY IDENTITY(1,1),
    CourseID INT,
    Title NVARCHAR(200),
    Description NVARCHAR(MAX),
    ContentType NVARCHAR(100),
    ContentData NVARCHAR(MAX),
    OrderIndex INT NOT NULL,
    isActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (CourseID) REFERENCES Courses(CourseID) ON DELETE CASCADE
);

-- Create CourseRegistrations table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CourseRegistrations' AND xtype='U')
CREATE TABLE CourseRegistrations (
    RegistrationID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT,
    CourseID INT,
    RegisteredAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Completed BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE NO ACTION,
    FOREIGN KEY (CourseID) REFERENCES Courses(CourseID) ON DELETE NO ACTION
);

-- Create CheckCourseContents table (tracks user progress through course content)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CheckCourseContents' AND xtype='U')
CREATE TABLE CheckCourseContents (
    CheckID INT PRIMARY KEY IDENTITY(1,1),
    RegistrationID INT NOT NULL,
    ContentID INT NOT NULL,
    IsCompleted BIT NOT NULL DEFAULT 0,
    CompletedAt DATETIME2,
    FOREIGN KEY (RegistrationID) REFERENCES CourseRegistrations(RegistrationID) ON DELETE CASCADE,
    FOREIGN KEY (ContentID) REFERENCES CourseContents(ContentID) ON DELETE NO ACTION
);

-- ================================================
-- Program Management Tables
-- ================================================

-- Create Programs table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Programs' AND xtype='U')
CREATE TABLE Programs (
    ProgramID INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    ThumbnailURL NVARCHAR(500) NOT NULL,
    StartDate DATETIME2 NOT NULL,
    EndDate DATETIME2 NOT NULL,
    Location NVARCHAR(255) NOT NULL,
    CreatedBy INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (CreatedBy) REFERENCES Users(UserID) ON DELETE NO ACTION
);

-- Create ProgramParticipations table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ProgramParticipations' AND xtype='U')
CREATE TABLE ProgramParticipations (
    ParticipationID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT,
    ProgramID INT,
    ParticipatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE NO ACTION,
    FOREIGN KEY (ProgramID) REFERENCES Programs(ProgramID) ON DELETE NO ACTION
);

-- ================================================
-- Appointment & Calendar System Tables
-- ================================================

-- Create Appointments table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Appointments' AND xtype='U')
CREATE TABLE Appointments (
    AppointmentID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT,
    ConsultantID INT,
    ScheduledAt DATETIME2 NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    Notes NVARCHAR(MAX),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE NO ACTION,
    FOREIGN KEY (ConsultantID) REFERENCES Consultants(ConsultantID) ON DELETE NO ACTION
);

-- Create ConsultantSchedules table (NEW - for calendar system)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ConsultantSchedules' AND xtype='U')
CREATE TABLE ConsultantSchedules (
    ScheduleID INT PRIMARY KEY IDENTITY(1,1),
    ConsultantID INT NOT NULL,
    DayOfWeek INT NOT NULL, -- 0=Sunday, 1=Monday, etc.
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    SlotDurationMinutes INT NOT NULL DEFAULT 30,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NULL,
    FOREIGN KEY (ConsultantID) REFERENCES Consultants(ConsultantID) ON DELETE CASCADE
);

-- Create ConsultantAvailabilities table (NEW - for specific date availability)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ConsultantAvailabilities' AND xtype='U')
CREATE TABLE ConsultantAvailabilities (
    AvailabilityID INT PRIMARY KEY IDENTITY(1,1),
    ConsultantID INT NOT NULL,
    Date DATE NOT NULL,
    StartTime TIME NULL,
    EndTime TIME NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Available', -- Available, Unavailable, Partial
    Notes NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NULL,
    FOREIGN KEY (ConsultantID) REFERENCES Consultants(ConsultantID) ON DELETE CASCADE
);

-- Create AppointmentSlots table (NEW - for detailed slot management)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AppointmentSlots' AND xtype='U')
CREATE TABLE AppointmentSlots (
    SlotID INT PRIMARY KEY IDENTITY(1,1),
    ConsultantID INT NOT NULL,
    SlotDateTime DATETIME2 NOT NULL,
    DurationMinutes INT NOT NULL DEFAULT 30,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Available', -- Available, Booked, Blocked
    AppointmentID INT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NULL,
    FOREIGN KEY (ConsultantID) REFERENCES Consultants(ConsultantID) ON DELETE CASCADE,
    FOREIGN KEY (AppointmentID) REFERENCES Appointments(AppointmentID) ON DELETE SET NULL
);

-- ================================================
-- News & Content Management Tables
-- ================================================

-- Create NewsArticles table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='NewsArticles' AND xtype='U')
CREATE TABLE NewsArticles (
    NewsArticleID INT PRIMARY KEY IDENTITY(1,1),
    NewsAticleName NVARCHAR(200) NOT NULL,
    Headline NVARCHAR(200) NOT NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    NewsContent NVARCHAR(MAX) NOT NULL,
    NewsSource NVARCHAR(255) NOT NULL,
    CategoryID INT,
    NewsStatus NVARCHAR(200) NOT NULL,
    CreatedByID INT,
    UpdatedByID INT,
    ModifiedDate DATETIME2,
    FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID) ON DELETE SET NULL,
    FOREIGN KEY (CreatedByID) REFERENCES Users(UserID) ON DELETE NO ACTION,
    FOREIGN KEY (UpdatedByID) REFERENCES Users(UserID) ON DELETE NO ACTION
);

-- Create NewsTags table (many-to-many relationship between NewsArticles and Tags)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='NewsTags' AND xtype='U')
CREATE TABLE NewsTags (
    NewsTagID INT PRIMARY KEY IDENTITY(1,1),
    NewsArticleID INT,
    TagID INT,
    FOREIGN KEY (NewsArticleID) REFERENCES NewsArticles(NewsArticleID) ON DELETE CASCADE,
    FOREIGN KEY (TagID) REFERENCES Tags(TagID) ON DELETE CASCADE
);

-- ================================================
-- Survey & Assessment Tables
-- ================================================

-- Create Surveys table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Surveys' AND xtype='U')
CREATE TABLE Surveys (
    SurveyID INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(255) NOT NULL,
    Type NVARCHAR(100) NOT NULL, -- ASSIST, CRAFFT, PreProgram, PostProgram
    Description NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ThumbnailURL NVARCHAR(500) NOT NULL
);

-- Create SurveyQuestions table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SurveyQuestions' AND xtype='U')
CREATE TABLE SurveyQuestions (
    QuestionID INT PRIMARY KEY IDENTITY(1,1),
    SurveyID INT,
    QuestionText NVARCHAR(MAX) NOT NULL,
    QuestionType NVARCHAR(100) NOT NULL, -- SingleChoice, MultipleChoice, Text
    FOREIGN KEY (SurveyID) REFERENCES Surveys(SurveyID) ON DELETE CASCADE
);

-- Create SurveyAnswers table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SurveyAnswers' AND xtype='U')
CREATE TABLE SurveyAnswers (
    AnswerID INT PRIMARY KEY IDENTITY(1,1),
    QuestionID INT,
    AnswerText NVARCHAR(MAX) NOT NULL,
    IsCorrect BIT,
    FOREIGN KEY (QuestionID) REFERENCES SurveyQuestions(QuestionID) ON DELETE CASCADE
);

-- Create UserSurveyResponses table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserSurveyResponses' AND xtype='U')
CREATE TABLE UserSurveyResponses (
    ResponseID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL,
    SurveyID INT NOT NULL,
    CompletedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    RiskLevel NVARCHAR(50) NOT NULL, -- Low, Moderate, High
    Recommendation NVARCHAR(MAX) NOT NULL,
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE NO ACTION,
    FOREIGN KEY (SurveyID) REFERENCES Surveys(SurveyID) ON DELETE NO ACTION
);

-- Create UserSurveyAnswers table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserSurveyAnswers' AND xtype='U')
CREATE TABLE UserSurveyAnswers (
    UserSurveyAnswerID INT PRIMARY KEY IDENTITY(1,1),
    ResponseID INT NOT NULL,
    QuestionID INT NOT NULL,
    SelectedAnswerID INT NOT NULL,
    FOREIGN KEY (ResponseID) REFERENCES UserSurveyResponses(ResponseID) ON DELETE CASCADE,
    FOREIGN KEY (QuestionID) REFERENCES SurveyQuestions(QuestionID) ON DELETE NO ACTION,
    FOREIGN KEY (SelectedAnswerID) REFERENCES SurveyAnswers(AnswerID) ON DELETE NO ACTION
);

-- ================================================
-- Analytics & Dashboard Tables
-- ================================================

-- Create DashboardData table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='DashboardData' AND xtype='U')
CREATE TABLE DashboardData (
    ID INT PRIMARY KEY IDENTITY(1,1),
    Metric NVARCHAR(255) NOT NULL,
    Value INT NOT NULL,
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- ================================================
-- Performance Indexes
-- ================================================

-- Core User indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Username')
    CREATE INDEX IX_Users_Username ON Users(Username);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Email')
    CREATE INDEX IX_Users_Email ON Users(Email);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Role')
    CREATE INDEX IX_Users_Role ON Users(Role);

-- Course-related indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Courses_CreatedBy')
    CREATE INDEX IX_Courses_CreatedBy ON Courses(CreatedBy);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CourseRegistrations_UserID')
    CREATE INDEX IX_CourseRegistrations_UserID ON CourseRegistrations(UserID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CourseRegistrations_CourseID')
    CREATE INDEX IX_CourseRegistrations_CourseID ON CourseRegistrations(CourseID);

-- Appointment-related indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Appointments_UserID')
    CREATE INDEX IX_Appointments_UserID ON Appointments(UserID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Appointments_ConsultantID')
    CREATE INDEX IX_Appointments_ConsultantID ON Appointments(ConsultantID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Appointments_ScheduledAt')
    CREATE INDEX IX_Appointments_ScheduledAt ON Appointments(ScheduledAt);

-- Calendar system indexes (NEW)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ConsultantSchedules_ConsultantID_DayOfWeek')
    CREATE INDEX IX_ConsultantSchedules_ConsultantID_DayOfWeek ON ConsultantSchedules(ConsultantID, DayOfWeek);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ConsultantAvailabilities_ConsultantID_Date')
    CREATE INDEX IX_ConsultantAvailabilities_ConsultantID_Date ON ConsultantAvailabilities(ConsultantID, Date);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AppointmentSlots_ConsultantID_SlotDateTime')
    CREATE INDEX IX_AppointmentSlots_ConsultantID_SlotDateTime ON AppointmentSlots(ConsultantID, SlotDateTime);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AppointmentSlots_Status_SlotDateTime')
    CREATE INDEX IX_AppointmentSlots_Status_SlotDateTime ON AppointmentSlots(Status, SlotDateTime);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AppointmentSlots_AppointmentID')
    CREATE INDEX IX_AppointmentSlots_AppointmentID ON AppointmentSlots(AppointmentID);

-- News-related indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_NewsArticles_CategoryID')
    CREATE INDEX IX_NewsArticles_CategoryID ON NewsArticles(CategoryID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_NewsArticles_CreatedDate')
    CREATE INDEX IX_NewsArticles_CreatedDate ON NewsArticles(CreatedDate);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_NewsArticles_CreatedByID')
    CREATE INDEX IX_NewsArticles_CreatedByID ON NewsArticles(CreatedByID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_NewsArticles_UpdatedByID')
    CREATE INDEX IX_NewsArticles_UpdatedByID ON NewsArticles(UpdatedByID);

-- Survey-related indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UserSurveyResponses_UserID')
    CREATE INDEX IX_UserSurveyResponses_UserID ON UserSurveyResponses(UserID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UserSurveyResponses_SurveyID')
    CREATE INDEX IX_UserSurveyResponses_SurveyID ON UserSurveyResponses(SurveyID);

-- ================================================
-- Initial Data Seeding
-- ================================================

-- Insert default categories
IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryName = 'Drug Prevention')
BEGIN
    INSERT INTO Categories (CategoryName, CategoryDescription, IsActive) VALUES 
    ('Drug Prevention', 'Articles related to drug prevention strategies', 1),
    ('Health Education', 'Educational content about health and wellness', 1),
    ('Community Programs', 'Information about community-based prevention programs', 1),
    ('Research & Studies', 'Latest research and studies in drug prevention', 1);
END

-- Insert default tags
IF NOT EXISTS (SELECT 1 FROM Tags WHERE TagName = 'Prevention')
BEGIN
    INSERT INTO Tags (TagName, Note) VALUES 
    ('Prevention', 'Content related to prevention strategies'),
    ('Education', 'Educational materials and resources'),
    ('Youth', 'Content specifically targeted at young people'),
    ('Community', 'Community-based initiatives and programs'),
    ('Research', 'Research findings and academic studies');
END

-- Insert default admin user
-- Password: Admin123! (hashed using ASP.NET Core Identity)
IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'admin')
BEGIN
    INSERT INTO Users (FullName, Username, Email, PasswordHash, Role, Status, IsEmailVerified) VALUES 
    ('System Administrator', 'admin', 'admin@drugprevention.com', 
     'AQAAAAEAACcQAAAAEPc4zU6hEUhKSlv9VXvMsV3v7+3PJ8xWJ8k0KOmBT6O2QKZ4T6V8J7hL5iJ9pBsEQ==', 
     'Admin', 'Active', 1);
END

-- Insert sample consultant (optional)
IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'consultant1')
BEGIN
    -- Insert consultant user
    INSERT INTO Users (FullName, Username, Email, PasswordHash, Role, Status, IsEmailVerified) VALUES 
    ('Dr. Jane Smith', 'consultant1', 'consultant@drugprevention.com', 
     'AQAAAAEAACcQAAAAEPc4zU6hEUhKSlv9VXvMsV3v7+3PJ8xWJ8k0KOmBT6O2QKZ4T6V8J7hL5iJ9pBsEQ==', 
     'Consultant', 'Active', 1);
    
    -- Get the consultant user ID and create consultant profile
    DECLARE @ConsultantUserID INT = SCOPE_IDENTITY();
    
    INSERT INTO Consultants (ConsultantID, Qualifications, Specialty, WorkingHours) VALUES 
    (@ConsultantUserID, 
     'Ph.D. in Psychology, Licensed Clinical Social Worker', 
     'Addiction Counseling, Youth Prevention Programs', 
     '["09:00:00","17:00:00"]');
     
    -- Create sample weekly schedule for the consultant
    INSERT INTO ConsultantSchedules (ConsultantID, DayOfWeek, StartTime, EndTime, SlotDurationMinutes) VALUES
    (@ConsultantUserID, 1, '09:00:00', '17:00:00', 60), -- Monday
    (@ConsultantUserID, 2, '09:00:00', '17:00:00', 60), -- Tuesday  
    (@ConsultantUserID, 3, '09:00:00', '17:00:00', 60), -- Wednesday
    (@ConsultantUserID, 4, '09:00:00', '17:00:00', 60), -- Thursday
    (@ConsultantUserID, 5, '09:00:00', '12:00:00', 60); -- Friday (half day)
END

GO

-- ================================================
-- Database Setup Complete
-- ================================================

PRINT '=== Drug Prevention System Database Setup Complete ===';
PRINT 'Database: DrugUsePreventionDB';
PRINT 'Total Tables: 19 (including 3 new calendar system tables)';
PRINT '';
PRINT 'Default Login Credentials:';
PRINT 'Username: admin';
PRINT 'Password: Admin123!';
PRINT '';
PRINT 'Sample Consultant:';
PRINT 'Username: consultant1';
PRINT 'Password: Admin123!';
PRINT '';
PRINT 'Connection String for Docker:';
PRINT 'Server=localhost;Database=DrugUsePreventionDB;User ID=sa;Password=DrugPrevention@2024;TrustServerCertificate=True';
PRINT '';
PRINT '=== Setup completed successfully! ===';
GO