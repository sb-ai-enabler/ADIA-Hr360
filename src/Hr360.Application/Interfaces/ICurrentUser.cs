namespace Hr360.Application.Interfaces;

public interface ICurrentUser
{
    string UserId { get; }
    string DisplayName { get; }
    bool IsInRole(string role);
}
