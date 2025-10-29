using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using CrewManagerData;
using CrewManagerData.Models;

namespace CrewManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SchedulesController : ControllerBase
{
    private readonly CMDBContext _context;

    public SchedulesController(CMDBContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(Policy = "Auth0")]
    public async Task<IActionResult> GetAllSchedules([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            var schedules = await _context.Schedules
                .Include(s => s.Boat)
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalCount = await _context.Schedules.Where(s => !s.IsDeleted).CountAsync();

            return Ok(new
            {
                schedules,
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
    public async Task<IActionResult> GetSchedule(int id)
    {
        try
        {
            var schedule = await _context.Schedules
                .Include(s => s.Boat)
                .Include(s => s.Events.Where(e => !e.IsDeleted))
                .Where(s => s.Id == id && !s.IsDeleted)
                .FirstOrDefaultAsync();

            if (schedule == null)
            {
                return NotFound(new { message = "Schedule not found" });
            }

            return Ok(schedule);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Policy = "Auth0")]
    public async Task<IActionResult> CreateSchedule([FromBody] CreateScheduleRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate that the boat exists
            var boatExists = await _context.Boats.AnyAsync(b => b.Id == request.BoatId && !b.IsDeleted);
            if (!boatExists)
            {
                return BadRequest(new { message = "Invalid BoatId. Boat does not exist." });
            }

            var userId = User.Identity?.Name ?? "Unknown";

            var schedule = new Schedule
            {
                Name = request.Name,
                Description = request.Description ?? string.Empty,
                BoatId = request.BoatId,
                CreatedBy = userId
            };

            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSchedule), new { id = schedule.Id }, schedule);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "Auth0")]
    public async Task<IActionResult> UpdateSchedule(int id, [FromBody] UpdateScheduleRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var schedule = await _context.Schedules
                .Where(s => s.Id == id && !s.IsDeleted)
                .FirstOrDefaultAsync();

            if (schedule == null)
            {
                return NotFound(new { message = "Schedule not found" });
            }

            // Validate that the boat exists
            var boatExists = await _context.Boats.AnyAsync(b => b.Id == request.BoatId && !b.IsDeleted);
            if (!boatExists)
            {
                return BadRequest(new { message = "Invalid BoatId. Boat does not exist." });
            }

            var userId = User.Identity?.Name ?? "Unknown";

            schedule.Name = request.Name;
            schedule.Description = request.Description ?? string.Empty;
            schedule.BoatId = request.BoatId;
            schedule.UpdatedAt = DateTime.UtcNow;
            schedule.UpdatedBy = userId;

            await _context.SaveChangesAsync();

            return Ok(schedule);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "Auth0")]
    public async Task<IActionResult> DeleteSchedule(int id)
    {
        try
        {
            var schedule = await _context.Schedules
                .Include(s => s.Events.Where(e => !e.IsDeleted))
                .Where(s => s.Id == id && !s.IsDeleted)
                .FirstOrDefaultAsync();

            if (schedule == null)
            {
                return NotFound(new { message = "Schedule not found" });
            }

            // Check if schedule has events
            if (schedule.Events.Any())
            {
                return BadRequest(new { message = "Cannot delete schedule that has events. Delete or reassign events first." });
            }

            var userId = User.Identity?.Name ?? "Unknown";

            // Soft delete
            schedule.IsDeleted = true;
            schedule.DeletedAt = DateTime.UtcNow;
            schedule.DeletedBy = userId;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Schedule deleted successfully", id });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }
}

public class CreateScheduleRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Required]
    public int BoatId { get; set; }
}

public class UpdateScheduleRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Required]
    public int BoatId { get; set; }
}