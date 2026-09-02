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
        try
        {
            var result = await authService.RegisterUserAsync(request);
            return result.Succeeded ? Ok(result) : BadRequest(result);
        }
        catch (Exception)
        {
            return InternalServerError();
        }
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginUserDTO request)
    {
        try
        {
            var result = await authService.LoginUserAsync(request);
            return result.Succeeded ? Ok(result) : Unauthorized(result);
        }
        catch (Exception)
        {
            return InternalServerError();
        }
    }

    private ObjectResult InternalServerError() => StatusCode(
        StatusCodes.Status500InternalServerError,
        new ResponseModel
        {
            Succeeded = false,
            Errors = ["An unexpected error occurred."]
        });
}
