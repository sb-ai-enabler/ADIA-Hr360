namespace Hr360.Shared;

public sealed record CreateTemplateRequest(
    string Name,
    string Description,
    TemplateDefinitionDto Definition);
