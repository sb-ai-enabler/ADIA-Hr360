using System.Text.Json;
using Hr360.Domain;
using Hr360.Shared;

namespace Hr360.Application;

internal static class Mapping
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static TemplateDefinitionDto ReadDefinition(string json) =>
        JsonSerializer.Deserialize<TemplateDefinitionDto>(json, JsonOptions) ?? new TemplateDefinitionDto([]);

    public static string WriteDefinition(TemplateDefinitionDto definition) =>
        JsonSerializer.Serialize(definition, JsonOptions);

    public static string WriteAnswers(IReadOnlyList<AnswerDto> answers) =>
        JsonSerializer.Serialize(answers, JsonOptions);

    public static ReviewTemplateDto ToDto(this ReviewTemplate template) =>
        new(
            template.Id,
            template.Name,
            template.Description,
            template.Version,
            ReadDefinition(template.DefinitionJson),
            template.CreatedAt,
            template.IsArchived);

    public static EmployeeDto ToDto(this Employee employee) =>
        new(
            employee.Id,
            employee.EntraObjectId,
            employee.DisplayName,
            employee.Email,
            employee.Department,
            employee.IsActive);

    public static ReviewCycleDto ToDto(this ReviewCycle cycle)
    {
        var assignmentCount = cycle.Assignments.Count;
        var submittedCount = cycle.Assignments.Count(a => a.Status == AssignmentStatus.Submitted);

        return new ReviewCycleDto(
            cycle.Id,
            cycle.Name,
            cycle.TemplateId,
            cycle.TemplateVersion,
            cycle.Status,
            cycle.StartsOn,
            cycle.EndsOn,
            assignmentCount,
            submittedCount);
    }

    public static ReviewAssignmentDto ToDto(this ReviewAssignment assignment) =>
        new(
            assignment.Id,
            assignment.CycleId,
            assignment.Cycle?.Name ?? string.Empty,
            assignment.RevieweeId,
            assignment.Reviewee?.DisplayName ?? string.Empty,
            assignment.ReviewerId,
            assignment.Reviewer?.DisplayName ?? string.Empty,
            assignment.Status,
            assignment.SubmittedAt);
}
