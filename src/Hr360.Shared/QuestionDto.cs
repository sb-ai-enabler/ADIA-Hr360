namespace Hr360.Shared;

public sealed record QuestionDto(
    string Id,
    string Prompt,
    QuestionType Type,
    bool Required,
    int? MinRating,
    int? MaxRating,
    string? HelpText);
