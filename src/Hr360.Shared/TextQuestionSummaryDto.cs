namespace Hr360.Shared;

public sealed record TextQuestionSummaryDto(
    string QuestionId,
    string Prompt,
    int ResponseCount,
    IReadOnlyList<string> Comments);
