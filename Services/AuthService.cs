using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CollabPlatform.Api.Data;
using CollabPlatform.Api.DTOs;
using CollabPlatform.Api.Models;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

namespace CollabPlatform.Api.Services;

public class AuthService : IAuthService
{
    private readonly MongoDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(MongoDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<TokenResponse> RegisterAsync(RegisterRequest request)
    {
        // Check if email exists
        var existingEmail = await _db.Users.Find(u => u.Email == request.Email.ToLower()).FirstOrDefaultAsync();
        if (existingEmail != null)
            throw new ApplicationException("Email already registered.");

        var user = new User
        {
            Email = request.Email.ToLower(),
            FullName = request.FullName,
            Role = request.Role,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        // Create empty embedded profile
        if (request.Role == "Brand")
            user.BrandProfile = new BrandProfile { CompanyName = request.FullName };
        else
            user.InfluencerProfile = new InfluencerProfile { DisplayName = request.FullName };

        await _db.Users.InsertOneAsync(user);
        return await GenerateTokens(user);
    }

    public async Task<TokenResponse> LoginAsync(LoginRequest request)
    {
        var user = await _db.Users.Find(u => u.Email == request.Email.ToLower()).FirstOrDefaultAsync();
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        return await GenerateTokens(user);
    }

    public async Task<TokenResponse> RefreshTokenAsync(RefreshRequest request)
    {
        // Validate expired access token to get user id
        var principal = GetPrincipalFromExpiredToken(request.AccessToken);
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) throw new UnauthorizedAccessException("Invalid token.");

        var user = await _db.Users.Find(u => u.Id == Guid.Parse(userId)).FirstOrDefaultAsync();
        if (user == null) throw new UnauthorizedAccessException("User not found.");

        // Check if refresh token matches (simplified: compare directly)
        if (user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiry < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        return await GenerateTokens(user);
    }

    private async Task<TokenResponse> GenerateTokens(User user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        // Save refresh token
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(
            double.Parse(_config["JwtSettings:RefreshTokenExpirationDays"]!));
        await _db.Users.ReplaceOneAsync(u => u.Id == user.Id, user);

        return new TokenResponse(accessToken, refreshToken);
    }

    private string GenerateAccessToken(User user)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["AccessTokenExpirationMinutes"]!));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);
        var tokenValidationParams = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParams, out var securityToken);
        if (securityToken is not JwtSecurityToken jwtToken ||
            !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new UnauthorizedAccessException("Invalid token.");

        return principal;
    }
}