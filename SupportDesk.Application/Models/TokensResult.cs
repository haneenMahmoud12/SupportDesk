namespace SupportDesk.Application.Models;

public sealed class TokensResult
{
    public string AccessToken { get; init; } = string.Empty;
    public DateTime AccessTokenExpiresAtUtc { get; init; }
    public string TokenType { get; init; } = "Bearer";
}
