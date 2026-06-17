namespace Hr360.Shared;

public sealed record TemplateSectionDto(
    string Id,
    string Title,
    IReadOnlyList<QuestionDto> Questions);
