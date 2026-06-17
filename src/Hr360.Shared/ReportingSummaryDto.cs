namespace Hr360.Shared;

public sealed record ReportingSummaryDto(
    int ActiveCycles,
    int TotalAssignments,
    int SubmittedAssignments,
    decimal CompletionRate,
    IReadOnlyList<CycleReportingDto> Cycles);
