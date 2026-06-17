using Hr360.Application.Interfaces;
using Hr360.Domain;
using Hr360.Shared;

namespace Hr360.Application;

public sealed class ReviewCycleService(
    IReviewCycleRepository cycles,
    IReviewTemplateRepository templates,
    IEmployeeRepository employees,
    IAuditEventRepository auditEvents,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser) : IReviewCycleService
{
    public async Task<IReadOnlyList<EmployeeDto>> GetEmployeesAsync(CancellationToken cancellationToken = default)
    {
        var items = await employees.GetActiveAsync(cancellationToken);
        return items.Select(e => e.ToDto()).ToList();
    }

    public async Task<IReadOnlyList<ReviewCycleDto>> GetCyclesAsync(CancellationToken cancellationToken = default)
    {
        var items = await cycles.GetAllWithAssignmentsAsync(cancellationToken);
        return items.Select(c => c.ToDto()).ToList();
    }

    public async Task<ReviewCycleDto> CreateCycleAsync(CreateCycleRequest request, CancellationToken cancellationToken = default)
    {
        if (request.EndsOn < request.StartsOn)
        {
            throw new InvalidOperationException("End date must be on or after the start date.");
        }

        var template = await templates.GetActiveByIdAsync(request.TemplateId, cancellationToken)
            ?? throw new InvalidOperationException("Template was not found.");

        var revieweeIds = request.RevieweeIds.Distinct().ToArray();
        var reviewerIds = request.ReviewerIds.Distinct().ToArray();

        if (revieweeIds.Length == 0 || reviewerIds.Length == 0)
        {
            throw new InvalidOperationException("At least one reviewee and reviewer are required.");
        }

        var candidateIds = revieweeIds.Concat(reviewerIds).Distinct().ToArray();
        var activeIds = await employees.GetActiveIdsAsync(candidateIds, cancellationToken);

        if (revieweeIds.Except(activeIds).Any() || reviewerIds.Except(activeIds).Any())
        {
            throw new InvalidOperationException("All reviewees and reviewers must be active employees.");
        }

        var cycle = new ReviewCycle
        {
            Name = request.Name.Trim(),
            TemplateId = template.Id,
            TemplateVersion = template.Version,
            TemplateSnapshotJson = template.DefinitionJson,
            Status = ReviewCycleStatus.Active,
            StartsOn = request.StartsOn,
            EndsOn = request.EndsOn,
            CreatedBy = currentUser.UserId
        };

        foreach (var revieweeId in revieweeIds)
        {
            foreach (var reviewerId in reviewerIds)
            {
                if (revieweeId == reviewerId)
                {
                    continue;
                }

                cycle.Assignments.Add(new ReviewAssignment
                {
                    CycleId = cycle.Id,
                    RevieweeId = revieweeId,
                    ReviewerId = reviewerId
                });
            }
        }

        if (cycle.Assignments.Count == 0)
        {
            throw new InvalidOperationException("The cycle did not create any peer review assignments.");
        }

        cycles.Add(cycle);
        auditEvents.Add(new AuditEvent
        {
            Actor = currentUser.UserId,
            Action = "cycle.created",
            EntityType = nameof(ReviewCycle),
            EntityId = cycle.Id
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return cycle.ToDto();
    }
}
