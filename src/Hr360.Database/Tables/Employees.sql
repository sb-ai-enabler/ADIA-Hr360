CREATE TABLE [dbo].[Employees] (
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_Employees] PRIMARY KEY,
    [EntraObjectId] nvarchar(200) NOT NULL,
    [DisplayName] nvarchar(200) NOT NULL,
    [Email] nvarchar(320) NOT NULL,
    [Department] nvarchar(120) NULL,
    [IsActive] bit NOT NULL CONSTRAINT [DF_Employees_IsActive] DEFAULT (1)
);
GO

CREATE UNIQUE INDEX [UX_Employees_EntraObjectId] ON [dbo].[Employees] ([EntraObjectId]);
GO

CREATE UNIQUE INDEX [UX_Employees_Email] ON [dbo].[Employees] ([Email]);
GO
