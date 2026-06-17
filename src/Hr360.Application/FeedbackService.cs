using Hr360.Application.Interfaces;
using Hr360.Domain;
using Hr360.Shared;

namespace Hr360.Application;

public sealed class FeedbackService(
    IReviewAssignmentRepository assignments,
    IFeedbackSubmissionRepository submissions,
    IAuditEventRepository auditEvents,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser) : IFeedbackService
{
    public async Task<IReadOnlyList<ReviewAssignmentDto>> GetAssignmentsForCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var items = await assignments.GetForReviewerAsync(currentUser.UserId, cancellationToken);
        return items.Select(a => a.ToDto()).ToList();
    }

    public async Task<AssignmentFormDto> GetFormAsync(Guid assignmentId, CancellationToken cancellationToken = default)
    {
        var assignment = await GetValidatedAssignmentAsync(assignmentId, allowSubmitted: true, cancellationToken);
        var definition = Mapping.ReadDefinition(assignment.Cycle!.TemplateSnapshotJson);
        return new AssignmentFormDto(assignment.ToDto(), definition);
    }

    public async Task SaveDraftAsync(SaveDraftRequest request, CancellationToken cancellationToken = default)
    {
        var assignment = await GetValidatedAssignmentAsync(request.AssignmentId, allowSubmitted: false, cancellationToken);
        ValidateAnswers(assignment, request.Answers, enforceRequired: false);
        var existing = await submissions.GetLatestDraftAsync(assignment.Id, cancellationToken);

        if (existing is null)
        {
            submissions.Add(new FeedbackSubmission
            {
                AssignmentId = assignment.Id,
                AnswersJson = Mapping.WriteAnswers(request.Answers),
                ClientDraftId = request.ClientDraftId,
                IsFinal = false
            });
        }
        else
        {
            existing.AnswersJson = Mapping.WriteAnswers(request.Answers);
            existing.ClientDraftId = request.ClientDraftId;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
        }

        assignment.Status = AssignmentStatus.Draft;
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task SubmitAsync(SubmitFeedbackRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            throw new InvalidOperationException("Idempotency key is required.");
        }

        var duplicate = await submissions.HasFinalSubmissionAsync(request.IdempotencyKey, cancellationToken);

        if (duplicate)
        {
            return;
        }

        var assignment = await GetValidatedAssignmentAsync(request.AssignmentId, allowSubmitted: false, cancellationToken);
        ValidateAnswers(assignment, request.Answers, enforceRequired: true);

        submissions.Add(new FeedbackSubmission
        {
            AssignmentId = assignment.Id,
            AnswersJson = Mapping.WriteAnswers(request.Answers),
            IdempotencyKey = request.IdempotencyKey,
            IsFinal = true,
            SubmittedAt = DateTimeOffset.UtcNow
        });

        assignment.Status = AssignmentStatus.Submitted;
        assignment.SubmittedAt = DateTimeOffset.UtcNow;

        auditEvents.Add(new AuditEvent
        {
            Actor = currentUser.UserId,
            Action = "feedback.submitted",
            EntityType = nameof(ReviewAssignment),
            EntityId = assignment.Id
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<ReviewAssignment> GetValidatedAssignmentAsync(Guid assignmentId, bool allowSubmitted, CancellationToken cancellationToken)
    {
        var assignment = await assignments.GetWithCycleAndReviewerAsync(assignmentId, cancellationToken)
            ?? throw new InvalidOperationException("Assignment was not found.");

        if (assignment.Reviewer?.EntraObjectId != currentUser.UserId)
        {
            throw new UnauthorizedAccessException("You can only submit your assigned reviews.");
        }

        if (assignment.Cycle?.Status != ReviewCycleStatus.Active)
        {
            throw new InvalidOperationException("This review cycle is not active.");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (today < assignment.Cycle.StartsOn || today > assignment.Cycle.EndsOn)
        {
            throw new InvalidOperationException("This review cycle is outside its submission window.");
        }

        if (!allowSubmitted && assignment.Status == AssignmentStatus.Submitted)
        {
            throw new InvalidOperationException("Feedback has already been submitted.");
        }

        return assignment;
    }

    private static void ValidateAnswers(ReviewAssignment assignment, IReadOnlyList<AnswerDto> answers, bool enforceRequired)
    {
        var definition = Mapping.ReadDefinition(assignment.Cycle!.TemplateSnapshotJson);
        var questions = definition.Sections
            .SelectMany(s => s.Questions)
            .ToDictionary(q => q.Id);

        foreach (var answer in answers)
        {
            if (!questions.ContainsKey(answer.QuestionId))
            {
                throw new InvalidOperationException($"Unknown question '{answer.QuestionId}'.");
            }
        }

        var answersById = answers
            .GroupBy(a => a.QuestionId)
            .ToDictionary(g => g.Key, g => g.Last());

        foreach (var question in questions.Values)
        {
            answersById.TryGetValue(question.Id, out var answer);

            var hasValue = answer is not null && question.Type switch
            {
                QuestionType.Rating => answer.Rating.HasValue,
                QuestionType.YesNo => answer.YesNo.HasValue,
                QuestionType.FreeText => !string.IsNullOrWhiteSpace(answer.Text),
                _ => false
            };

            if (enforceRequired && question.Required && !hasValue)
            {
                throw new InvalidOperationException($"'{question.Prompt}' is required.");
            }

            if (question.Type == QuestionType.Rating && answer?.Rating is int rating)
            {
                var min = question.MinRating ?? 1;
                var max = question.MaxRating ?? 5;
                if (rating < min || rating > max)
                {
                    throw new InvalidOperationException($"'{question.Prompt}' must be between {min} and {max}.");
                }
            }
        }
    }
}
