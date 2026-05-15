using CollabPlatform.Api.DTOs;

namespace CollabPlatform.Api.Services;

public interface IMatchingService
{
    Task<List<BrandMatchDto>> GetMatchingBrandsForInfluencerAsync(Guid influencerId);
    Task<List<InfluencerMatchDto>> GetMatchingInfluencersForBrandAsync(Guid brandId);
}