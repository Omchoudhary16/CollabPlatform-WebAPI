using System.Security.Claims;
using CollabPlatform.Api.Services;

namespace CollabPlatform.Api.Endpoints;

public static class MatchEndpoints
{
    public static RouteGroupBuilder MapMatchEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/brands", async (ClaimsPrincipal user, IMatchingService matcher) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var matches = await matcher.GetMatchingBrandsForInfluencerAsync(userId);
            return Results.Ok(matches);
        }).RequireAuthorization("InfluencerOnly");

        group.MapGet("/influencers", async (ClaimsPrincipal user, IMatchingService matcher) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var matches = await matcher.GetMatchingInfluencersForBrandAsync(userId);
            return Results.Ok(matches);
        }).RequireAuthorization("BrandOnly");

        return group;
    }
}