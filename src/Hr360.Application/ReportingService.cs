using Hr360.Application.Interfaces;
using Hr360.Shared;

namespace Hr360.Application;

public sealed class ReportingService(IReviewCycleRepository cycleRepository) : IReportingService
{
    private const int MinimumGroupSize = 3;

    public async Task<ReportingSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var cycles = await cycleRepository.GetReportingDataAsync(cancellationToken);

        var cycleReports = cycles.Select(c =>
        {
            var assignmentCount = c.AssignmentCount;
            var submittedCount = c.SubmittedCount;
            var completionRate = assignmentCount == 0 ? 0 : decimal.Round((decimal)submittedCount / assignmentCount * 100, 1);

            return new CycleReportingDto(
                c.Id,
                c.Name,
                assignmentCount,
                submittedCount,
                completionRate,
                submittedCount > 0 && submittedCount < MinimumGroupSize,
                c.Submissions
                    .GroupBy(a => new
                    {
                        a.RevieweeId,
                        a.RevieweeName
                    })
                    .Select(g => BuildRevieweeReport(c.TemplateSnapshotJson, g.Key.RevieweeId, g.Key.RevieweeName, g.ToList()))
                    .OrderBy(r => r.RevieweeName)
                    .ToList());
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

    private static RevieweeReportingDto BuildRevieweeReport(
        string templateSnapshotJson,
        Guid revieweeId,
        string revieweeName,
        IReadOnlyList<ReportingSubmissionReadModel> submissions)
    {
        if (submissions.Count < MinimumGroupSize)
        {
            return new RevieweeReportingDto(revieweeId, revieweeName, submissions.Count, true, [], [], []);
        }

        var questions = Mapping.ReadDefinition(templateSnapshotJson)
            .Sections
            .SelectMany(s => s.Questions)
            .ToList();

        var answerSets = submissions
            .Select(s => Mapping.ReadAnswers(s.AnswersJson)
                .GroupBy(a => a.QuestionId)
                .ToDictionary(g => g.Key, g => g.Last()))
            .ToList();

        var ratingSummaries = questions
            .Where(q => q.Type == QuestionType.Rating)
            .Select(q =>
            {
                var values = answerSets
                    .Select(a => a.TryGetValue(q.Id, out var answer) ? answer.Rating : null)
                    .Where(v => v.HasValue)
                    .Select(v => v!.Value)
                    .ToList();

                return new RatingQuestionSummaryDto(
                    q.Id,
                    q.Prompt,
                    values.Count,
                    values.Count == 0 ? 0 : decimal.Round(values.Average(v => (decimal)v), 1),
                    values.Count == 0 ? null : values.Min(),
                    values.Count == 0 ? null : values.Max());
            })
            .ToList();

        var yesNoSummaries = questions
            .Where(q => q.Type == QuestionType.YesNo)
            .Select(q =>
            {
                var values = answerSets
                    .Select(a => a.TryGetValue(q.Id, out var answer) ? answer.YesNo : null)
                    .Where(v => v.HasValue)
                    .Select(v => v!.Value)
                    .ToList();
                var yesCount = values.Count(v => v);
                var noCount = values.Count - yesCount;

                return new YesNoQuestionSummaryDto(
                    q.Id,
                    q.Prompt,
                    values.Count,
                    yesCount,
                    noCount,
                    values.Count == 0 ? 0 : decimal.Round((decimal)yesCount / values.Count * 100, 1));
            })
            .ToList();

        var textSummaries = questions
            .Where(q => q.Type == QuestionType.FreeText)
            .Select(q =>
            {
                var comments = answerSets
                    .Select(a => a.TryGetValue(q.Id, out var answer) ? answer.Text : null)
                    .Where(text => !string.IsNullOrWhiteSpace(text))
                    .Select(text => text!.Trim())
                    .ToList();

                return new TextQuestionSummaryDto(q.Id, q.Prompt, comments.Count, comments);
            })
            .ToList();

        return new RevieweeReportingDto(
            revieweeId,
            revieweeName,
            submissions.Count,
            false,
            ratingSummaries,
            yesNoSummaries,
            textSummaries);
    }
}
