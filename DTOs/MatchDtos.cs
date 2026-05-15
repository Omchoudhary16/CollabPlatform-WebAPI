namespace CollabPlatform.Api.DTOs;

public record BrandMatchDto(Guid BrandId, string CompanyName, int CommonCategoryCount);
public record InfluencerMatchDto(Guid InfluencerId, string DisplayName, int CommonCategoryCount);