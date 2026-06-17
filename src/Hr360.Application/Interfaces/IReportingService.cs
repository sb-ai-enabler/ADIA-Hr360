using Hr360.Shared;

namespace Hr360.Application.Interfaces;

public interface IReportingService
{
    Task<ReportingSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}
