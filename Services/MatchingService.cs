using CollabPlatform.Api.Data;
using CollabPlatform.Api.DTOs;
using CollabPlatform.Api.Models;
using MongoDB.Driver;

namespace CollabPlatform.Api.Services;

public class MatchingService : IMatchingService
{
    private readonly MongoDbContext _db;

    public MatchingService(MongoDbContext db) => _db = db;

    public async Task<List<BrandMatchDto>> GetMatchingBrandsForInfluencerAsync(Guid influencerId)
    {
        // Get influencer's categories
        var influencer = await _db.Users.Find(u => u.Id == influencerId).FirstOrDefaultAsync();
        if (influencer == null || !influencer.Categories.Any()) return new();

        var influCategories = influencer.Categories;

        // Find brands where Categories array overlaps
        var brands = await _db.Users.Find(u => u.Role == "Brand" && u.Categories.Any(c => influCategories.Contains(c)))
            .ToListAsync();

        // Compute common count and sort descending
        var matches = brands
            .Select(b => new BrandMatchDto(
                b.Id,
                b.BrandProfile?.CompanyName ?? "Unknown",
                b.Categories.Count(c => influCategories.Contains(c))
            ))
            .OrderByDescending(m => m.CommonCategoryCount)
            .Take(50)
            .ToList();

        return matches;
    }

    public async Task<List<InfluencerMatchDto>> GetMatchingInfluencersForBrandAsync(Guid brandId)
    {
        var brand = await _db.Users.Find(u => u.Id == brandId).FirstOrDefaultAsync();
        if (brand == null || !brand.Categories.Any()) return new();

        var brandCategories = brand.Categories;

        var influencers = await _db.Users.Find(u => u.Role == "Influencer" && u.Categories.Any(c => brandCategories.Contains(c)))
            .ToListAsync();

        var matches = influencers
            .Select(i => new InfluencerMatchDto(
                i.Id,
                i.InfluencerProfile?.DisplayName ?? "Unknown",
                i.Categories.Count(c => brandCategories.Contains(c))
            ))
            .OrderByDescending(m => m.CommonCategoryCount)
            .Take(50)
            .ToList();

        return matches;
    }
}