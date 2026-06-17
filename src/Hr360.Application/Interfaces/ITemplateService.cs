using Hr360.Shared;

namespace Hr360.Application.Interfaces;

public interface ITemplateService
{
    Task<IReadOnlyList<ReviewTemplateDto>> GetTemplatesAsync(CancellationToken cancellationToken = default);
    Task<ReviewTemplateDto> CreateTemplateAsync(CreateTemplateRequest request, CancellationToken cancellationToken = default);
}
