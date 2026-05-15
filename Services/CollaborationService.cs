using CollabPlatform.Api.Data;
using CollabPlatform.Api.DTOs;
using CollabPlatform.Api.Models;
using MongoDB.Driver;

namespace CollabPlatform.Api.Services;

public class CollaborationService : ICollaborationService
{
    private readonly MongoDbContext _db;

    public CollaborationService(MongoDbContext db) => _db = db;

    public async Task<CollaborationRequestDto> SendRequestAsync(Guid brandId, SendRequestDto dto)
    {
        // Verify both exist
        var brand = await _db.Users.Find(u => u.Id == brandId && u.Role == "Brand").FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Brand not found.");
        var influencer = await _db.Users.Find(u => u.Id == dto.InfluencerId && u.Role == "Influencer").FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Influencer not found.");

        var request = new CollaborationRequest
        {
            BrandId = brandId,
            InfluencerId = dto.InfluencerId,
            CampaignDetailsJson = dto.CampaignDetails
        };

        await _db.CollaborationRequests.InsertOneAsync(request);

        return new CollaborationRequestDto(
            request.Id,
            brandId,
            brand.BrandProfile?.CompanyName ?? "",
            dto.InfluencerId,
            influencer.InfluencerProfile?.DisplayName ?? "",
            request.CampaignDetailsJson,
            request.Status.ToString(),
            request.CreatedAt
        );
    }

    public async Task AcceptRequestAsync(Guid requestId, Guid userId)
    {
        var filter = Builders<CollaborationRequest>.Filter.Where(
            r => r.Id == requestId && r.InfluencerId == userId && r.Status == RequestStatus.Pending);
        var update = Builders<CollaborationRequest>.Update
            .Set(r => r.Status, RequestStatus.Accepted)
            .Set(r => r.UpdatedAt, DateTime.UtcNow);
        var result = await _db.CollaborationRequests.UpdateOneAsync(filter, update);
        if (result.MatchedCount == 0)
            throw new InvalidOperationException("Request not found or not pending.");
    }

    public async Task DeclineRequestAsync(Guid requestId, Guid userId)
    {
        var filter = Builders<CollaborationRequest>.Filter.Where(
            r => r.Id == requestId && r.InfluencerId == userId && r.Status == RequestStatus.Pending);
        var update = Builders<CollaborationRequest>.Update
            .Set(r => r.Status, RequestStatus.Declined)
            .Set(r => r.UpdatedAt, DateTime.UtcNow);
        var result = await _db.CollaborationRequests.UpdateOneAsync(filter, update);
        if (result.MatchedCount == 0)
            throw new InvalidOperationException("Request not found or not pending.");
    }

    public async Task<List<CollaborationRequestDto>> GetReceivedRequestsAsync(Guid userId)
    {
        var requests = await _db.CollaborationRequests.Find(r => r.InfluencerId == userId).ToListAsync();
        return await MapToDtoList(requests);
    }

    public async Task<List<CollaborationRequestDto>> GetSentRequestsAsync(Guid userId)
    {
        var requests = await _db.CollaborationRequests.Find(r => r.BrandId == userId).ToListAsync();
        return await MapToDtoList(requests);
    }

    private async Task<List<CollaborationRequestDto>> MapToDtoList(List<CollaborationRequest> requests)
    {
        // We need brand and influencer names – cache users?
        var userIds = requests.Select(r => r.BrandId)
            .Union(requests.Select(r => r.InfluencerId))
            .Distinct()
            .ToList();

        var users = await _db.Users.Find(u => userIds.Contains(u.Id)).ToListAsync();
        var userMap = users.ToDictionary(u => u.Id);

        return requests.Select(r =>
        {
            var brand = userMap.GetValueOrDefault(r.BrandId);
            var influencer = userMap.GetValueOrDefault(r.InfluencerId);
            return new CollaborationRequestDto(
                r.Id,
                r.BrandId,
                brand?.BrandProfile?.CompanyName ?? "Unknown",
                r.InfluencerId,
                influencer?.InfluencerProfile?.DisplayName ?? "Unknown",
                r.CampaignDetailsJson,
                r.Status.ToString(),
                r.CreatedAt
            );
        }).ToList();
    }
}