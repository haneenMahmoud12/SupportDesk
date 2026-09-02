using System.ComponentModel.DataAnnotations;

namespace SupportDesk.Application.DTOs;

public sealed class RegisterUserDTO
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required, StringLength(256, MinimumLength = 8)]
    public string Password { get; init; } = string.Empty;

    [Required, Compare(nameof(Password))]
    public string ConfirmPassword { get; init; } = string.Empty;
}
