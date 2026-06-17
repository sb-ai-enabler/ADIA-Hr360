CREATE TABLE [dbo].[ReviewTemplates] (
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_ReviewTemplates] PRIMARY KEY,
    [Name] nvarchar(160) NOT NULL,
    [Description] nvarchar(1000) NOT NULL,
    [Version] int NOT NULL,
    [DefinitionJson] nvarchar(max) NOT NULL,
    [IsArchived] bit NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [CreatedBy] nvarchar(200) NOT NULL
);
GO

CREATE UNIQUE INDEX [UX_ReviewTemplates_Name_Version] ON [dbo].[ReviewTemplates] ([Name], [Version]);
GO
