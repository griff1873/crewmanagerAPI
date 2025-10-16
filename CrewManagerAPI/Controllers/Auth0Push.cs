using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;
using CrewManagerData;
using CrewManagerData.Models;

namespace CrewManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class Auth0Push : ControllerBase
{
    private readonly CMDBContext _context;

    public Auth0Push(CMDBContext context)
    {
        _context = context;
    }

    [HttpPost("user")]
    [Authorize(AuthenticationSchemes = "ApiKey")]
    public async Task<IActionResult> CreateOrUpdateUser([FromBody] Auth0UserData userData)
    {
        try
        {
            if (userData == null)
            {
                return BadRequest("User data is required");
            }

            // Check if user already exists
            var existingUser = await _context.Auth0Users
                .Include(u => u.Identities)
                .ThenInclude(i => i.ProfileData)
                .FirstOrDefaultAsync(u => u.Auth0UserId == userData.UserId);

            if (existingUser != null)
            {
                // Update existing user
                await UpdateUserFromData(existingUser, userData);
                await _context.SaveChangesAsync();
                return Ok(new { message = "User updated successfully", userId = userData.UserId, id = existingUser.Id });
            }
            else
            {
                // Create new user
                var newUser = await CreateUserFromData(userData);
                _context.Auth0Users.Add(newUser);
                await _context.SaveChangesAsync();
                return Ok(new { message = "User created successfully", userId = userData.UserId, id = newUser.Id });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet("user/{auth0UserId}")]
    [Authorize(AuthenticationSchemes = "ApiKey")]
    public async Task<IActionResult> GetUser(string auth0UserId)
    {
        try
        {
            var user = await _context.Auth0Users
                .Include(u => u.Identities)
                .ThenInclude(i => i.ProfileData)
                .FirstOrDefaultAsync(u => u.Auth0UserId == auth0UserId);

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpPut("user/{auth0UserId}")]
    [Authorize(AuthenticationSchemes = "ApiKey")]
    public async Task<IActionResult> UpdateUser(string auth0UserId, [FromBody] Auth0UserData userData)
    {
        try
        {
            var existingUser = await _context.Auth0Users
                .Include(u => u.Identities)
                .ThenInclude(i => i.ProfileData)
                .FirstOrDefaultAsync(u => u.Auth0UserId == auth0UserId);

            if (existingUser == null)
            {
                return NotFound(new { message = "User not found" });
            }

            await UpdateUserFromData(existingUser, userData);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User updated successfully", userId = auth0UserId, id = existingUser.Id });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpDelete("user/{auth0UserId}")]
    [Authorize(AuthenticationSchemes = "ApiKey")]
    public async Task<IActionResult> DeleteUser(string auth0UserId)
    {
        try
        {
            var user = await _context.Auth0Users
                .Include(u => u.Identities)
                .FirstOrDefaultAsync(u => u.Auth0UserId == auth0UserId);

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Soft delete
            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            user.DeletedBy = "API";

            // Also soft delete associated identities
            foreach (var identity in user.Identities)
            {
                identity.IsDeleted = true;
                identity.DeletedAt = DateTime.UtcNow;
                identity.DeletedBy = "API";
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "User deleted successfully", userId = auth0UserId });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet("users")]
    [Authorize(AuthenticationSchemes = "ApiKey")]
    public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            var users = await _context.Auth0Users
                .Where(u => !u.IsDeleted)
                .Include(u => u.Identities.Where(i => !i.IsDeleted))
                .ThenInclude(i => i.ProfileData)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalCount = await _context.Auth0Users.Where(u => !u.IsDeleted).CountAsync();

            return Ok(new
            {
                users,
                pagination = new
                {
                    page,
                    pageSize,
                    totalCount,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    private async Task<Auth0User> CreateUserFromData(Auth0UserData userData)
    {
        var user = new Auth0User
        {
            Auth0UserId = userData.UserId,
            Email = userData.Email,
            EmailVerified = userData.EmailVerified,
            Username = userData.Username,
            PhoneNumber = userData.PhoneNumber,
            PhoneVerified = userData.PhoneVerified,
            Auth0CreatedAt = userData.CreatedAt,
            Auth0UpdatedAt = userData.UpdatedAt,
            AppMetadata = JsonSerializer.Serialize(userData.AppMetadata),
            UserMetadata = JsonSerializer.Serialize(userData.UserMetadata),
            Picture = userData.Picture,
            Name = userData.Name,
            Nickname = userData.Nickname,
            Multifactor = JsonSerializer.Serialize(userData.Multifactor),
            LastIp = userData.LastIp,
            LastLogin = userData.LastLogin,
            LoginsCount = userData.LoginsCount,
            Blocked = userData.Blocked,
            GivenName = userData.GivenName,
            FamilyName = userData.FamilyName,
            CreatedBy = "Auth0Webhook"
        };

        // Process identities
        foreach (var identityData in userData.Identities)
        {
            var profileData = new Auth0ProfileData
            {
                Email = identityData.ProfileData.Email,
                EmailVerified = identityData.ProfileData.EmailVerified,
                Name = identityData.ProfileData.Name,
                Username = identityData.ProfileData.Username,
                GivenName = identityData.ProfileData.GivenName,
                FamilyName = identityData.ProfileData.FamilyName,
                PhoneNumber = identityData.ProfileData.PhoneNumber,
                PhoneVerified = identityData.ProfileData.PhoneVerified,
                CreatedBy = "Auth0Webhook"
            };

            var identity = new Auth0Identity
            {
                Provider = identityData.Provider,
                IsSocial = identityData.IsSocial,
                Connection = identityData.Connection,
                UserId = identityData.UserId,
                Auth0UserId = userData.UserId,
                ProfileData = profileData,
                CreatedBy = "Auth0Webhook"
            };

            user.Identities.Add(identity);
        }

        return user;
    }

    private async Task UpdateUserFromData(Auth0User existingUser, Auth0UserData userData)
    {
        // Update user properties
        existingUser.Email = userData.Email;
        existingUser.EmailVerified = userData.EmailVerified;
        existingUser.Username = userData.Username;
        existingUser.PhoneNumber = userData.PhoneNumber;
        existingUser.PhoneVerified = userData.PhoneVerified;
        existingUser.Auth0UpdatedAt = userData.UpdatedAt;
        existingUser.AppMetadata = JsonSerializer.Serialize(userData.AppMetadata);
        existingUser.UserMetadata = JsonSerializer.Serialize(userData.UserMetadata);
        existingUser.Picture = userData.Picture;
        existingUser.Name = userData.Name;
        existingUser.Nickname = userData.Nickname;
        existingUser.Multifactor = JsonSerializer.Serialize(userData.Multifactor);
        existingUser.LastIp = userData.LastIp;
        existingUser.LastLogin = userData.LastLogin;
        existingUser.LoginsCount = userData.LoginsCount;
        existingUser.Blocked = userData.Blocked;
        existingUser.GivenName = userData.GivenName;
        existingUser.FamilyName = userData.FamilyName;
        existingUser.UpdatedAt = DateTime.UtcNow;
        existingUser.UpdatedBy = "Auth0Webhook";

        // Remove existing identities (soft delete)
        foreach (var identity in existingUser.Identities.Where(i => !i.IsDeleted))
        {
            identity.IsDeleted = true;
            identity.DeletedAt = DateTime.UtcNow;
            identity.DeletedBy = "Auth0Webhook";
        }

        // Add new identities
        foreach (var identityData in userData.Identities)
        {
            var profileData = new Auth0ProfileData
            {
                Email = identityData.ProfileData.Email,
                EmailVerified = identityData.ProfileData.EmailVerified,
                Name = identityData.ProfileData.Name,
                Username = identityData.ProfileData.Username,
                GivenName = identityData.ProfileData.GivenName,
                FamilyName = identityData.ProfileData.FamilyName,
                PhoneNumber = identityData.ProfileData.PhoneNumber,
                PhoneVerified = identityData.ProfileData.PhoneVerified,
                CreatedBy = "Auth0Webhook"
            };

            var identity = new Auth0Identity
            {
                Provider = identityData.Provider,
                IsSocial = identityData.IsSocial,
                Connection = identityData.Connection,
                UserId = identityData.UserId,
                Auth0UserId = userData.UserId,
                ProfileData = profileData,
                CreatedBy = "Auth0Webhook"
            };

            existingUser.Identities.Add(identity);
        }
    }
}

public class Auth0UserData
{
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("email_verified")]
    public bool EmailVerified { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("phone_number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [JsonPropertyName("phone_verified")]
    public bool PhoneVerified { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("identities")]
    public List<Auth0Identity> Identities { get; set; } = new();

    [JsonPropertyName("app_metadata")]
    public Dictionary<string, object> AppMetadata { get; set; } = new();

    [JsonPropertyName("user_metadata")]
    public Dictionary<string, object> UserMetadata { get; set; } = new();

    [JsonPropertyName("picture")]
    public string Picture { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("nickname")]
    public string Nickname { get; set; } = string.Empty;

    [JsonPropertyName("multifactor")]
    public List<string> Multifactor { get; set; } = new();

    [JsonPropertyName("last_ip")]
    public string LastIp { get; set; } = string.Empty;

    [JsonPropertyName("last_login")]
    public DateTime? LastLogin { get; set; }

    [JsonPropertyName("logins_count")]
    public int LoginsCount { get; set; }

    [JsonPropertyName("blocked")]
    public bool Blocked { get; set; }

    [JsonPropertyName("given_name")]
    public string GivenName { get; set; } = string.Empty;

    [JsonPropertyName("family_name")]
    public string FamilyName { get; set; } = string.Empty;
}

public class Auth0Identity
{
    [JsonPropertyName("provider")]
    public string Provider { get; set; } = string.Empty;

    [JsonPropertyName("isSocial")]
    public bool IsSocial { get; set; }

    [JsonPropertyName("connection")]
    public string Connection { get; set; } = string.Empty;

    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("profileData")]
    public Auth0ProfileData ProfileData { get; set; } = new();
}

public class Auth0ProfileData
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("email_verified")]
    public bool EmailVerified { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("given_name")]
    public string GivenName { get; set; } = string.Empty;

    [JsonPropertyName("family_name")]
    public string FamilyName { get; set; } = string.Empty;

    [JsonPropertyName("phone_number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [JsonPropertyName("phone_verified")]
    public bool PhoneVerified { get; set; }
}