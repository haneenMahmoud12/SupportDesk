using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportDesk.Application.DTOs;
using SupportDesk.Application.Models;
using SupportDesk.Application.Services;

namespace SupportDesk.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserDTO request)
    {
        var result = await authService.RegisterUserAsync(request);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginUserDTO request)
    {
        var result = await authService.LoginUserAsync(request);
        return result.Succeeded ? Ok(result) : Unauthorized(result);
    }
}
