using Hr360.Shared;

namespace Hr360.Application.Interfaces;

public interface IReviewCycleService
{
    Task<IReadOnlyList<EmployeeDto>> GetEmployeesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReviewCycleDto>> GetCyclesAsync(CancellationToken cancellationToken = default);
    Task<ReviewCycleDto> CreateCycleAsync(CreateCycleRequest request, CancellationToken cancellationToken = default);
}
