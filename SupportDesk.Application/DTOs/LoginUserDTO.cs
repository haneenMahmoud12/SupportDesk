using System.ComponentModel.DataAnnotations;

namespace SupportDesk.Application.DTOs;

public sealed class LoginUserDTO
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required, StringLength(256)]
    public string Password { get; init; } = string.Empty;
}
