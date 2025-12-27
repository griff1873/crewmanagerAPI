using System.ComponentModel.DataAnnotations;
using CrewManagerData;
using CrewManagerData.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrewManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CrewEventController : ControllerBase
{
    private readonly CMDBContext _context;

    public CrewEventController(CMDBContext context)
    {
        _context = context;
    }

    [HttpGet("by-event/{eventId}")]
    [Authorize(Policy = "Auth0")]
    public async Task<IActionResult> GetByEvent(int eventId)
    {
        try
        {
            var responses = await _context.CrewEvents
                .Include(ce => ce.Profile)
                .Where(ce => ce.EventId == eventId && !ce.IsDeleted)
                .OrderBy(ce => ce.Profile.Name)
                .ToListAsync();

            return Ok(responses);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet("by-profile/{profileId}")]
    [Authorize(Policy = "Auth0")]
    public async Task<IActionResult> GetByProfile(int profileId)
    {
        try
        {
            var responses = await _context.CrewEvents
                .Include(ce => ce.Event)
                .Where(ce => ce.ProfileId == profileId && !ce.IsDeleted)
                .OrderByDescending(ce => ce.Event.StartDate)
                .ToListAsync();

            return Ok(responses);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "Auth0")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var crewEvent = await _context.CrewEvents
                .Include(ce => ce.Event)
                .Include(ce => ce.Profile)
                .Where(ce => ce.Id == id && !ce.IsDeleted)
                .FirstOrDefaultAsync();

            if (crewEvent == null)
            {
                return NotFound();
            }

            return Ok(crewEvent);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Policy = "Auth0")]
    public async Task<IActionResult> Create([FromBody] CreateCrewEventRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if exists
            var existing = await _context.CrewEvents
                .Where(ce => ce.EventId == request.EventId && ce.ProfileId == request.ProfileId && !ce.IsDeleted)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                return BadRequest(new { message = "Response already exists for this event and profile." });
            }

            var userId = User.Identity?.Name ?? "Unknown";

            var crewEvent = new CrewEvent
            {
                EventId = request.EventId,
                ProfileId = request.ProfileId,
                Status = request.Status,
                CreatedBy = userId
            };

            _context.CrewEvents.Add(crewEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = crewEvent.Id }, crewEvent);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "Auth0")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCrewEventRequest request)
    {
        try
        {
            var crewEvent = await _context.CrewEvents
                .Where(ce => ce.Id == id && !ce.IsDeleted)
                .FirstOrDefaultAsync();

            if (crewEvent == null)
            {
                return NotFound();
            }

            var userId = User.Identity?.Name ?? "Unknown";

            crewEvent.Status = request.Status;
            crewEvent.UpdatedAt = DateTime.UtcNow;
            crewEvent.UpdatedBy = userId;

            await _context.SaveChangesAsync();

            return Ok(crewEvent);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "Auth0")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var crewEvent = await _context.CrewEvents
                .Where(ce => ce.Id == id && !ce.IsDeleted)
                .FirstOrDefaultAsync();

            if (crewEvent == null)
            {
                return NotFound();
            }

            var userId = User.Identity?.Name ?? "Unknown";

            crewEvent.IsDeleted = true;
            crewEvent.DeletedAt = DateTime.UtcNow;
            crewEvent.DeletedBy = userId;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }
}

public class CreateCrewEventRequest
{
    [Required]
    public int EventId { get; set; }
    [Required]
    public int ProfileId { get; set; }
    public string Status { get; set; } = "Pending";
}

public class UpdateCrewEventRequest
{
    [Required]
    public string Status { get; set; } = "Pending";
}
