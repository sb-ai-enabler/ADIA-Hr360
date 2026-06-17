namespace Hr360.Shared;

public sealed record RatingQuestionSummaryDto(
    string QuestionId,
    string Prompt,
    int ResponseCount,
    decimal Average,
    int? Minimum,
    int? Maximum);
