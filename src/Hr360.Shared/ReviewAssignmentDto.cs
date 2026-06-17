namespace Hr360.Shared;

public sealed record ReviewAssignmentDto(
    Guid Id,
    Guid CycleId,
    string CycleName,
    Guid RevieweeId,
    string RevieweeName,
    Guid ReviewerId,
    string ReviewerName,
    AssignmentStatus Status,
    DateTimeOffset? SubmittedAt);
