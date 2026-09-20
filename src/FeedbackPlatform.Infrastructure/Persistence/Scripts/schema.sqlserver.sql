IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        Id            UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Email         NVARCHAR(256)    NOT NULL,
        DisplayName   NVARCHAR(200)    NOT NULL,
        PasswordHash  NVARCHAR(500)    NOT NULL,
        Role          INT              NOT NULL,
        IsActive      BIT              NOT NULL DEFAULT (1),
        CreatedAtUtc  DATETIMEOFFSET   NOT NULL
    );
    CREATE UNIQUE INDEX UX_Users_Email ON dbo.Users (Email);
END;

IF OBJECT_ID(N'dbo.FeedbackApps', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.FeedbackApps
    (
        Id                          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        OwnerUserId                 UNIQUEIDENTIFIER NOT NULL,
        Name                        NVARCHAR(200)    NOT NULL,
        Description                 NVARCHAR(1000)   NULL,
        RateLimitPerMinuteOverride  INT              NULL,
        CreatedAtUtc                DATETIMEOFFSET   NOT NULL,
        CONSTRAINT FK_FeedbackApps_Users FOREIGN KEY (OwnerUserId) REFERENCES dbo.Users (Id)
    );
    CREATE INDEX IX_FeedbackApps_OwnerUserId ON dbo.FeedbackApps (OwnerUserId);
END;

IF OBJECT_ID(N'dbo.ApiKeys', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ApiKeys
    (
        Id              UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        FeedbackAppId   UNIQUEIDENTIFIER NOT NULL,
        KeyPrefix       NVARCHAR(32)     NOT NULL,
        KeyHash         NVARCHAR(128)    NOT NULL,
        CreatedAtUtc    DATETIMEOFFSET   NOT NULL,
        RevokedAtUtc    DATETIMEOFFSET   NULL,
        LastUsedAtUtc   DATETIMEOFFSET   NULL,
        CONSTRAINT FK_ApiKeys_FeedbackApps FOREIGN KEY (FeedbackAppId) REFERENCES dbo.FeedbackApps (Id)
    );
    CREATE UNIQUE INDEX UX_ApiKeys_KeyHash ON dbo.ApiKeys (KeyHash);
    CREATE INDEX IX_ApiKeys_FeedbackAppId ON dbo.ApiKeys (FeedbackAppId);
END;

IF OBJECT_ID(N'dbo.FeedbackFieldDefinitions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.FeedbackFieldDefinitions
    (
        Id              UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        FeedbackAppId   UNIQUEIDENTIFIER NOT NULL,
        FieldKey        NVARCHAR(100)    NOT NULL,
        Label           NVARCHAR(200)    NOT NULL,
        FieldType       INT              NOT NULL,
        IsRequired      BIT              NOT NULL DEFAULT (0),
        DisplayOrder    INT              NOT NULL DEFAULT (0),
        OptionsCsv      NVARCHAR(1000)   NULL,
        CONSTRAINT FK_FeedbackFieldDefinitions_FeedbackApps FOREIGN KEY (FeedbackAppId) REFERENCES dbo.FeedbackApps (Id)
    );
    CREATE INDEX IX_FeedbackFieldDefinitions_FeedbackAppId ON dbo.FeedbackFieldDefinitions (FeedbackAppId);
END;

IF OBJECT_ID(N'dbo.Feedbacks', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Feedbacks
    (
        Id                UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        FeedbackAppId     UNIQUEIDENTIFIER NOT NULL,
        AppVersion        NVARCHAR(50)     NOT NULL,
        Title             NVARCHAR(300)    NOT NULL,
        Comment           NVARCHAR(MAX)    NOT NULL,
        StarRating        INT              NOT NULL,
        ReporterName      NVARCHAR(200)    NULL,
        ReporterContact   NVARCHAR(200)    NULL,
        IpAddress         NVARCHAR(64)     NOT NULL,
        CustomFieldsJson  NVARCHAR(MAX)    NULL,
        CreatedAtUtc      DATETIMEOFFSET   NOT NULL,
        CONSTRAINT FK_Feedbacks_FeedbackApps FOREIGN KEY (FeedbackAppId) REFERENCES dbo.FeedbackApps (Id)
    );
    CREATE INDEX IX_Feedbacks_FeedbackAppId_CreatedAtUtc ON dbo.Feedbacks (FeedbackAppId, CreatedAtUtc);
END;

IF OBJECT_ID(N'dbo.AppSettings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AppSettings
    (
        [Key]   NVARCHAR(200)  NOT NULL PRIMARY KEY,
        [Value] NVARCHAR(MAX)  NOT NULL
    );
END;
