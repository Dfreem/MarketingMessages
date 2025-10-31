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
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250921162804_Initial'
)
BEGIN
    CREATE TABLE [ApplicationLogs] (
        [Id] int NOT NULL IDENTITY,
        [Message] nvarchar(max) NULL,
        [MessageTemplate] nvarchar(max) NULL,
        [Level] nvarchar(max) NULL,
        [TimeStamp] datetime2 NULL,
        [Exception] nvarchar(max) NULL,
        [Properties] nvarchar(max) NULL,
        CONSTRAINT [PK_ApplicationLogs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250921162804_Initial'
)
BEGIN
    CREATE TABLE [Contents] (
        [ContentId] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [TextContent] nvarchar(max) NOT NULL,
        [HtmlContent] nvarchar(max) NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [ModifiedOn] datetime2 NULL,
        [TemplatePropertyNames] nvarchar(max) NOT NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_Contents] PRIMARY KEY ([ContentId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250921162804_Initial'
)
BEGIN
    CREATE TABLE [EmailEvents] (
        [Id] int NOT NULL IDENTITY,
        [Event] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NOT NULL,
        [Category] nvarchar(max) NULL,
        [SmtpId] nvarchar(max) NULL,
        [SgEventId] nvarchar(max) NULL,
        [SgMessageId] nvarchar(max) NULL,
        [Timestamp] bigint NOT NULL,
        [UserAgent] nvarchar(max) NULL,
        [Ip] nvarchar(max) NULL,
        [Url] nvarchar(max) NULL,
        [Reason] nvarchar(max) NULL,
        [Status] nvarchar(max) NULL,
        [Response] nvarchar(max) NULL,
        [ContactId] int NOT NULL,
        [CampaignId] int NOT NULL,
        [ReceivedOn] datetime2 NOT NULL,
        CONSTRAINT [PK_EmailEvents] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250921162804_Initial'
)
BEGIN
    CREATE TABLE [EngagementUrls] (
        [Id] int NOT NULL IDENTITY,
        [ContactId] int NOT NULL,
        [UserId] nvarchar(max) NOT NULL,
        [CampaignId] int NOT NULL,
        [Url] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_EngagementUrls] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250921162804_Initial'
)
BEGIN
    CREATE TABLE [Images] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [ImageData] varbinary(max) NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_Images] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250921162804_Initial'
)
BEGIN
    CREATE TABLE [JobLogs] (
        [JobLogId] int NOT NULL IDENTITY,
        [JobId] int NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [FinishDate] datetime2 NULL,
        [Succeeded] bit NULL,
        [Notes] nvarchar(max) NULL,
        CONSTRAINT [PK_JobLogs] PRIMARY KEY ([JobLogId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250921162804_Initial'
)
BEGIN
    CREATE TABLE [Senders] (
        [SenderId] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NOT NULL,
        [ReplyTo] nvarchar(max) NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Senders] PRIMARY KEY ([SenderId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250921162804_Initial'
)
BEGIN
    CREATE TABLE [SentLogs] (
        [SentLogId] int NOT NULL IDENTITY,
        [JobId] int NOT NULL,
        [EmailContentId] int NOT NULL,
        [SendListId] int NOT NULL,
        [Subject] nvarchar(max) NULL,
        [Body] nvarchar(max) NULL,
        [Success] bit NOT NULL,
        [Result] nvarchar(max) NULL,
        [DateSent] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [ModifiedOn] datetime2 NULL,
        CONSTRAINT [PK_SentLogs] PRIMARY KEY ([SentLogId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250921162804_Initial'
)
BEGIN
    CREATE TABLE [Settings] (
        [Id] int NOT NULL IDENTITY,
        [SettingName] nvarchar(max) NOT NULL,
        [SettingValue] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Settings] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250921162804_Initial'
)
BEGIN
    CREATE TABLE [SuppressionGroups] (
        [SuppressionGroupId] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_SuppressionGroups] PRIMARY KEY ([SuppressionGroupId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250921162804_Initial'
)
BEGIN
    CREATE TABLE [ContentImages] (
        [Id] int NOT NULL IDENTITY,
        [ImageId] int NOT NULL,
        [EmailContentContentId] int NOT NULL,
        CONSTRAINT [PK_ContentImages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ContentImages_Contents_EmailContentContentId] FOREIGN KEY ([EmailContentContentId]) REFERENCES [Contents] ([ContentId]) ON DELETE CASCADE,
        CONSTRAINT [FK_ContentImages_Images_ImageId] FOREIGN KEY ([ImageId]) REFERENCES [Images] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250921162804_Initial'
)
BEGIN
    CREATE TABLE [Audiences] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(450) NOT NULL,
        [CreatedBy] nvarchar(450) NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [Query] nvarchar(max) NOT NULL,
        [SuppressionGroupId] int NULL,
        CONSTRAINT [PK_Audiences] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Audiences_SuppressionGroups_SuppressionGroupId] FOREIGN KEY ([SuppressionGroupId]) REFERENCES [SuppressionGroups] ([SuppressionGroupId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250921162804_Initial'
)
BEGIN
    CREATE TABLE [Contacts] (
        [ContactId] int NOT NULL IDENTITY,
        [FirstName] nvarchar(max) NOT NULL,
        [LastName] nvarchar(max) NOT NULL,
        [Title] nvarchar(max) NULL,
        [Address] nvarchar(max) NULL,
        [City] nvarchar(max) NULL,
        [State] nvarchar(max) NULL,
        [Zip] nvarchar(max) NULL,
        [Country] nvarchar(max) NULL,
        [MobilePhone] nvarchar(max) NULL,
        [ContactEmail] nvarchar(max) NOT NULL,
        [Profession] nvarchar(max) NULL,
        [Profession2] nvarchar(max) NULL,
        [Custom1] nvarchar(max) NULL,
        [Custom2] nvarchar(max) NULL,
        [CreatedBy] nvarchar(max) NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [ModifiedOn] datetime2 NULL,
        [SuppressionGroupId] int NULL,
        CONSTRAINT [PK_Contacts] PRIMARY KEY ([ContactId]),
        CONSTRAINT [FK_Contacts_SuppressionGroups_SuppressionGroupId] FOREIGN KEY ([SuppressionGroupId]) REFERENCES [SuppressionGroups] ([SuppressionGroupId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250921162804_Initial'
)
BEGIN
    CREATE TABLE [Campaigns] (
        [CampaignId] int NOT NULL IDENTITY,
        [SenderId] int NOT NULL,
        [StartDate] datetime2 NULL,
        [IsEnabled] bit NOT NULL,
        [IsStarted] bit NOT NULL,
        [IsExecuting] bit NOT NULL,
        [IsComplete] bit NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [ModifiedOn] datetime2 NULL,
        [EmailContentId] int NOT NULL,
        [AudienceId] int NOT NULL,
        [Subject] nvarchar(max) NOT NULL,
        [Deleted] bit NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Campaigns] PRIMARY KEY ([CampaignId]),
        CONSTRAINT [FK_Campaigns_Audiences_AudienceId] FOREIGN KEY ([AudienceId]) REFERENCES [Audiences] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Campaigns_Contents_EmailContentId] FOREIGN KEY ([EmailContentId]) REFERENCES [Contents] ([ContentId]) ON DELETE CASCADE,
        CONSTRAINT [FK_Campaigns_Senders_SenderId] FOREIGN KEY ([SenderId]) REFERENCES [Senders] ([SenderId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250921162804_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Audiences_Name_CreatedBy] ON [Audiences] ([Name], [CreatedBy]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250921162804_Initial'
)
BEGIN
    CREATE INDEX [IX_Audiences_SuppressionGroupId] ON [Audiences] ([SuppressionGroupId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250921162804_Initial'
)
BEGIN
    CREATE INDEX [IX_Campaigns_AudienceId] ON [Campaigns] ([AudienceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250921162804_Initial'
)
BEGIN
    CREATE INDEX [IX_Campaigns_EmailContentId] ON [Campaigns] ([EmailContentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250921162804_Initial'
)
BEGIN
    CREATE INDEX [IX_Campaigns_SenderId] ON [Campaigns] ([SenderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250921162804_Initial'
)
BEGIN
    CREATE INDEX [IX_Contacts_SuppressionGroupId] ON [Contacts] ([SuppressionGroupId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250921162804_Initial'
)
BEGIN
    CREATE INDEX [IX_ContentImages_EmailContentContentId] ON [ContentImages] ([EmailContentContentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250921162804_Initial'
)
BEGIN
    CREATE INDEX [IX_ContentImages_ImageId] ON [ContentImages] ([ImageId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250921162804_Initial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250921162804_Initial', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250923023051_EditableAudience'
)
BEGIN
    ALTER TABLE [Audiences] ADD [JSONForm] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250923023051_EditableAudience'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250923023051_EditableAudience', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250926001349_Deleted'
)
BEGIN
    ALTER TABLE [Audiences] ADD [Deleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250926001349_Deleted'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250926001349_Deleted', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250926003425_FullFeatureCampaign'
)
BEGIN
    ALTER TABLE [Campaigns] ADD [EndDate] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250926003425_FullFeatureCampaign'
)
BEGIN
    ALTER TABLE [Campaigns] ADD [FrequencyInterval] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250926003425_FullFeatureCampaign'
)
BEGIN
    ALTER TABLE [Campaigns] ADD [FrequencyUnit] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250926003425_FullFeatureCampaign'
)
BEGIN
    ALTER TABLE [Campaigns] ADD [IsRecurring] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250926003425_FullFeatureCampaign'
)
BEGIN
    ALTER TABLE [Campaigns] ADD [JobType] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250926003425_FullFeatureCampaign'
)
BEGIN
    ALTER TABLE [Campaigns] ADD [JobsCompleted] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250926003425_FullFeatureCampaign'
)
BEGIN
    ALTER TABLE [Campaigns] ADD [NextExecution] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250926003425_FullFeatureCampaign'
)
BEGIN
    ALTER TABLE [Campaigns] ADD [TotalJobLimit] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250926003425_FullFeatureCampaign'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250926003425_FullFeatureCampaign', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017024012_RenameJobType_CampaignType'
)
BEGIN
    EXEC sp_rename N'[Campaigns].[JobType]', N'CampaignType', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017024012_RenameJobType_CampaignType'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251017024012_RenameJobType_CampaignType', N'9.0.9');
END;

COMMIT;
GO

