namespace Hr360.Shared;

public sealed record ReviewTemplateDto(
    Guid Id,
    string Name,
    string Description,
    int Version,
    TemplateDefinitionDto Definition,
    DateTimeOffset CreatedAt,
    bool IsArchived);
