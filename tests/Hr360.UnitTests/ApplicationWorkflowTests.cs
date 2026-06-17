using Hr360.Application;
using Hr360.Application.Interfaces;
using Hr360.Domain;
using Hr360.Infrastructure;
using Hr360.Infrastructure.Repositories;
using Hr360.Shared;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Hr360.UnitTests;

public sealed class ApplicationWorkflowTests
{
    [Fact]
    public async Task CreateCycle_Snapshots_Template_Definition()
    {
        await using var db = CreateDb();
        var admin = new TestUser("demo.admin", Hr360Roles.HrAdmin);
        var templateService = CreateTemplateService(db, admin);
        var cycleService = CreateCycleService(db, admin);

        var alex = new Employee { EntraObjectId = "demo.employee", DisplayName = "Alex", Email = "alex@example.com" };
        var sam = new Employee { EntraObjectId = "demo.peer", DisplayName = "Sam", Email = "sam@example.com" };
        db.Employees.AddRange(alex, sam);
        await db.SaveChangesAsync();

        var template = await templateService.CreateTemplateAsync(new CreateTemplateRequest(
            "Core 360",
            "Initial",
            new TemplateDefinitionDto([
                new TemplateSectionDto("impact", "Impact", [
                    new QuestionDto("q1", "Original question", QuestionType.FreeText, true, null, null, null)
                ])
            ])));

        var cycle = await cycleService.CreateCycleAsync(new CreateCycleRequest(
            "Cycle 1",
            template.Id,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            [alex.Id],
            [sam.Id]));

        var persistedCycle = await db.Cycles.SingleAsync(c => c.Id == cycle.Id);
        Assert.Contains("Original question", persistedCycle.TemplateSnapshotJson);

        var persistedTemplate = await db.Templates.SingleAsync(t => t.Id == template.Id);
        persistedTemplate.DefinitionJson = persistedTemplate.DefinitionJson.Replace("Original question", "Changed later");
        await db.SaveChangesAsync();

        Assert.Contains("Original question", persistedCycle.TemplateSnapshotJson);
        Assert.DoesNotContain("Changed later", persistedCycle.TemplateSnapshotJson);
    }

