using Hr360.Application.Interfaces;
using Hr360.Shared;

namespace Hr360.Application;

public sealed class ReportingService(IReviewCycleRepository cycleRepository) : IReportingService
{
    private const int MinimumGroupSize = 3;

    public async Task<ReportingSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var cycles = await cycleRepository.GetAllWithAssignmentsAsync(cancellationToken);

        var cycleReports = cycles.Select(c =>
        {
            var assignmentCount = c.Assignments.Count;
            var submittedCount = c.Assignments.Count(a => a.Status == AssignmentStatus.Submitted);
            var completionRate = assignmentCount == 0 ? 0 : decimal.Round((decimal)submittedCount / assignmentCount * 100, 1);

            return new CycleReportingDto(
                c.Id,
                c.Name,
                assignmentCount,
                submittedCount,
                completionRate,
                submittedCount > 0 && submittedCount < MinimumGroupSize);
        }).ToList();

        var totalAssignments = cycleReports.Sum(c => c.AssignmentCount);
        var submittedAssignments = cycleReports.Sum(c => c.SubmittedCount);
        var totalCompletion = totalAssignments == 0 ? 0 : decimal.Round((decimal)submittedAssignments / totalAssignments * 100, 1);

        return new ReportingSummaryDto(
            cycles.Count(c => c.Status == ReviewCycleStatus.Active),
            totalAssignments,
            submittedAssignments,
            totalCompletion,
            cycleReports);
    }
}
