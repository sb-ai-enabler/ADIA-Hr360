using Hr360.Shared;

namespace Hr360.Application.Interfaces;

public interface IFeedbackService
{
    Task<IReadOnlyList<ReviewAssignmentDto>> GetAssignmentsForCurrentUserAsync(CancellationToken cancellationToken = default);
    Task<AssignmentFormDto> GetFormAsync(Guid assignmentId, CancellationToken cancellationToken = default);
    Task SaveDraftAsync(SaveDraftRequest request, CancellationToken cancellationToken = default);
    Task SubmitAsync(SubmitFeedbackRequest request, CancellationToken cancellationToken = default);
}
