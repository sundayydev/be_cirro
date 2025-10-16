using BE_CIRRO.Core.Services;
using BE_CIRRO.Domain.IRepositories;
using BE_CIRRO.Domain.Models;
using BE_CIRRO.Shared.DTOs;
using BE_CIRRO.Shared.DTOs.Auth;
using BE_CIRRO.Shared.DTOs.User;
using Mapster;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BE_CIRRO.Core.Services;

public class AuthService
{
    private readonly UserService _userService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    // Xóa bỏ Dictionary in-memory
    // private readonly Dictionary<string, string> _refreshTokens = new();
    private readonly StackExchange.Redis.IDatabase _redisDb;
    private const string RefreshTokenPrefix = "refresh_token:";

    public AuthService(UserService userService, IConfiguration configuration, ILogger<AuthService> logger, IConnectionMultiplexer redis)
    {
        _userService = userService;
        _configuration = configuration;
        _logger = logger;
        _redisDb = redis.GetDatabase();
    }

    // Helper để lấy key Redis nhất quán
    private string GetRedisKey(string refreshToken) => $"{RefreshTokenPrefix}{refreshToken}";

    // Đăng ký user mới (Không thay đổi)
    public async Task<UserDto?> RegisterAsync(RegisterDto dto)
    {
        try
        {
            var existingUser = await _userService.GetUserByUsernameAsync(dto.Username);
            if (existingUser != null)
            {
                _logger.LogWarning("Username {Username} đã tồn tại", dto.Username);
                return null;
            }

            var createUserDto = new CreateUserDto
            {
                Username = dto.Username,
                Password = HashPassword(dto.Password),
                Email = dto.Email,
                Role = dto.Role
            };

            var user = await _userService.CreateUserAsync(createUserDto.Adapt<User>());
            return user.Adapt<UserDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration for username {Username}", dto.Username);
            throw;
        }
    }

    // Đăng nhập và tạo JWT token
    public async Task<TokenDto?> LoginAsync(LoginDto dto)
    {
        try
        {
            var user = await _userService.GetUserByUsernameAsync(dto.Username);
            if (user == null)
            {
                _logger.LogWarning("Login attempt with non-existent username: {Username}", dto.Username);
                return null;
            }

            if (!VerifyPassword(dto.Password, user.Password))
            {
                _logger.LogWarning("Invalid password for username: {Username}", dto.Username);
                return null;
            }

            // Đổi sang await vì GenerateTokenAsync giờ là async
            var tokenDto = await GenerateTokenAsync(user);

            _logger.LogInformation("User {Username} logged in successfully", dto.Username);
            return tokenDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for username {Username}", dto.Username);
            throw;
        }
    }

    // Lấy thông tin user hiện tại từ JWT token (Không thay đổi)
    public async Task<UserDto?> GetCurrentUserAsync(string userId)
    {
        try
        {
            if (!Guid.TryParse(userId, out var userIdGuid))
            {
                _logger.LogWarning("Invalid user ID format: {UserId}", userId);
                return null;
            }

            var user = await _userService.GetUserByIdAsync(userIdGuid);
            if (user == null)
            {
                _logger.LogWarning("User not found with ID: {UserId}", userId);
                return null;
            }

            return user.Adapt<UserDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user with ID {UserId}", userId);
            throw;
        }
    }

    // Làm mới access token (SỬ DỤNG REDIS)
    public async Task<TokenDto?> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var redisKey = GetRedisKey(refreshToken);
            // 1. Kiểm tra refresh token có tồn tại trong Redis không
            RedisValue userIdValue = await _redisDb.StringGetAsync(redisKey);

            if (userIdValue.IsNullOrEmpty)
            {
                _logger.LogWarning("Invalid or expired refresh token provided: {RefreshToken}", refreshToken);
                return null;
            }

            var userId = userIdValue.ToString();
            var user = await _userService.GetUserByIdAsync(Guid.Parse(userId));
            if (user == null)
            {
                _logger.LogWarning("User not found for refresh token: {UserId}", userId);
                // Dọn dẹp token mồ côi
                await _redisDb.KeyDeleteAsync(redisKey);
                return null;
            }

            // 2. Tạo token mới (Đã await)
            var newTokenDto = await GenerateTokenAsync(user);

            // 3. Xóa refresh token cũ (Token Rotation)
            await _redisDb.KeyDeleteAsync(redisKey);

