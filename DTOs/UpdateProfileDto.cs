namespace CollabPlatform.Api.DTOs;

public class UpdateProfileDto
{
    public string? FullName { get; set; }
    public List<string>? Categories { get; set; }
    public BrandProfileDto? BrandProfile { get; set; }
    public InfluencerProfileDto? InfluencerProfile { get; set; }
}

public class BrandProfileDto
{
    public string? CompanyName { get; set; }
    public string? LogoUrl { get; set; }
    public string? Industry { get; set; }
    public string? Website { get; set; }
    public decimal? MinBudget { get; set; }
    public decimal? MaxBudget { get; set; }
}

public class InfluencerProfileDto
{
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public Dictionary<string, int>? FollowerCounts { get; set; }
    public Dictionary<string, string>? PlatformLinks { get; set; }
    public string? MediaKitUrl { get; set; }
}