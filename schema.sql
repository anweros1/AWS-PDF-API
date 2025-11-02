IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [ApiCallLogs] (
    [Id] int NOT NULL IDENTITY,
    [PdfGuid] nvarchar(36) NOT NULL,
    [ApplicationName] nvarchar(200) NOT NULL,
    [Endpoint] nvarchar(500) NOT NULL,
    [HttpMethod] nvarchar(10) NOT NULL,
    [RequestBody] nvarchar(max) NULL,
    [ResponseStatusCode] int NULL,
    [ResponseBody] nvarchar(max) NULL,
    [ErrorMessage] nvarchar(max) NULL,
    [DurationMs] bigint NOT NULL,
    [Timestamp] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [ClientIpAddress] nvarchar(45) NULL,
    [IsSuccess] bit NOT NULL,
    CONSTRAINT [PK_ApiCallLogs] PRIMARY KEY ([Id])
);

CREATE TABLE [PdfHistories] (
    [Id] int NOT NULL IDENTITY,
    [PdfGuid] nvarchar(36) NOT NULL,
    [ApplicationName] nvarchar(200) NOT NULL,
    [OriginalUrl] nvarchar(2000) NOT NULL,
    [S3Url] nvarchar(2000) NULL,
    [S3BucketName] nvarchar(200) NULL,
    [S3ObjectKey] nvarchar(500) NULL,
    [LocalFilePath] nvarchar(1000) NULL,
    [IsStoredInS3] bit NOT NULL,
    [FileSizeBytes] bigint NOT NULL,
    [PageCount] int NULL,
    [Keywords] nvarchar(max) NULL,
    [Bookmarks] nvarchar(max) NULL,
    [Variables] nvarchar(max) NULL,
    [IsMerged] bit NOT NULL,
    [SourcePdfGuids] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [UpdatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    CONSTRAINT [PK_PdfHistories] PRIMARY KEY ([Id])
);

CREATE INDEX [IX_ApiCallLogs_ApplicationName] ON [ApiCallLogs] ([ApplicationName]);

CREATE INDEX [IX_ApiCallLogs_IsSuccess] ON [ApiCallLogs] ([IsSuccess]);

CREATE INDEX [IX_ApiCallLogs_PdfGuid] ON [ApiCallLogs] ([PdfGuid]);

CREATE INDEX [IX_ApiCallLogs_PdfGuid_Timestamp] ON [ApiCallLogs] ([PdfGuid], [Timestamp]);

CREATE INDEX [IX_ApiCallLogs_Timestamp] ON [ApiCallLogs] ([Timestamp]);

CREATE INDEX [IX_PdfHistories_ApplicationName] ON [PdfHistories] ([ApplicationName]);

CREATE INDEX [IX_PdfHistories_CreatedAt] ON [PdfHistories] ([CreatedAt]);

CREATE INDEX [IX_PdfHistories_IsDeleted] ON [PdfHistories] ([IsDeleted]);

CREATE INDEX [IX_PdfHistories_IsDeleted_PdfGuid] ON [PdfHistories] ([IsDeleted], [PdfGuid]);

CREATE INDEX [IX_PdfHistories_IsMerged] ON [PdfHistories] ([IsMerged]);

CREATE INDEX [IX_PdfHistories_IsStoredInS3] ON [PdfHistories] ([IsStoredInS3]);

CREATE UNIQUE INDEX [IX_PdfHistories_PdfGuid] ON [PdfHistories] ([PdfGuid]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251029154704_InitialCreate', N'9.0.10');

COMMIT;
GO

