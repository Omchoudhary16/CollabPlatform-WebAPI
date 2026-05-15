using System.Security.Claims;
using CollabPlatform.Api.Data;
using CollabPlatform.Api.Models;
using MongoDB.Driver;

namespace CollabPlatform.Api.Endpoints;

public static class NotificationEndpoints
{
    public static RouteGroupBuilder MapNotificationEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/count", async (ClaimsPrincipal user, MongoDbContext db) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var filter = Builders<CollaborationRequest>.Filter.And(
                Builders<CollaborationRequest>.Filter.Eq(r => r.InfluencerId, userId),
                Builders<CollaborationRequest>.Filter.Eq(r => r.Status, RequestStatus.Pending)
            );
            var count = await db.CollaborationRequests.CountDocumentsAsync(filter);
            return Results.Ok(new { count });
        }).RequireAuthorization();

        return group;
    }
}