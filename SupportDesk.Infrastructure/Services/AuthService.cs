using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SupportDesk.Application.DTOs;
using SupportDesk.Application.Constants;
using SupportDesk.Application.Models;
using SupportDesk.Application.Services;
using SupportDesk.Infrastructure.Authentication;

namespace SupportDesk.Infrastructure.Services;

public sealed class AuthService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IConfiguration configuration) : IAuthService
{
    public async Task<RegisterUserModel> RegisterUserAsync(RegisterUserDTO request)
    {
        var user = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.Email
        };

        var createResult = await userManager.CreateAsync(user, request.Password);

        if (!createResult.Succeeded)
        {
            return new RegisterUserModel
            {
                Succeeded = false,
                Errors = createResult.Errors.Select(error => error.Description).ToArray()
            };
        }

        if (!await roleManager.RoleExistsAsync(RoleNames.User))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(RoleNames.User));
            if (!roleResult.Succeeded)
            {
                await userManager.DeleteAsync(user);
                return new RegisterUserModel
                {
                    Succeeded = false,
                    Errors = roleResult.Errors.Select(error => error.Description).ToArray()
                };
            }
        }

        var addToRoleResult = await userManager.AddToRoleAsync(user, RoleNames.User);
        if (!addToRoleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return new RegisterUserModel
            {
                Succeeded = false,
                Errors = addToRoleResult.Errors.Select(error => error.Description).ToArray()
            };
        }

        return new RegisterUserModel
        {
            Succeeded = true,
            UserId = user.Id
        };
    }

    public async Task<LoginUserModel> LoginUserAsync(LoginUserDTO request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            return FailedLogin();
        }

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(
            configuration.GetValue<double?>("Jwt:AccessTokenExpirationMinutes") ?? 60);
        var token = await CreateAccessTokenAsync(user, expiresAtUtc);

        return new LoginUserModel
        {
            Succeeded = true,
            UserId = user.Id,
            Tokens = new TokensResult
            {
                AccessToken = token,
                AccessTokenExpiresAtUtc = expiresAtUtc
            }
        };
    }

    private async Task<string> CreateAccessTokenAsync(
        ApplicationUser user,
        DateTime expiresAtUtc)
    {
        var key = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT signing key is not configured.");
        var issuer = configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("JWT issuer is not configured.");
        var audience = configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("JWT audience is not configured.");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id)
        };

        var roles = await userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static LoginUserModel FailedLogin() => new()
    {
        Succeeded = false,
        Errors = ["Invalid email or password."]
    };
}
