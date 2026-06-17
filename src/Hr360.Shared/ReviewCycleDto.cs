namespace Hr360.Shared;

public sealed record ReviewCycleDto(
    Guid Id,
    string Name,
    Guid TemplateId,
    int TemplateVersion,
    ReviewCycleStatus Status,
    DateOnly StartsOn,
    DateOnly EndsOn,
    int AssignmentCount,
    int SubmittedCount);
