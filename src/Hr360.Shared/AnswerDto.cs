namespace Hr360.Shared;

public sealed record AnswerDto(
    string QuestionId,
    int? Rating,
    string? Text,
    bool? YesNo);
