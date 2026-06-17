namespace Hr360.Shared;

public sealed record CreateCycleRequest(
    string Name,
    Guid TemplateId,
    DateOnly StartsOn,
    DateOnly EndsOn,
    IReadOnlyList<Guid> RevieweeIds,
    IReadOnlyList<Guid> ReviewerIds);
