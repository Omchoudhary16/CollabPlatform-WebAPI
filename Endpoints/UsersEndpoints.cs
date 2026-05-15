using CollabPlatform.Api.Data;
using CollabPlatform.Api.Models;
using MongoDB.Driver;
using System.Security.Claims;

namespace CollabPlatform.Api.Endpoints;

public static class UsersEndpoints
{
    public static RouteGroupBuilder MapUsersEndpoints(this RouteGroupBuilder group)
    {
        // Get all users of a specific role (opposite of current user)
        group.MapGet("/", async (
            ClaimsPrincipal user,
            MongoDbContext db,
            string? role,
            string? search,
            string? category) =>
        {
            var currentUserId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var currentUser = await db.Users.Find(u => u.Id == currentUserId).FirstOrDefaultAsync();
            if (currentUser == null) return Results.Unauthorized();

            // If role not provided, default to opposite of current user
            var targetRole = role ?? (currentUser.Role == "Brand" ? "Influencer" : "Brand");

            var builder = Builders<User>.Filter.Where(u => u.Id != currentUserId && u.Role == targetRole);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchFilter = Builders<User>.Filter.Regex(u => u.FullName, new MongoDB.Bson.BsonRegularExpression(search, "i"));
                builder = builder & searchFilter;
            }
            if (!string.IsNullOrWhiteSpace(category))
            {
                // category filter: user's Categories array contains the given category
                var catFilter = Builders<User>.Filter.AnyEq(u => u.Categories, category);
                builder = builder & catFilter;
            }

            var users = await db.Users.Find(builder)
                .Project(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Role,
                    u.Categories,
                    BrandProfile = u.Role == "Brand" ? new
                    {
                        u.BrandProfile!.CompanyName,
                        u.BrandProfile.LogoUrl,
                        u.BrandProfile.Industry,
                        u.BrandProfile.Website
                    } : null,
                    InfluencerProfile = u.Role == "Influencer" ? new
                    {
                        u.InfluencerProfile!.DisplayName,
                        u.InfluencerProfile.Bio,
                        u.InfluencerProfile.FollowerCounts,
                        u.InfluencerProfile.PlatformLinks,
                        u.InfluencerProfile.MediaKitUrl
                    } : null
                })
                .ToListAsync();

            return Results.Ok(users);
        }).RequireAuthorization();

        // Get single user public profile
        group.MapGet("/{id:guid}", async (Guid id, MongoDbContext db) =>
        {
            var user = await db.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null) return Results.NotFound();

            return Results.Ok(new
            {
                user.Id,
                user.FullName,
                user.Role,
                user.Categories,
                BrandProfile = user.Role == "Brand" ? new
                {
                    user.BrandProfile!.CompanyName,
                    user.BrandProfile.LogoUrl,
                    user.BrandProfile.Industry,
                    user.BrandProfile.Website,
                    user.BrandProfile.MinBudget,
                    user.BrandProfile.MaxBudget
                } : null,
                InfluencerProfile = user.Role == "Influencer" ? new
                {
                    user.InfluencerProfile!.DisplayName,
                    user.InfluencerProfile.Bio,
                    user.InfluencerProfile.FollowerCounts,
                    user.InfluencerProfile.PlatformLinks,
                    user.InfluencerProfile.MediaKitUrl
                } : null
            });
        }).RequireAuthorization();

        return group;
    }
}