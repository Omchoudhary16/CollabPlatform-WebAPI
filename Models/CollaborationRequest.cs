using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CollabPlatform.Api.Models;

public class CollaborationRequest
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [BsonRepresentation(BsonType.String)]
    public Guid BrandId { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid InfluencerId { get; set; }

    public string CampaignDetailsJson { get; set; } = "{}"; // JSON string
    public RequestStatus Status { get; set; } = RequestStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public enum RequestStatus
{
    Pending,
    Accepted,
    Declined,
    Completed
}