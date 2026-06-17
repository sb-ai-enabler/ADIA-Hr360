CREATE TABLE [dbo].[FeedbackSubmissions] (
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_FeedbackSubmissions] PRIMARY KEY,
    [AssignmentId] uniqueidentifier NOT NULL,
    [AnswersJson] nvarchar(max) NOT NULL,
    [IsFinal] bit NOT NULL,
    [IdempotencyKey] nvarchar(120) NOT NULL,
    [ClientDraftId] nvarchar(120) NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [SubmittedAt] datetimeoffset NULL,
    CONSTRAINT [FK_FeedbackSubmissions_ReviewAssignments_AssignmentId] FOREIGN KEY ([AssignmentId]) REFERENCES [dbo].[ReviewAssignments] ([Id])
);
GO

CREATE UNIQUE INDEX [UX_FeedbackSubmissions_IdempotencyKey]
    ON [dbo].[FeedbackSubmissions] ([IdempotencyKey])
    WHERE [IdempotencyKey] <> '';
GO
