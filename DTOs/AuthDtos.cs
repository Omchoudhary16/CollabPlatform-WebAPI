namespace CollabPlatform.Api.DTOs;

public record RegisterRequest(string Email, string Password, string FullName, string Role);
public record LoginRequest(string Email, string Password);
public record TokenResponse(string AccessToken, string RefreshToken);
public record RefreshRequest(string AccessToken, string RefreshToken);
