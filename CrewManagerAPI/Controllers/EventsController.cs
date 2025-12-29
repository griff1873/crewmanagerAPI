using System.ComponentModel.DataAnnotations;
using CrewManagerData;
using CrewManagerData.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrewManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly CMDBContext _context;

    public EventsController(CMDBContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(Policy = "Auth0")]
    public async Task<IActionResult> GetAllEvents([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            var events = await _context.Events
                .Include(e => e.Boat)
                .Include(e => e.EventType)
                .Where(e => !e.IsDeleted)
                .OrderBy(e => e.StartDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalCount = await _context.Events.Where(e => !e.IsDeleted).CountAsync();

            return Ok(new
            {
                events,
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
    public async Task<IActionResult> GetEvent(int id)
    {
        try
        {
            var eventItem = await _context.Events
                .Include(e => e.Boat)
                .Include(e => e.EventType)
                .Where(e => e.Id == id && !e.IsDeleted)
                .FirstOrDefaultAsync();

            if (eventItem == null)
            {
                return NotFound(new { message = "Event not found" });
            }

            return Ok(eventItem);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Policy = "Auth0")]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.Identity?.Name ?? "Unknown";

            // Validate that the boat exists
            var boatExists = await _context.Boats.AnyAsync(b => b.Id == request.BoatId && !b.IsDeleted);
            if (!boatExists)
            {
                return BadRequest(new { message = "Invalid BoatId. Boat does not exist." });
            }

            // Validate that the event type exists
            var eventTypeExists = await _context.EventTypes.AnyAsync(et => et.Id == request.EventTypeId && !et.IsDeleted);
            if (!eventTypeExists)
            {
                return BadRequest(new { message = "Invalid EventTypeId. Event type does not exist." });
            }

            var eventItem = new Event
            {
                Name = request.Name,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Location = request.Location,
                Description = request.Description ?? string.Empty,
                MinCrew = request.MinCrew,
                MaxCrew = request.MaxCrew,
                DesiredCrew = request.DesiredCrew,
                BoatId = request.BoatId,
                EventTypeId = request.EventTypeId,
                CreatedBy = userId
            };

            _context.Events.Add(eventItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEvent), new { id = eventItem.Id }, eventItem);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "Auth0")]
    public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var eventItem = await _context.Events
                .Where(e => e.Id == id && !e.IsDeleted)
                .FirstOrDefaultAsync();

            if (eventItem == null)
            {
                return NotFound(new { message = "Event not found" });
            }

            // Validate that the boat exists
            var boatExists = await _context.Boats.AnyAsync(b => b.Id == request.BoatId && !b.IsDeleted);
            if (!boatExists)
            {
                return BadRequest(new { message = "Invalid BoatId. Boat does not exist." });
            }

            // Validate that the event type exists
            var eventTypeExists = await _context.EventTypes.AnyAsync(et => et.Id == request.EventTypeId && !et.IsDeleted);
            if (!eventTypeExists)
            {
                return BadRequest(new { message = "Invalid EventTypeId. Event type does not exist." });
            }

            var userId = User.Identity?.Name ?? "Unknown";

            eventItem.Name = request.Name;
            eventItem.StartDate = request.StartDate;
            eventItem.EndDate = request.EndDate;
            eventItem.Location = request.Location;
            eventItem.Description = request.Description ?? string.Empty;
            eventItem.MinCrew = request.MinCrew;
            eventItem.MaxCrew = request.MaxCrew;
            eventItem.DesiredCrew = request.DesiredCrew;
            eventItem.BoatId = request.BoatId;
            eventItem.EventTypeId = request.EventTypeId;
            eventItem.UpdatedAt = DateTime.UtcNow;
            eventItem.UpdatedBy = userId;

            await _context.SaveChangesAsync();

            return Ok(eventItem);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "Auth0")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        try
        {
            var eventItem = await _context.Events
                .Where(e => e.Id == id && !e.IsDeleted)
                .FirstOrDefaultAsync();

            if (eventItem == null)
            {
                return NotFound(new { message = "Event not found" });
            }

            var userId = User.Identity?.Name ?? "Unknown";

            // Soft delete
            eventItem.IsDeleted = true;
            eventItem.DeletedAt = DateTime.UtcNow;
            eventItem.DeletedBy = userId;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Event deleted successfully", id });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet("upcoming")]
    [Authorize(Policy = "Auth0")]
    public async Task<IActionResult> GetUpcomingEvents([FromQuery] int days = 30, [FromQuery] int[] boatIds = null!)
    {
        try
        {
            if (boatIds == null || boatIds.Length == 0)
            {
                return BadRequest(new { message = "BoatIds parameter is required and must contain at least one boat ID." });
            }

            var cutoffDate = DateTime.UtcNow.AddDays(days);

            var events = await _context.Events
                .Include(e => e.Boat)
                .Where(e => !e.IsDeleted
                    && e.StartDate >= DateTime.UtcNow
                    && e.StartDate <= cutoffDate
                    && boatIds.Contains(e.BoatId))
                .OrderBy(e => e.StartDate)
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                    e.StartDate,
                    e.EndDate,
                    e.Location,
                    e.Description,
                    e.MinCrew,
                    e.MaxCrew,
                    e.DesiredCrew,
                    e.BoatId,
                    e.EventTypeId,
                    Boat = e.Boat, // Include Boat navigation
                    // Calculate confirmed crew
                    CrewCount = _context.CrewEvents.Count(ce => ce.EventId == e.Id && ce.Status == "In" && !ce.IsDeleted)
                })
                .ToListAsync();

            return Ok(events);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet("search")]
    [Authorize(Policy = "Auth0")]
    public async Task<IActionResult> SearchEvents([FromQuery] string? name, [FromQuery] string? location, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            var query = _context.Events.Where(e => !e.IsDeleted);

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(e => e.Name.Contains(name));
            }

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(e => e.Location.Contains(location));
            }

            if (startDate.HasValue)
            {
                query = query.Where(e => e.StartDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => e.EndDate <= endDate.Value);
            }

            var events = await query.OrderBy(e => e.StartDate).ToListAsync();

            return Ok(events);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet("my-events")]
    [Authorize(Policy = "Auth0")]
    public async Task<IActionResult> GetMyEvents([FromQuery] int profileId, [FromQuery] bool includePast = false)
    {
        try
        {
            // 1. Get boats where user is Owner OR Active/Invited Crew
            // Owner
            var ownedBoatIds = await _context.Boats
                .Where(b => b.ProfileId == profileId && !b.IsDeleted)
                .Select(b => b.Id)
                .ToListAsync();

            // Crew (Active or Invited)
            var crewBoatIds = await _context.BoatCrews
                .Where(bc => bc.ProfileId == profileId && !bc.IsDeleted && (bc.Status == "A" || bc.Status == "I"))
                .Select(bc => bc.BoatId)
                .ToListAsync();

            var allBoatIds = ownedBoatIds.Union(crewBoatIds).Distinct().ToList();

            if (!allBoatIds.Any())
            {
                return Ok(new List<object>());
            }

            // 2. Fetch Events
            var query = _context.Events
                .Include(e => e.Boat)
                .Where(e => !e.IsDeleted && allBoatIds.Contains(e.BoatId));

            if (!includePast)
            {
                // Show upcoming and recent (e.g. today)
                query = query.Where(e => e.EndDate >= DateTime.UtcNow.AddHours(-12)); // lenient cutoff
            }

            var events = await query
                .OrderBy(e => e.StartDate)
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                    e.StartDate,
                    e.EndDate,
                    e.Location,
                    e.Description,
                    e.MinCrew,
                    e.MaxCrew,
                    e.DesiredCrew,
                    e.BoatId,
                    e.EventTypeId,
                    Boat = e.Boat,
                    // Get user's specific status for this event
                    MyStatus = _context.CrewEvents
                        .Where(ce => ce.EventId == e.Id && ce.ProfileId == profileId && !ce.IsDeleted)
                        .Select(ce => ce.Status)
                        .FirstOrDefault() ?? "Pending", // Default to Pending if no record
                    // Calculate confirmed crew
                    CrewCount = _context.CrewEvents.Count(ce => ce.EventId == e.Id && ce.Status == "In" && !ce.IsDeleted),
                    // Resolve Display Color: Crew preference -> Boat default
                    CalendarColor = _context.BoatCrews
                        .Where(bc => bc.BoatId == e.BoatId && bc.ProfileId == profileId && !bc.IsDeleted)
                        .Select(bc => bc.CalendarColor)
                        .FirstOrDefault() ?? e.Boat.CalendarColor
                })
                .ToListAsync();

            return Ok(events);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }
}

public class CreateEventRequest
{
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MinCrew { get; set; }
    public int MaxCrew { get; set; }
    public int DesiredCrew { get; set; }
    [Required]
    public int BoatId { get; set; }
    [Required]
    public int EventTypeId { get; set; }
}

public class UpdateEventRequest
{
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MinCrew { get; set; }
    public int MaxCrew { get; set; }
    public int DesiredCrew { get; set; }
    [Required]
    public int BoatId { get; set; }
    [Required]
    public int EventTypeId { get; set; }
}