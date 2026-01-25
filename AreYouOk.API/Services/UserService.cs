using AreYouOk.API.Data;
using AreYouOk.API.Models;
using AreYouOk.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AreYouOk.API.Services;

/// <summary>
/// Service for user authentication and profile management
/// </summary>
public class UserService
{
    private readonly AreYouOkDbContext _context;
    private readonly PasswordHasher _passwordHasher;
    private readonly TokenService _tokenService;

    public UserService(
        AreYouOkDbContext context,
        PasswordHasher passwordHasher,
        TokenService tokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto dto)
    {
        // Check if email already exists
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
        {
            return ApiResponse<AuthResponseDto>.ErrorResponse("Email already registered");
        }

        // Check if phone number already exists
        if (await _context.Users.AnyAsync(u => u.PhoneNumber == dto.PhoneNumber))
        {
            return ApiResponse<AuthResponseDto>.ErrorResponse("Phone number already registered");
        }

        // Create new user
        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            PasswordHash = _passwordHasher.HashPassword(dto.Password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Generate token
        var token = _tokenService.GenerateToken(user.Id, user.Email);
        var expiryHours = _tokenService.GetExpiryHours();

        var response = new AuthResponseDto
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(expiryHours)
        };

        return ApiResponse<AuthResponseDto>.SuccessResponse(response);
    }

    /// <summary>
    /// Login a user
    /// </summary>
    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        // Find user by email
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null)
        {
            return ApiResponse<AuthResponseDto>.ErrorResponse("Invalid email or password");
        }

        // Verify password
        if (!_passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
        {
            return ApiResponse<AuthResponseDto>.ErrorResponse("Invalid email or password");
        }

        // Generate token
        var token = _tokenService.GenerateToken(user.Id, user.Email);
        var expiryHours = _tokenService.GetExpiryHours();

        var response = new AuthResponseDto
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(expiryHours)
        };

        return ApiResponse<AuthResponseDto>.SuccessResponse(response);
    }

    /// <summary>
    /// Get user profile by ID
    /// </summary>
    public async Task<ApiResponse<UserProfileDto>> GetProfileAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return ApiResponse<UserProfileDto>.ErrorResponse("User not found");
        }

        var profile = new UserProfileDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber
        };

        return ApiResponse<UserProfileDto>.SuccessResponse(profile);
    }

    /// <summary>
    /// Get user by phone number
    /// </summary>
    public async Task<User?> GetUserByPhoneAsync(string phoneNumber)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
    }

    /// <summary>
    /// Update user profile
    /// </summary>
    public async Task<ApiResponse<UserProfileDto>> UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return ApiResponse<UserProfileDto>.ErrorResponse("User not found");
        }

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        
        if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
        {
            // Check if phone number is already used by another user
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == dto.PhoneNumber && u.Id != userId);
            
            if (existingUser != null)
            {
                return ApiResponse<UserProfileDto>.ErrorResponse("Phone number already in use");
            }
            
            user.PhoneNumber = dto.PhoneNumber;
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var profile = new UserProfileDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber
        };

        return ApiResponse<UserProfileDto>.SuccessResponse(profile);
    }
}
