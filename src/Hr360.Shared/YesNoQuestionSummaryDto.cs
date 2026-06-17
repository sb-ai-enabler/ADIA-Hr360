namespace Hr360.Shared;

public sealed record YesNoQuestionSummaryDto(
    string QuestionId,
    string Prompt,
    int ResponseCount,
    int YesCount,
    int NoCount,
    decimal YesPercentage);
