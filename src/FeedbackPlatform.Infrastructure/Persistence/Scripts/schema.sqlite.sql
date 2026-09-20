CREATE TABLE IF NOT EXISTS Users
(
    Id            TEXT    NOT NULL PRIMARY KEY,
    Email         TEXT    NOT NULL,
    DisplayName   TEXT    NOT NULL,
    PasswordHash  TEXT    NOT NULL,
    Role          INTEGER NOT NULL,
    IsActive      INTEGER NOT NULL DEFAULT 1,
    CreatedAtUtc  TEXT    NOT NULL
);
CREATE UNIQUE INDEX IF NOT EXISTS UX_Users_Email ON Users (Email);

CREATE TABLE IF NOT EXISTS FeedbackApps
(
    Id                          TEXT    NOT NULL PRIMARY KEY,
    OwnerUserId                 TEXT    NOT NULL,
    Name                        TEXT    NOT NULL,
    Description                 TEXT    NULL,
    RateLimitPerMinuteOverride  INTEGER NULL,
    CreatedAtUtc                TEXT    NOT NULL,
    FOREIGN KEY (OwnerUserId) REFERENCES Users (Id)
);
CREATE INDEX IF NOT EXISTS IX_FeedbackApps_OwnerUserId ON FeedbackApps (OwnerUserId);

CREATE TABLE IF NOT EXISTS ApiKeys
(
    Id              TEXT    NOT NULL PRIMARY KEY,
    FeedbackAppId   TEXT    NOT NULL,
    KeyPrefix       TEXT    NOT NULL,
    KeyHash         TEXT    NOT NULL,
    CreatedAtUtc    TEXT    NOT NULL,
    RevokedAtUtc    TEXT    NULL,
    LastUsedAtUtc   TEXT    NULL,
    FOREIGN KEY (FeedbackAppId) REFERENCES FeedbackApps (Id)
);
CREATE UNIQUE INDEX IF NOT EXISTS UX_ApiKeys_KeyHash ON ApiKeys (KeyHash);
CREATE INDEX IF NOT EXISTS IX_ApiKeys_FeedbackAppId ON ApiKeys (FeedbackAppId);

CREATE TABLE IF NOT EXISTS FeedbackFieldDefinitions
(
    Id              TEXT    NOT NULL PRIMARY KEY,
    FeedbackAppId   TEXT    NOT NULL,
    FieldKey        TEXT    NOT NULL,
    Label           TEXT    NOT NULL,
    FieldType       INTEGER NOT NULL,
    IsRequired      INTEGER NOT NULL DEFAULT 0,
    DisplayOrder    INTEGER NOT NULL DEFAULT 0,
    OptionsCsv      TEXT    NULL,
    FOREIGN KEY (FeedbackAppId) REFERENCES FeedbackApps (Id)
);
CREATE INDEX IF NOT EXISTS IX_FeedbackFieldDefinitions_FeedbackAppId ON FeedbackFieldDefinitions (FeedbackAppId);

CREATE TABLE IF NOT EXISTS Feedbacks
(
    Id                TEXT    NOT NULL PRIMARY KEY,
    FeedbackAppId     TEXT    NOT NULL,
    AppVersion        TEXT    NOT NULL,
    Title             TEXT    NOT NULL,
    Comment           TEXT    NOT NULL,
    StarRating        INTEGER NOT NULL,
    ReporterName      TEXT    NULL,
    ReporterContact   TEXT    NULL,
    IpAddress         TEXT    NOT NULL,
    CustomFieldsJson  TEXT    NULL,
    CreatedAtUtc      TEXT    NOT NULL,
    FOREIGN KEY (FeedbackAppId) REFERENCES FeedbackApps (Id)
);
CREATE INDEX IF NOT EXISTS IX_Feedbacks_FeedbackAppId_CreatedAtUtc ON Feedbacks (FeedbackAppId, CreatedAtUtc);

CREATE TABLE IF NOT EXISTS AppSettings
(
    Key   TEXT NOT NULL PRIMARY KEY,
    Value TEXT NOT NULL
);
