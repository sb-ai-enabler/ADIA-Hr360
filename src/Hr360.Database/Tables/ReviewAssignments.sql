CREATE TABLE [dbo].[ReviewAssignments] (
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_ReviewAssignments] PRIMARY KEY,
    [CycleId] uniqueidentifier NOT NULL,
    [RevieweeId] uniqueidentifier NOT NULL,
    [ReviewerId] uniqueidentifier NOT NULL,
    [Status] int NOT NULL,
    [SubmittedAt] datetimeoffset NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [FK_ReviewAssignments_ReviewCycles_CycleId] FOREIGN KEY ([CycleId]) REFERENCES [dbo].[ReviewCycles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ReviewAssignments_Employees_RevieweeId] FOREIGN KEY ([RevieweeId]) REFERENCES [dbo].[Employees] ([Id]),
    CONSTRAINT [FK_ReviewAssignments_Employees_ReviewerId] FOREIGN KEY ([ReviewerId]) REFERENCES [dbo].[Employees] ([Id])
);
GO

CREATE UNIQUE INDEX [UX_ReviewAssignments_Cycle_Reviewee_Reviewer]
    ON [dbo].[ReviewAssignments] ([CycleId], [RevieweeId], [ReviewerId]);
GO
