using SupportDesk.Application.DTOs;
using SupportDesk.Application.Models;

namespace SupportDesk.Application.Services;

public interface IAuthService
{
    Task<RegisterUserModel> RegisterUserAsync(RegisterUserDTO user);
    Task<LoginUserModel> LoginUserAsync(LoginUserDTO user);
}
