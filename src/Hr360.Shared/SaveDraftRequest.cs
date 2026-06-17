namespace Hr360.Shared;

public sealed record SaveDraftRequest(
    Guid AssignmentId,
    IReadOnlyList<AnswerDto> Answers,
    string ClientDraftId);
