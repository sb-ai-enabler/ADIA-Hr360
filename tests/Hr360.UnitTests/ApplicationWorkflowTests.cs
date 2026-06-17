using Hr360.Application;
using Hr360.Application.Interfaces;
using Hr360.Domain;
using Hr360.Infrastructure;
using Hr360.Infrastructure.Repositories;
using Hr360.Shared;
using Microsoft.EntityFrameworkCore;

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

    private sealed class TestUser(string userId, params string[] roles) : ICurrentUser
    {
        public string UserId { get; } = userId;
        public string DisplayName => UserId;
        public bool IsInRole(string role) => roles.Contains(role);
    }
}