            _logger.LogInformation("Token refreshed successfully for user {UserId}", userId);
            return newTokenDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token for refresh token: {RefreshToken}", refreshToken);
            throw;
        }
    }

    // Đăng xuất (SỬ DỤNG REDIS)
    public async Task<bool> LogoutAsync(string refreshToken)
    {
        try
        {
            var redisKey = GetRedisKey(refreshToken);
            // Xóa key khỏi Redis
            bool deleted = await _redisDb.KeyDeleteAsync(redisKey);

            if (deleted)
            {
                _logger.LogInformation("User logged out successfully (token revoked)");
            }
            else
            {
                _logger.LogWarning("Logout attempt with non-existent or expired refresh token");
            }

            return deleted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            throw;
        }
    }

    // Đổi mật khẩu (Không thay đổi)
    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found for password change: {UserId}", userId);
                return false;
            }

            if (!VerifyPassword(currentPassword, user.Password))
            {
                _logger.LogWarning("Invalid current password for user: {UserId}", userId);
                return false;
            }

            user.Password = HashPassword(newPassword);
            await _userService.UpdateUserAsync(userId, user);

            _logger.LogInformation("Password changed successfully for user: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserId}", userId);
            throw;
        }
    }

    // Tạo JWT token và refresh token (SỬ DỤNG REDIS)
    // Chuyển thành async Task
    private async Task<TokenDto> GenerateTokenAsync(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? "your-super-secret-key-that-is-at-least-32-characters-long";
        var issuer = jwtSettings["Issuer"] ?? "BE_CIRRO";
        var audience = jwtSettings["Audience"] ?? "BE_CIRRO_Users";
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

        // Thêm cấu hình thời hạn cho Refresh Token
        var refreshExpiryDays = int.Parse(jwtSettings["RefreshExpiryDays"] ?? "7");
        var refreshTokenExpiry = TimeSpan.FromDays(refreshExpiryDays);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("UserId", user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = GenerateRefreshToken();

        // Lưu refresh token vào Redis với thời gian hết hạn
        var redisKey = GetRedisKey(refreshToken);
        bool stored = await _redisDb.StringSetAsync(
            redisKey,
            user.UserId.ToString(),
            refreshTokenExpiry
        );

        if (!stored)
        {
            _logger.LogError("Không thể lưu refresh token vào Redis cho User {UserId}", user.UserId);
            // Ném lỗi để ngăn chặn việc trả token về client nếu không lưu được
            throw new Exception("Lỗi hệ thống: Không thể tạo phiên đăng nhập.");
        }

        var tokenDto = new TokenDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes),
            TokenType = "Bearer"
        };

        // Bỏ Task.FromResult vì hàm đã là async
        return tokenDto;
    }

    // Tạo refresh token (Không thay đổi)
    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    // Hash password (Không thay đổi)
    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    // Verify password (Không thay đổi)
    private bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }

    // Validate JWT token (Không thay đổi)
    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? "your-super-secret-key-that-is-at-least-32-characters-long";
            var issuer = jwtSettings["Issuer"] ?? "BE_CIRRO";
            var audience = jwtSettings["Audience"] ?? "BE_CIRRO_Users";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating JWT token");
            return null;
        }
    }

    // Xóa/Thay đổi các Debug method

    // Phương thức này không còn cần thiết vì Redis tự động dọn dẹp
    // public void CleanupExpiredTokens() { ... }

    // Phương thức này RẤT NGUY HIỂM trong production Redis (dùng KEYS *)
    // Tốt nhất nên xóa bỏ
    // public Dictionary<string, string> GetAllRefreshTokens() { ... }

    // Debug method để kiểm tra refresh token cụ thể (SỬ DỤNG REDIS)
    public async Task<bool> IsRefreshTokenValid(string refreshToken)
    {
        var redisKey = GetRedisKey(refreshToken);
        return await _redisDb.KeyExistsAsync(redisKey);
    }

    // Debug method để lấy thông tin refresh token (SỬ DỤNG REDIS)
    public async Task<string?> GetRefreshTokenInfo(string refreshToken)
    {
        var redisKey = GetRedisKey(refreshToken);
        RedisValue userId = await _redisDb.StringGetAsync(redisKey);

        if (!userId.IsNullOrEmpty)
        {
            return $"RefreshToken (Key): {redisKey}, UserId (Value): {userId}";
        }

        return null;
    }
}