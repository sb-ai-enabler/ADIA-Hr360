namespace Hr360.Shared;

public sealed record EmployeeDto(
    Guid Id,
    string EntraObjectId,
    string DisplayName,
    string Email,
    string? Department,
    bool IsActive);
