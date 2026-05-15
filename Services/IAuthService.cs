using CollabPlatform.Api.DTOs;

namespace CollabPlatform.Api.Services;

public interface IAuthService
{
    Task<TokenResponse> RegisterAsync(RegisterRequest request);
    Task<TokenResponse> LoginAsync(LoginRequest request);
    Task<TokenResponse> RefreshTokenAsync(RefreshRequest request);
}