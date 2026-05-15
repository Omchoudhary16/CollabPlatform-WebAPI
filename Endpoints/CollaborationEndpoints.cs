using System.Security.Claims;
using CollabPlatform.Api.DTOs;
using CollabPlatform.Api.Hubs;
using CollabPlatform.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CollabPlatform.Api.Endpoints;

public static class CollaborationEndpoints
{
    public static RouteGroupBuilder MapCollaborationEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/request", async (
            [FromBody] SendRequestDto dto,
            ClaimsPrincipal user,
            ICollaborationService collabService,
            IHubContext<NotificationHub> hubContext) =>
        {
            var brandId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var request = await collabService.SendRequestAsync(brandId, dto);

            await hubContext.Clients.User(dto.InfluencerId.ToString())
                .SendAsync("NewCollaborationRequest", request);

            return Results.Ok(request);
        }).RequireAuthorization("BrandOnly");

        // Accept (influencer)
        group.MapPost("/{requestId:guid}/accept", async (
            Guid requestId,
            ClaimsPrincipal user,
            ICollaborationService collabService) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await collabService.AcceptRequestAsync(requestId, userId);
            return Results.Ok(new { success = true });   // <-- changed
        }).RequireAuthorization("InfluencerOnly");

        // Decline
        group.MapPost("/{requestId:guid}/decline", async (
            Guid requestId,
            ClaimsPrincipal user,
            ICollaborationService collabService) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await collabService.DeclineRequestAsync(requestId, userId);
            return Results.Ok(new { success = true });   // <-- changed
        }).RequireAuthorization("InfluencerOnly");

        group.MapGet("/received", async (
            ClaimsPrincipal user,
            ICollaborationService collabService) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var list = await collabService.GetReceivedRequestsAsync(userId);
            return Results.Ok(list);
        }).RequireAuthorization();

        group.MapGet("/sent", async (
            ClaimsPrincipal user,
            ICollaborationService collabService) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var list = await collabService.GetSentRequestsAsync(userId);
            return Results.Ok(list);
        }).RequireAuthorization();

        return group;
    }
}