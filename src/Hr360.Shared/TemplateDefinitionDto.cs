namespace Hr360.Shared;

public sealed record TemplateDefinitionDto(
    IReadOnlyList<TemplateSectionDto> Sections);
