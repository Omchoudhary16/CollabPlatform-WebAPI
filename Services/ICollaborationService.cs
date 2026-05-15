using CollabPlatform.Api.DTOs;

namespace CollabPlatform.Api.Services;

public interface ICollaborationService
{
    Task<CollaborationRequestDto> SendRequestAsync(Guid brandId, SendRequestDto dto);
    Task AcceptRequestAsync(Guid requestId, Guid userId);
    Task DeclineRequestAsync(Guid requestId, Guid userId);
    Task<List<CollaborationRequestDto>> GetReceivedRequestsAsync(Guid userId);
    Task<List<CollaborationRequestDto>> GetSentRequestsAsync(Guid userId);
}