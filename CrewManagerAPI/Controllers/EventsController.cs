using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CrewManagerData;
using CrewManagerData.Models;

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
    [Authorize(AuthenticationSchemes = "ApiKey")]
    public async Task<IActionResult> GetAllEvents([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            var events = await _context.Events
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
    [Authorize(AuthenticationSchemes = "ApiKey")]
    public async Task<IActionResult> GetEvent(int id)
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

            return Ok(eventItem);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = "ApiKey")]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
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
                CreatedBy = "API"
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
    [Authorize(AuthenticationSchemes = "ApiKey")]
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

            eventItem.Name = request.Name;
            eventItem.StartDate = request.StartDate;
            eventItem.EndDate = request.EndDate;
            eventItem.Location = request.Location;
            eventItem.Description = request.Description ?? string.Empty;
            eventItem.MinCrew = request.MinCrew;
            eventItem.MaxCrew = request.MaxCrew;
            eventItem.DesiredCrew = request.DesiredCrew;
            eventItem.UpdatedAt = DateTime.UtcNow;
            eventItem.UpdatedBy = "API";

            await _context.SaveChangesAsync();

            return Ok(eventItem);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(AuthenticationSchemes = "ApiKey")]
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

            // Soft delete
            eventItem.IsDeleted = true;
            eventItem.DeletedAt = DateTime.UtcNow;
            eventItem.DeletedBy = "API";

            await _context.SaveChangesAsync();

            return Ok(new { message = "Event deleted successfully", id });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet("upcoming")]
    [Authorize(AuthenticationSchemes = "ApiKey")]
    public async Task<IActionResult> GetUpcomingEvents([FromQuery] int days = 30)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(days);

            var events = await _context.Events
                .Where(e => !e.IsDeleted && e.StartDate >= DateTime.UtcNow && e.StartDate <= cutoffDate)
                .OrderBy(e => e.StartDate)
                .ToListAsync();

            return Ok(events);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet("search")]
    [Authorize(AuthenticationSchemes = "ApiKey")]
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
}