    [Fact]
    public async Task Submit_Rejects_User_Who_Is_Not_Assigned_Reviewer()
    {
        await using var db = CreateDb();
        var admin = new TestUser("demo.admin", Hr360Roles.HrAdmin);
        var templateService = CreateTemplateService(db, admin);
        var cycleService = CreateCycleService(db, admin);

        var alex = new Employee { EntraObjectId = "demo.employee", DisplayName = "Alex", Email = "alex@example.com" };
        var sam = new Employee { EntraObjectId = "demo.peer", DisplayName = "Sam", Email = "sam@example.com" };
        db.Employees.AddRange(alex, sam);
        await db.SaveChangesAsync();

        var template = await templateService.CreateTemplateAsync(new CreateTemplateRequest(
            "Core 360",
            "Initial",
            new TemplateDefinitionDto([
                new TemplateSectionDto("impact", "Impact", [
                    new QuestionDto("q1", "Question", QuestionType.FreeText, true, null, null, null)
                ])
            ])));

        await cycleService.CreateCycleAsync(new CreateCycleRequest(
            "Cycle 1",
            template.Id,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            [alex.Id],
            [sam.Id]));

        var assignment = await db.Assignments.SingleAsync();
        var feedback = CreateFeedbackService(db, new TestUser("demo.employee", Hr360Roles.Employee));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            feedback.SubmitAsync(new SubmitFeedbackRequest(
                assignment.Id,
                [new AnswerDto("q1", null, "Looks good", null)],
                Guid.NewGuid().ToString("N"))));
    }

    [Fact]
    public async Task Reporting_Suppresses_Reviewee_Results_Below_Anonymity_Threshold()
    {
        await using var db = CreateDb();
        var admin = new TestUser("demo.admin", Hr360Roles.HrAdmin);
        var templateService = CreateTemplateService(db, admin);
        var cycleService = CreateCycleService(db, admin);
        var reporting = CreateReportingService(db);

        var alex = new Employee { EntraObjectId = "demo.employee", DisplayName = "Alex", Email = "alex@example.com" };
        var sam = new Employee { EntraObjectId = "demo.peer1", DisplayName = "Sam", Email = "sam@example.com" };
        var taylor = new Employee { EntraObjectId = "demo.peer2", DisplayName = "Taylor", Email = "taylor@example.com" };
        db.Employees.AddRange(alex, sam, taylor);
        await db.SaveChangesAsync();

        var template = await templateService.CreateTemplateAsync(CreateReportingTemplateRequest());
        await cycleService.CreateCycleAsync(new CreateCycleRequest(
            "Cycle 1",
            template.Id,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            [alex.Id],
            [sam.Id, taylor.Id]));

        await SubmitAllAssignmentsAsync(db);

        var summary = await reporting.GetSummaryAsync();
        var revieweeReport = summary.Cycles.Single().Reviewees.Single();

        Assert.True(revieweeReport.IsSuppressed);
        Assert.Equal(2, revieweeReport.SubmittedCount);
        Assert.Empty(revieweeReport.RatingQuestions);
        Assert.Empty(revieweeReport.YesNoQuestions);
        Assert.Empty(revieweeReport.TextQuestions);
    }

    [Fact]
    public async Task Reporting_Aggregates_Anonymized_Reviewee_Results_When_Threshold_Is_Met()
    {
        await using var db = CreateDb();
        var admin = new TestUser("demo.admin", Hr360Roles.HrAdmin);
        var templateService = CreateTemplateService(db, admin);
        var cycleService = CreateCycleService(db, admin);
        var reporting = CreateReportingService(db);

        var alex = new Employee { EntraObjectId = "demo.employee", DisplayName = "Alex", Email = "alex@example.com" };
        var sam = new Employee { EntraObjectId = "demo.peer1", DisplayName = "Sam Reviewer", Email = "sam@example.com" };
        var taylor = new Employee { EntraObjectId = "demo.peer2", DisplayName = "Taylor Reviewer", Email = "taylor@example.com" };
        var jordan = new Employee { EntraObjectId = "demo.peer3", DisplayName = "Jordan Reviewer", Email = "jordan@example.com" };
        db.Employees.AddRange(alex, sam, taylor, jordan);
        await db.SaveChangesAsync();

        var template = await templateService.CreateTemplateAsync(CreateReportingTemplateRequest());
        await cycleService.CreateCycleAsync(new CreateCycleRequest(
            "Cycle 1",
            template.Id,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            [alex.Id],
            [sam.Id, taylor.Id, jordan.Id]));

        await SubmitAllAssignmentsAsync(db);

        var summary = await reporting.GetSummaryAsync();
        var revieweeReport = summary.Cycles.Single().Reviewees.Single();

        Assert.False(revieweeReport.IsSuppressed);
        Assert.Equal(3, revieweeReport.SubmittedCount);

        var rating = Assert.Single(revieweeReport.RatingQuestions);
        Assert.Equal(3, rating.ResponseCount);
        Assert.Equal(4.0m, rating.Average);
        Assert.Equal(3, rating.Minimum);
        Assert.Equal(5, rating.Maximum);

        var yesNo = Assert.Single(revieweeReport.YesNoQuestions);
        Assert.Equal(2, yesNo.YesCount);
        Assert.Equal(1, yesNo.NoCount);
        Assert.Equal(66.7m, yesNo.YesPercentage);

        var text = Assert.Single(revieweeReport.TextQuestions);
        Assert.Equal(3, text.ResponseCount);

        var reportJson = JsonSerializer.Serialize(summary);
        Assert.DoesNotContain("Sam Reviewer", reportJson);
        Assert.DoesNotContain("Taylor Reviewer", reportJson);
        Assert.DoesNotContain("Jordan Reviewer", reportJson);
        Assert.DoesNotContain("demo.peer", reportJson);
    }

    private static Hr360DbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<Hr360DbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new Hr360DbContext(options);
    }

    private static TemplateService CreateTemplateService(Hr360DbContext db, ICurrentUser user) =>
        new(new ReviewTemplateRepository(db), new AuditEventRepository(db), new UnitOfWork(db), user);

    private static ReviewCycleService CreateCycleService(Hr360DbContext db, ICurrentUser user) =>
        new(
            new ReviewCycleRepository(db),
            new ReviewTemplateRepository(db),
            new EmployeeRepository(db),
            new AuditEventRepository(db),
            new UnitOfWork(db),
            user);

    private static FeedbackService CreateFeedbackService(Hr360DbContext db, ICurrentUser user) =>
        new(
            new ReviewAssignmentRepository(db),
            new FeedbackSubmissionRepository(db),
            new AuditEventRepository(db),
            new UnitOfWork(db),
            user);

    private static ReportingService CreateReportingService(Hr360DbContext db) =>
        new(new ReviewCycleRepository(db));

    private static CreateTemplateRequest CreateReportingTemplateRequest() =>
        new(
            "Core 360",
            "Initial",
            new TemplateDefinitionDto([
                new TemplateSectionDto("impact", "Impact", [
                    new QuestionDto("impact-rating", "Rate impact.", QuestionType.Rating, true, 1, 5, null),
                    new QuestionDto("recommend", "Recommend for broader responsibility?", QuestionType.YesNo, true, null, null, null),
                    new QuestionDto("impact-example", "Share one example.", QuestionType.FreeText, true, null, null, null)
                ])
            ]));

    private static async Task SubmitAllAssignmentsAsync(Hr360DbContext db)
    {
        var assignments = await db.Assignments
            .Include(a => a.Reviewer)
            .OrderBy(a => a.Reviewer!.EntraObjectId)
            .ToListAsync();

        var ratings = new[] { 3, 4, 5 };
        var yesNo = new[] { true, false, true };

        for (var index = 0; index < assignments.Count; index++)
        {
            var assignment = assignments[index];
            var feedback = CreateFeedbackService(db, new TestUser(assignment.Reviewer!.EntraObjectId, Hr360Roles.Employee));
            await feedback.SubmitAsync(new SubmitFeedbackRequest(
                assignment.Id,
                [
                    new AnswerDto("impact-rating", ratings[index], null, null),
                    new AnswerDto("recommend", null, null, yesNo[index]),
                    new AnswerDto("impact-example", null, $"Comment {index + 1}", null)
                ],
                Guid.NewGuid().ToString("N")));
        }
    }

    private sealed class TestUser(string userId, params string[] roles) : ICurrentUser
    {
        public string UserId { get; } = userId;
        public string DisplayName => UserId;
        public bool IsInRole(string role) => roles.Contains(role);
    }
}
