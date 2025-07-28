-- Create Database
CREATE DATABASE DrugUsePreventionDB;
GO

USE DrugUsePreventionDB;
GO

-- Create Users table (main user table)
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
CREATE TABLE Categories (
    CategoryID INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(200) NOT NULL,
    CategoryDescription NVARCHAR(MAX),
    ParentCategoryID NVARCHAR(50),
    IsActive BIT NOT NULL DEFAULT 1
);

-- Create Tags table
CREATE TABLE Tags (
    TagID INT PRIMARY KEY IDENTITY(1,1),
    TagName NVARCHAR(200) NOT NULL,
    Note NVARCHAR(MAX) NOT NULL DEFAULT ''
);

-- Create Consultants table
CREATE TABLE Consultants (
    ConsultantID INT PRIMARY KEY,
    Qualifications NVARCHAR(MAX) NOT NULL,
    Specialty NVARCHAR(MAX) NOT NULL,
    WorkingHours NVARCHAR(MAX) NOT NULL, -- JSON serialized List<DateTime>
    FOREIGN KEY (ConsultantID) REFERENCES Users(UserID) ON DELETE CASCADE
);

-- Create Courses table
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
CREATE TABLE CheckCourseContents (
    CheckID INT PRIMARY KEY IDENTITY(1,1),
    RegistrationID INT NOT NULL,
    ContentID INT NOT NULL,
    IsCompleted BIT NOT NULL DEFAULT 0,
    CompletedAt DATETIME2,
    FOREIGN KEY (RegistrationID) REFERENCES CourseRegistrations(RegistrationID) ON DELETE CASCADE,
    FOREIGN KEY (ContentID) REFERENCES CourseContents(ContentID) ON DELETE NO ACTION
);

-- Create Programs table
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
CREATE TABLE ProgramParticipations (
    ParticipationID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT,
    ProgramID INT,
    ParticipatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE NO ACTION,
    FOREIGN KEY (ProgramID) REFERENCES Programs(ProgramID) ON DELETE NO ACTION
);

-- Create Appointments table
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

-- Create NewsArticles table (FIXED: All User references set to NO ACTION to avoid cascade cycles)
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
CREATE TABLE NewsTags (
    NewsTagID INT PRIMARY KEY IDENTITY(1,1),
    NewsArticleID INT,
    TagID INT,
    FOREIGN KEY (NewsArticleID) REFERENCES NewsArticles(NewsArticleID) ON DELETE CASCADE,
    FOREIGN KEY (TagID) REFERENCES Tags(TagID) ON DELETE CASCADE
);

-- Create Surveys table
CREATE TABLE Surveys (
    SurveyID INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(255) NOT NULL,
    Type NVARCHAR(100) NOT NULL, -- ASSIST, CRAFFT, PreProgram, PostProgram
    Description NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ThumbnailURL NVARCHAR(500) NOT NULL
);

-- Create SurveyQuestions table
CREATE TABLE SurveyQuestions (
    QuestionID INT PRIMARY KEY IDENTITY(1,1),
    SurveyID INT,
    QuestionText NVARCHAR(MAX) NOT NULL,
    QuestionType NVARCHAR(100) NOT NULL, -- SingleChoice, MultipleChoice, Text
    FOREIGN KEY (SurveyID) REFERENCES Surveys(SurveyID) ON DELETE CASCADE
);

-- Create SurveyAnswers table
CREATE TABLE SurveyAnswers (
    AnswerID INT PRIMARY KEY IDENTITY(1,1),
    QuestionID INT,
    AnswerText NVARCHAR(MAX) NOT NULL,
    IsCorrect BIT,
    FOREIGN KEY (QuestionID) REFERENCES SurveyQuestions(QuestionID) ON DELETE CASCADE
);

-- Create UserSurveyResponses table
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
CREATE TABLE UserSurveyAnswers (
    UserSurveyAnswerID INT PRIMARY KEY IDENTITY(1,1),
    ResponseID INT NOT NULL,
    QuestionID INT NOT NULL,
    SelectedAnswerID INT NOT NULL,
    FOREIGN KEY (ResponseID) REFERENCES UserSurveyResponses(ResponseID) ON DELETE CASCADE,
    FOREIGN KEY (QuestionID) REFERENCES SurveyQuestions(QuestionID) ON DELETE NO ACTION,
    FOREIGN KEY (SelectedAnswerID) REFERENCES SurveyAnswers(AnswerID) ON DELETE NO ACTION
);

-- Create DashboardData table
CREATE TABLE DashboardData (
    ID INT PRIMARY KEY IDENTITY(1,1),
    Metric NVARCHAR(255) NOT NULL,
    Value INT NOT NULL,
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- Create indexes for better performance
CREATE INDEX IX_Users_Username ON Users(Username);
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_Role ON Users(Role);
CREATE INDEX IX_Courses_CreatedBy ON Courses(CreatedBy);
CREATE INDEX IX_CourseRegistrations_UserID ON CourseRegistrations(UserID);
CREATE INDEX IX_CourseRegistrations_CourseID ON CourseRegistrations(CourseID);
CREATE INDEX IX_Appointments_UserID ON Appointments(UserID);
CREATE INDEX IX_Appointments_ConsultantID ON Appointments(ConsultantID);
CREATE INDEX IX_Appointments_ScheduledAt ON Appointments(ScheduledAt);
CREATE INDEX IX_NewsArticles_CategoryID ON NewsArticles(CategoryID);
CREATE INDEX IX_NewsArticles_CreatedDate ON NewsArticles(CreatedDate);
CREATE INDEX IX_NewsArticles_CreatedByID ON NewsArticles(CreatedByID);
CREATE INDEX IX_NewsArticles_UpdatedByID ON NewsArticles(UpdatedByID);
CREATE INDEX IX_UserSurveyResponses_UserID ON UserSurveyResponses(UserID);
CREATE INDEX IX_UserSurveyResponses_SurveyID ON UserSurveyResponses(SurveyID);

-- Insert some initial data for roles and categories
INSERT INTO Categories (CategoryName, CategoryDescription, IsActive) VALUES 
('Drug Prevention', 'Articles related to drug prevention strategies', 1),
('Health Education', 'Educational content about health and wellness', 1),
('Community Programs', 'Information about community-based prevention programs', 1),
('Research & Studies', 'Latest research and studies in drug prevention', 1);

INSERT INTO Tags (TagName, Note) VALUES 
('Prevention', 'Content related to prevention strategies'),
('Education', 'Educational materials and resources'),
('Youth', 'Content specifically targeted at young people'),
('Community', 'Community-based initiatives and programs'),
('Research', 'Research findings and academic studies');

-- Create default users with properly hashed passwords
-- Generated using PasswordHashUtility - Password: Admin123!
INSERT INTO Users (FullName, Username, Email, PasswordHash, Role, Status, IsEmailVerified) VALUES 
('System Administrator', 'admin', 'admin@drugprevention.com', 'AQAAAAEAACcQAAAAEMockHashHereReplaceWithRealHashFromUtility', 'Admin', 'Active', 1);

-- To generate real hashes, run: dotnet run in PasswordHashGenerator folder
-- Or use PasswordHashUtility.HashPassword("YourPassword") in your code

GO

PRINT 'Database DrugUsePreventionDB created successfully with all tables, indexes, and initial data!'; 