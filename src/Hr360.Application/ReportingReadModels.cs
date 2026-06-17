using Hr360.Shared;

namespace Hr360.Application;

public sealed record ReportingCycleReadModel(
    Guid Id,
    string Name,
    ReviewCycleStatus Status,
    int AssignmentCount,
    int SubmittedCount,
    string TemplateSnapshotJson,
    IReadOnlyList<ReportingSubmissionReadModel> Submissions);

public sealed record ReportingSubmissionReadModel(
    Guid RevieweeId,
    string RevieweeName,
    string AnswersJson);
