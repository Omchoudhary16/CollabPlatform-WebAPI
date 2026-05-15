namespace CollabPlatform.Api.DTOs;

public record SendRequestDto(Guid InfluencerId, string CampaignDetails);
public record CollaborationRequestDto(
    Guid Id,
    Guid BrandId,
    string CompanyName,
    Guid InfluencerId,
    string InfluencerName,
    string CampaignDetails,
    string Status,
    DateTime CreatedAt);