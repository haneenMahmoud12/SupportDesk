namespace SupportDesk.Application.Models;

public sealed class LoginUserModel : ResponseModel
{
    public string? UserId { get; init; }
    public TokensResult? Tokens { get; init; }
}
