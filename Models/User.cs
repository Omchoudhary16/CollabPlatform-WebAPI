using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CollabPlatform.Api.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty; // BCrypt hash
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "Influencer"; // "Brand" or "Influencer"
    public List<string> Categories { get; set; } = new(); // category names

    // Embedded profiles (null if not applicable)
    public BrandProfile? BrandProfile { get; set; }
    public InfluencerProfile? InfluencerProfile { get; set; }

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
}

public class BrandProfile
{
    public string CompanyName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Industry { get; set; }
    public string? Website { get; set; }
    public decimal? MinBudget { get; set; }
    public decimal? MaxBudget { get; set; }
}

public class InfluencerProfile
{
    public string DisplayName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public Dictionary<string, int>? FollowerCounts { get; set; } // platform -> count
    public Dictionary<string, string>? PlatformLinks { get; set; } // platform -> link
    public string? MediaKitUrl { get; set; }
}