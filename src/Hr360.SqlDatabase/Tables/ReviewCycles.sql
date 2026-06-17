CREATE TABLE [dbo].[ReviewCycles] (
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_ReviewCycles] PRIMARY KEY,
    [Name] nvarchar(160) NOT NULL,
    [TemplateId] uniqueidentifier NOT NULL,
    [TemplateVersion] int NOT NULL,
    [TemplateSnapshotJson] nvarchar(max) NOT NULL,
    [Status] int NOT NULL,
    [StartsOn] date NOT NULL,
    [EndsOn] date NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [CreatedBy] nvarchar(200) NOT NULL,
    CONSTRAINT [FK_ReviewCycles_ReviewTemplates_TemplateId] FOREIGN KEY ([TemplateId]) REFERENCES [dbo].[ReviewTemplates] ([Id])
);
GO
