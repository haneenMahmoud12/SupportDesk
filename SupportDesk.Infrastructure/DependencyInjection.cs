using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SupportDesk.Application.Services;
using SupportDesk.Application.Services.TicketService;
using SupportDesk.Application.Services.TicketCommentsService;
using SupportDesk.Application.Interfaces.Repositories;
using SupportDesk.Application.Models;
using SupportDesk.Infrastructure.Authentication;
using SupportDesk.Infrastructure.Data;
using SupportDesk.Infrastructure.Services;
using SupportDesk.Infrastructure.Repositories;

namespace SupportDesk.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        var jwtKey = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT signing key is not configured.");
        var jwtKeyBytes = Encoding.UTF8.GetBytes(jwtKey);
        if (jwtKeyBytes.Length < 32)
        {
            throw new InvalidOperationException(
                "JWT signing key must be at least 32 bytes (256 bits) for HS256.");
        }
        var jwtIssuer = configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("JWT issuer is not configured.");
        var jwtAudience = configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("JWT audience is not configured.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        jwtKeyBytes),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(new ResponseModel
                        {
                            Succeeded = false,
                            Errors = ["Authentication is required or the token is invalid."]
                        });
                    },
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsJsonAsync(new ResponseModel
                        {
                            Succeeded = false,
                            Errors = ["You are not authorized to perform this action."]
                        });
                    }
                };
            });

        services.AddAuthorization();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<ITicketCommentsService, TicketCommentsService>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<ITicketCommentsRepository, TicketCommentsRepository>();

        return services;
    }
}
