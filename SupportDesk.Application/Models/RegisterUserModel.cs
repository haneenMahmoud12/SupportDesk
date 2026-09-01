namespace SupportDesk.Application.Models;

public sealed class RegisterUserModel
{
    public bool Succeeded { get; init; }
    public string? UserId { get; init; }
    public IReadOnlyCollection<string> Errors { get; init; } = [];
}
