namespace Hr360.Shared;

public sealed record SubmitFeedbackRequest(
    Guid AssignmentId,
    IReadOnlyList<AnswerDto> Answers,
    string IdempotencyKey);
