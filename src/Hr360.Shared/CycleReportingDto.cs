namespace Hr360.Shared;

public sealed record CycleReportingDto(
    Guid CycleId,
    string CycleName,
    int AssignmentCount,
    int SubmittedCount,
    decimal CompletionRate,
    bool IsSuppressed,
    IReadOnlyList<RevieweeReportingDto> Reviewees);
