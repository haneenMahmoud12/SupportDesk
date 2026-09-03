using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using SupportDesk.Api.Middleware;
using SupportDesk.Application.Models;
using SupportDesk.Infrastructure;
using SupportDesk.Infrastructure.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressMapClientErrors = true;
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                    ? "The request contains an invalid value."
                    : error.ErrorMessage)
                .ToArray();

            return new BadRequestObjectResult(new ResponseModel
            {
                Succeeded = false,
                Errors = errors
            });
        };
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter the JWT access token returned by the login endpoint."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

await app.Services.SeedIdentityAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;
    await response.WriteAsJsonAsync(new ResponseModel
    {
        Succeeded = false,
        Errors = [$"Request failed with status code {response.StatusCode}."]
    });
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
