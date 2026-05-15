using System.Security.Claims;
using CollabPlatform.Api.Data;
using CollabPlatform.Api.DTOs;
using CollabPlatform.Api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace CollabPlatform.Api.Endpoints;

public static class ProfileEndpoints
{
    public static RouteGroupBuilder MapProfileEndpoints(this RouteGroupBuilder group)
    {
        // ---------- GET profile ----------
        group.MapGet("/", async (ClaimsPrincipal user, MongoDbContext db) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var u = await db.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (u == null) return Results.NotFound();

            if (u.Role == "Brand")
                return Results.Ok(new
                {
                    u.Id,
                    u.Email,
                    u.FullName,
                    u.Role,
                    BrandProfile = u.BrandProfile,
                    Categories = u.Categories
                });
            else
                return Results.Ok(new
                {
                    u.Id,
                    u.Email,
                    u.FullName,
                    u.Role,
                    InfluencerProfile = u.InfluencerProfile,
                    Categories = u.Categories
                });
        }).RequireAuthorization();

        // ---------- PUT profile ----------
        group.MapPut("/", async (
            ClaimsPrincipal user,
            [FromBody] UpdateProfileDto dto,
            MongoDbContext db) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var existingUser = await db.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (existingUser == null) return Results.NotFound();

            // Update basic fields
            var update = Builders<User>.Update
                .Set(u => u.FullName, dto.FullName ?? existingUser.FullName)
                .Set(u => u.Categories, dto.Categories ?? existingUser.Categories);

            // Update role-specific profile
            if (existingUser.Role == "Brand" && dto.BrandProfile != null)
            {
                update = update.Set(u => u.BrandProfile, new BrandProfile
                {
                    CompanyName = dto.BrandProfile.CompanyName ?? existingUser.BrandProfile?.CompanyName ?? "",
                    LogoUrl = dto.BrandProfile.LogoUrl ?? existingUser.BrandProfile?.LogoUrl,
                    Industry = dto.BrandProfile.Industry ?? existingUser.BrandProfile?.Industry,
                    Website = dto.BrandProfile.Website ?? existingUser.BrandProfile?.Website,
                    MinBudget = dto.BrandProfile.MinBudget ?? existingUser.BrandProfile?.MinBudget,
                    MaxBudget = dto.BrandProfile.MaxBudget ?? existingUser.BrandProfile?.MaxBudget
                });
            }
            else if (existingUser.Role == "Influencer" && dto.InfluencerProfile != null)
            {
                update = update.Set(u => u.InfluencerProfile, new InfluencerProfile
                {
                    DisplayName = dto.InfluencerProfile.DisplayName ?? existingUser.InfluencerProfile?.DisplayName ?? "",
                    Bio = dto.InfluencerProfile.Bio ?? existingUser.InfluencerProfile?.Bio,
                    FollowerCounts = dto.InfluencerProfile.FollowerCounts ?? existingUser.InfluencerProfile?.FollowerCounts,
                    PlatformLinks = dto.InfluencerProfile.PlatformLinks ?? existingUser.InfluencerProfile?.PlatformLinks,
                    MediaKitUrl = dto.InfluencerProfile.MediaKitUrl ?? existingUser.InfluencerProfile?.MediaKitUrl
                });
            }

            await db.Users.UpdateOneAsync(u => u.Id == userId, update);

            // Return the updated user (same shape as GET)
            var updatedUser = await db.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (updatedUser == null) return Results.NotFound();

            if (updatedUser.Role == "Brand")
                return Results.Ok(new
                {
                    updatedUser.Id,
                    updatedUser.Email,
                    updatedUser.FullName,
                    updatedUser.Role,
                    BrandProfile = updatedUser.BrandProfile,
                    Categories = updatedUser.Categories
                });
            else
                return Results.Ok(new
                {
                    updatedUser.Id,
                    updatedUser.Email,
                    updatedUser.FullName,
                    updatedUser.Role,
                    InfluencerProfile = updatedUser.InfluencerProfile,
                    Categories = updatedUser.Categories
                });
        }).RequireAuthorization();

        return group;
    }
}