using Hr360.Application.Interfaces;
using Hr360.Domain;
using Hr360.Shared;

namespace Hr360.Application;

public sealed class TemplateService(
    IReviewTemplateRepository templates,
    IAuditEventRepository auditEvents,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser) : ITemplateService
{
    public async Task<IReadOnlyList<ReviewTemplateDto>> GetTemplatesAsync(CancellationToken cancellationToken = default)
    {
        var items = await templates.GetAllOrderedByCreatedDescAsync(cancellationToken);
        return items.Select(t => t.ToDto()).ToList();
    }

    public async Task<ReviewTemplateDto> CreateTemplateAsync(CreateTemplateRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new InvalidOperationException("Template name is required.");
        }

        if (request.Definition.Sections.Count == 0 || request.Definition.Sections.All(s => s.Questions.Count == 0))
        {
            throw new InvalidOperationException("At least one question is required.");
        }

        var template = new ReviewTemplate
        {
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            DefinitionJson = Mapping.WriteDefinition(request.Definition),
            CreatedBy = currentUser.UserId
        };

        templates.Add(template);
        auditEvents.Add(new AuditEvent
        {
            Actor = currentUser.UserId,
            Action = "template.created",
            EntityType = nameof(ReviewTemplate),
            EntityId = template.Id
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return template.ToDto();
    }
}
