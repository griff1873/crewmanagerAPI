using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CrewManagerData;
using CrewManagerData.Models;
using Microsoft.AspNetCore.Authorization;

namespace CrewManagerAPI.Controllers
{
    public class CreateEventTypeRequest
    {
        public int? ProfileId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EventTypesController : ControllerBase
    {
        private readonly CMDBContext _context;

        public EventTypesController(CMDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetEventTypes()
        {
            try
            {
                var eventTypes = await _context.EventTypes
                    .Where(et => !et.IsDeleted)
                    .OrderBy(et => et.Name)
                    .Select(et => new { et.Id, et.Name, et.ProfileId })
                    .ToListAsync();

                return Ok(eventTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving event types", error = ex.Message });
            }
        }

        [HttpGet("profile/{profileId}")]
        public async Task<IActionResult> GetEventTypesByProfile(int profileId)
        {
            try
            {
                var eventTypes = await _context.EventTypes
                    .Where(et => !et.IsDeleted && (et.ProfileId == null || et.ProfileId == profileId))
                    .OrderBy(et => et.Name)
                    .Select(et => new { et.Id, et.Name, et.ProfileId })
                    .ToListAsync();

                return Ok(eventTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving event types for profile", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateEventType([FromBody] CreateEventTypeRequest request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return BadRequest(new { message = "Event type name is required" });
                }

                if (request.Name.Length > 200)
                {
                    return BadRequest(new { message = "Event type name cannot exceed 200 characters" });
                }

                // Check if event type with same name already exists for this profile
                var existingEventType = await _context.EventTypes
                    .Where(et => !et.IsDeleted &&
                                et.Name.ToLower() == request.Name.ToLower() &&
                                et.ProfileId == request.ProfileId)
                    .FirstOrDefaultAsync();

                if (existingEventType != null)
                {
                    return Conflict(new { message = "An event type with this name already exists for this profile" });
                }

                // If ProfileId is provided, verify the profile exists
                if (request.ProfileId.HasValue)
                {
                    var profileExists = await _context.Profiles
                        .AnyAsync(p => p.Id == request.ProfileId.Value && !p.IsDeleted);

                    if (!profileExists)
                    {
                        return BadRequest(new { message = "Profile not found" });
                    }
                }

                // Create new event type
                var eventType = new EventType
                {
                    Name = request.Name.Trim(),
                    ProfileId = request.ProfileId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.EventTypes.Add(eventType);
                await _context.SaveChangesAsync();

                return CreatedAtAction(
                    nameof(GetEventTypes),
                    new { id = eventType.Id },
                    new { eventType.Id, eventType.Name, eventType.ProfileId }
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating event type", error = ex.Message });
            }
        }

        [HttpDelete("profile/{profileId}/eventtype/{eventTypeId}")]
        public async Task<IActionResult> DeleteEventType(int profileId, int eventTypeId)
        {
            try
            {
                // Find the event type
                var eventType = await _context.EventTypes
                    .Where(et => et.Id == eventTypeId && !et.IsDeleted)
                    .FirstOrDefaultAsync();

                if (eventType == null)
                {
                    return NotFound(new { message = "Event type not found" });
                }

                // Check if event type has null ProfileId (system-wide event types cannot be deleted)
                if (eventType.ProfileId == null)
                {
                    return BadRequest(new { message = "System-wide event types cannot be deleted" });
                }

                // Check if the event type belongs to the specified profile
                if (eventType.ProfileId != profileId)
                {
                    return BadRequest(new { message = "Event type does not belong to the specified profile" });
                }

                // Verify the profile exists
                var profileExists = await _context.Profiles
                    .AnyAsync(p => p.Id == profileId && !p.IsDeleted);

                if (!profileExists)
                {
                    return BadRequest(new { message = "Profile not found" });
                }

                // Soft delete the event type
                eventType.IsDeleted = true;
                eventType.DeletedAt = DateTime.UtcNow;
                eventType.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Event type deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting event type", error = ex.Message });
            }
        }
    }
}