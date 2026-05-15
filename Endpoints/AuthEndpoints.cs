using CollabPlatform.Api.DTOs;
using CollabPlatform.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CollabPlatform.Api.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/register", async ([FromBody] RegisterRequest request, IAuthService authService) =>
        {
            try
            {
                var tokens = await authService.RegisterAsync(request);
                return Results.Ok(tokens);
            }
            catch (ApplicationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        group.MapPost("/login", async ([FromBody] LoginRequest request, IAuthService authService) =>
        {
            try
            {
                var tokens = await authService.LoginAsync(request);
                return Results.Ok(tokens);
            }
            catch (UnauthorizedAccessException)
            {
               return Results.Unauthorized();
            }
        });

        group.MapPost("/refresh", async ([FromBody] RefreshRequest request, IAuthService authService) =>
        {
            try
            {
                var tokens = await authService.RefreshTokenAsync(request);
                return Results.Ok(tokens);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
        });

        return group;
    }
}