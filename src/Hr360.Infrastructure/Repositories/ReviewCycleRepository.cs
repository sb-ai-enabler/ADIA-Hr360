using Hr360.Application;
using Hr360.Application.Interfaces;
using Hr360.Domain;
using Hr360.Shared;
using Microsoft.EntityFrameworkCore;

namespace Hr360.Infrastructure.Repositories;

public sealed class ReviewCycleRepository(Hr360DbContext context)
    : RepositoryBase<ReviewCycle>(context), IReviewCycleRepository
{
    public async Task<IReadOnlyList<ReviewCycle>> GetAllWithAssignmentsAsync(CancellationToken cancellationToken = default) =>
        await Set
            .Include(c => c.ReviewAssignments)
            .OrderByDescending(c => c.StartsOn)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ReportingCycleReadModel>> GetReportingDataAsync(CancellationToken cancellationToken = default) =>
        await BuildReportingDataAsync(cancellationToken);

    private async Task<IReadOnlyList<ReportingCycleReadModel>> BuildReportingDataAsync(CancellationToken cancellationToken)
    {
        var cycles = await Set
            .OrderByDescending(c => c.StartsOn)
            .Select(c => new
            {
                c.Id,
                c.Name,
                Status = (ReviewCycleStatus)c.Status,
                c.TemplateSnapshotJson
            })
            .ToListAsync(cancellationToken);

        var cycleIds = cycles.Select(c => c.Id).ToArray();
        var assignments = await Context.Set<ReviewAssignment>()
            .Where(a => cycleIds.Contains(a.CycleId))
            .Include(a => a.Reviewee)
            .Include(a => a.FeedbackSubmissions)
            .ToListAsync(cancellationToken);

        return cycles.Select(c =>
        {
            var cycleAssignments = assignments.Where(a => a.CycleId == c.Id).ToList();
            var submissions = cycleAssignments
                .Select(a => new
                {
                    Assignment = a,
                    FinalSubmission = a.FeedbackSubmissions
                        .Where(s => s.IsFinal)
                        .OrderByDescending(s => s.SubmittedAt)
                        .ThenByDescending(s => s.UpdatedAt)
                        .FirstOrDefault()
                })
                .Where(a => a.FinalSubmission is not null)
                .Select(a => new ReportingSubmissionReadModel(
                    a.Assignment.RevieweeId,
                    a.Assignment.Reviewee?.DisplayName ?? string.Empty,
                    a.FinalSubmission!.AnswersJson))
                .ToList();

            return new ReportingCycleReadModel(
                c.Id,
                c.Name,
                c.Status,
                cycleAssignments.Count,
                submissions.Count,
                c.TemplateSnapshotJson,
                submissions);
        }).ToList();
    }
}
