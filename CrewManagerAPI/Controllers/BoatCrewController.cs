using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using CrewManagerData;
using CrewManagerData.Models;

namespace CrewManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BoatCrewController : ControllerBase
{
    private readonly CMDBContext _context;

    public BoatCrewController(CMDBContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(Policy = "Auth0")]
    public async Task<IActionResult> GetAllBoatCrew([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            var boatCrews = await _context.BoatCrews
                .Include(bc => bc.Profile)
                .Include(bc => bc.Boat)
                .Where(bc => !bc.IsDeleted)
                .OrderBy(bc => bc.Boat.Name)
                .ThenBy(bc => bc.Profile.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalCount = await _context.BoatCrews.Where(bc => !bc.IsDeleted).CountAsync();

            return Ok(new
            {
                boatCrews,
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
    public async Task<IActionResult> GetBoatCrew(int id)
    {
        try
        {
            var boatCrew = await _context.BoatCrews
                .Include(bc => bc.Profile)
                .Include(bc => bc.Boat)
                .Where(bc => bc.Id == id && !bc.IsDeleted)
                .FirstOrDefaultAsync();

            if (boatCrew == null)
            {
                return NotFound(new { message = "Boat crew assignment not found" });
            }

            return Ok(boatCrew);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet("by-boat/{boatId}")]
    [Authorize(Policy = "Auth0")]
    public async Task<IActionResult> GetCrewByBoat(int boatId)
    {
        try
        {
            var boatCrews = await _context.BoatCrews
                .Include(bc => bc.Profile)
                .Include(bc => bc.Boat)
                .Where(bc => bc.BoatId == boatId && !bc.IsDeleted)
                .OrderBy(bc => bc.Profile.Name)
                .ToListAsync();

            return Ok(boatCrews);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet("by-profile/{profileLoginId}")]
    [Authorize(Policy = "Auth0")]
    public async Task<IActionResult> GetCrewByProfile(string profileLoginId)
    {
        try
        {
            var boatCrews = await _context.BoatCrews
                .Include(bc => bc.Profile)
                .Include(bc => bc.Boat)
                .Where(bc => bc.ProfileLoginId == profileLoginId && !bc.IsDeleted)
                .OrderBy(bc => bc.Boat.Name)
                .ToListAsync();

            return Ok(boatCrews);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Policy = "Auth0")]
    public async Task<IActionResult> CreateBoatCrew([FromBody] CreateBoatCrewRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate that the profile exists
            var profileExists = await _context.Profiles.AnyAsync(p => p.LoginId == request.ProfileLoginId && !p.IsDeleted);
            if (!profileExists)
            {
                return BadRequest(new { message = "Invalid ProfileLoginId. Profile does not exist." });
            }

            // Validate that the boat exists
            var boatExists = await _context.Boats.AnyAsync(b => b.Id == request.BoatId && !b.IsDeleted);
            if (!boatExists)
            {
                return BadRequest(new { message = "Invalid BoatId. Boat does not exist." });
            }

            // Check if crew assignment already exists
            var existingCrew = await _context.BoatCrews
                .AnyAsync(bc => bc.ProfileLoginId == request.ProfileLoginId && bc.BoatId == request.BoatId && !bc.IsDeleted);
            if (existingCrew)
            {
                return BadRequest(new { message = "This profile is already assigned as crew to this boat." });
            }

            var userId = User.Identity?.Name ?? "Unknown";

            var boatCrew = new BoatCrew
            {
                ProfileLoginId = request.ProfileLoginId,
                BoatId = request.BoatId,
                IsAdmin = request.IsAdmin,
                CreatedBy = userId
            };

            _context.BoatCrews.Add(boatCrew);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBoatCrew), new { id = boatCrew.Id }, boatCrew);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "Auth0")]
    public async Task<IActionResult> UpdateBoatCrew(int id, [FromBody] UpdateBoatCrewRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var boatCrew = await _context.BoatCrews
                .Where(bc => bc.Id == id && !bc.IsDeleted)
                .FirstOrDefaultAsync();

            if (boatCrew == null)
            {
                return NotFound(new { message = "Boat crew assignment not found" });
            }

            var userId = User.Identity?.Name ?? "Unknown";

            boatCrew.IsAdmin = request.IsAdmin;
            boatCrew.UpdatedAt = DateTime.UtcNow;
            boatCrew.UpdatedBy = userId;

            await _context.SaveChangesAsync();

            return Ok(boatCrew);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "Auth0")]
    public async Task<IActionResult> DeleteBoatCrew(int id)
    {
        try
        {
            var boatCrew = await _context.BoatCrews
                .Where(bc => bc.Id == id && !bc.IsDeleted)
                .FirstOrDefaultAsync();

            if (boatCrew == null)
            {
                return NotFound(new { message = "Boat crew assignment not found" });
            }

            var userId = User.Identity?.Name ?? "Unknown";

            // Soft delete
            boatCrew.IsDeleted = true;
            boatCrew.DeletedAt = DateTime.UtcNow;
            boatCrew.DeletedBy = userId;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Boat crew assignment deleted successfully", id });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet("admins/by-boat/{boatId}")]
    [Authorize(Policy = "Auth0")]
    public async Task<IActionResult> GetBoatAdmins(int boatId)
    {
        try
        {
            var admins = await _context.BoatCrews
                .Include(bc => bc.Profile)
                .Include(bc => bc.Boat)
                .Where(bc => bc.BoatId == boatId && bc.IsAdmin && !bc.IsDeleted)
                .OrderBy(bc => bc.Profile.Name)
                .ToListAsync();

            return Ok(admins);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }
}

public class CreateBoatCrewRequest
{
    [Required]
    public string ProfileLoginId { get; set; } = string.Empty;
    [Required]
    public int BoatId { get; set; }
    public bool IsAdmin { get; set; } = false;
}

public class UpdateBoatCrewRequest
{
    public bool IsAdmin { get; set; } = false;
}