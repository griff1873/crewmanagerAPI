using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using CrewManagerData;
using CrewManagerData.Models;

namespace CrewManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BoatsController : ControllerBase
{
    private readonly CMDBContext _context;

    public BoatsController(CMDBContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(Policy = "Auth0")]
    public async Task<IActionResult> GetAllBoats([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            var boats = await _context.Boats
                .Include(b => b.Profile)
                .Where(b => !b.IsDeleted)
                .OrderBy(b => b.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalCount = await _context.Boats.Where(b => !b.IsDeleted).CountAsync();

            return Ok(new
            {
                boats,
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

    [HttpGet("{id}")]
    [Authorize(Policy = "Auth0")]
    public async Task<IActionResult> GetBoat(int id)
    {
        try
        {
            var boat = await _context.Boats
                .Include(b => b.Profile)
                .Where(b => b.Id == id && !b.IsDeleted)
                .FirstOrDefaultAsync();

            if (boat == null)
            {
                return NotFound(new { message = "Boat not found" });
            }

            return Ok(boat);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet("by-profile/{profileId}")]
    [Authorize(Policy = "Auth0")]
    public async Task<IActionResult> GetBoatsByProfile(int profileId)
    {
        try
        {
            var boats = await _context.Boats
                .Include(b => b.Profile)
                .Where(b => b.ProfileId == profileId && !b.IsDeleted)
                .OrderBy(b => b.Name)
                .ToListAsync();

            return Ok(boats);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Policy = "Auth0")]
    public async Task<IActionResult> CreateBoat([FromBody] CreateBoatRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate that the profile exists
            var profileExists = await _context.Profiles.AnyAsync(p => p.Id == request.ProfileId && !p.IsDeleted);
            if (!profileExists)
            {
                return BadRequest(new { message = "Invalid ProfileId. Profile does not exist." });
            }

            var userId = User.Identity?.Name ?? "Unknown";

            var boat = new Boat
            {
                Name = request.Name,
                Description = request.Description ?? string.Empty,
                ProfileId = request.ProfileId,
                CreatedBy = userId
            };

            _context.Boats.Add(boat);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBoat), new { id = boat.Id }, boat);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "Auth0")]
    public async Task<IActionResult> UpdateBoat(int id, [FromBody] UpdateBoatRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var boat = await _context.Boats
                .Where(b => b.Id == id && !b.IsDeleted)
                .FirstOrDefaultAsync();

            if (boat == null)
            {
                return NotFound(new { message = "Boat not found" });
            }

            // Validate that the profile exists
            var profileExists = await _context.Profiles.AnyAsync(p => p.Id == request.ProfileId && !p.IsDeleted);
            if (!profileExists)
            {
                return BadRequest(new { message = "Invalid ProfileId. Profile does not exist." });
            }

            var userId = User.Identity?.Name ?? "Unknown";

            boat.Name = request.Name;
            boat.Description = request.Description ?? string.Empty;
            boat.ProfileId = request.ProfileId;
            boat.UpdatedAt = DateTime.UtcNow;
            boat.UpdatedBy = userId;

            await _context.SaveChangesAsync();

            return Ok(boat);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "Auth0")]
    public async Task<IActionResult> DeleteBoat(int id)
    {
        try
        {
            var boat = await _context.Boats
                .Where(b => b.Id == id && !b.IsDeleted)
                .FirstOrDefaultAsync();

            if (boat == null)
            {
                return NotFound(new { message = "Boat not found" });
            }

            var userId = User.Identity?.Name ?? "Unknown";

            // Soft delete
            boat.IsDeleted = true;
            boat.DeletedAt = DateTime.UtcNow;
            boat.DeletedBy = userId;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Boat deleted successfully", id });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet("{id}/schedules")]
    [Authorize(Policy = "Auth0")]
    public async Task<IActionResult> GetBoatSchedules(int id)
    {
        try
        {
            var boat = await _context.Boats
                .Include(b => b.Schedules.Where(s => !s.IsDeleted))
                .ThenInclude(s => s.Events.Where(e => !e.IsDeleted))
                .Where(b => b.Id == id && !b.IsDeleted)
                .FirstOrDefaultAsync();

            if (boat == null)
            {
                return NotFound(new { message = "Boat not found" });
            }

            return Ok(boat.Schedules);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet("search")]
    [Authorize(Policy = "Auth0")]
    public async Task<IActionResult> SearchBoats([FromQuery] string? name, [FromQuery] int? profileId)
    {
        try
        {
            var query = _context.Boats
                .Include(b => b.Profile)
                .Where(b => !b.IsDeleted);

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(b => b.Name.Contains(name));
            }

            if (profileId.HasValue)
            {
                query = query.Where(b => b.ProfileId == profileId.Value);
            }

            var boats = await query.OrderBy(b => b.Name).ToListAsync();

            return Ok(boats);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }
}

public class CreateBoatRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Required]
    public int ProfileId { get; set; }
}

public class UpdateBoatRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Required]
    public int ProfileId { get; set; }
}