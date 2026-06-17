CREATE TABLE [dbo].[AuditEvents] (
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_AuditEvents] PRIMARY KEY,
    [Actor] nvarchar(200) NOT NULL,
    [Action] nvarchar(120) NOT NULL,
    [EntityType] nvarchar(120) NOT NULL,
    [EntityId] uniqueidentifier NOT NULL,
    [MetadataJson] nvarchar(max) NOT NULL,
    [OccurredAt] datetimeoffset NOT NULL
);
GO
