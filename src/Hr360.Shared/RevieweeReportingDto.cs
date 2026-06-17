namespace Hr360.Shared;

public sealed record RevieweeReportingDto(
    Guid RevieweeId,
    string RevieweeName,
    int SubmittedCount,
    bool IsSuppressed,
    IReadOnlyList<RatingQuestionSummaryDto> RatingQuestions,
    IReadOnlyList<YesNoQuestionSummaryDto> YesNoQuestions,
    IReadOnlyList<TextQuestionSummaryDto> TextQuestions);
