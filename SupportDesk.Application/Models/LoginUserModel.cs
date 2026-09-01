namespace SupportDesk.Application.Models;

public sealed class LoginUserModel
{
    public bool Succeeded { get; init; }
    public string? UserId { get; init; }
    public TokensResult? Tokens { get; init; }
    public IReadOnlyCollection<string> Errors { get; init; } = [];
}
