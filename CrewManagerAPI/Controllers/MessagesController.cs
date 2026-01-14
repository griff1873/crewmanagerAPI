using System.ComponentModel.DataAnnotations;
using CrewManagerData;
using CrewManagerData.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrewManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "Auth0")]
public class MessagesController : ControllerBase
{
    private readonly CMDBContext _context;

    public MessagesController(CMDBContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var sender = await _context.Profiles.FindAsync(request.SenderProfileId);
            if (sender == null) return BadRequest("Invalid Sender Profile ID");

            var message = new Message
            {
                SenderProfileId = request.SenderProfileId,
                Subject = request.Subject,
                Body = request.Body,
                Channel = request.Channel,
                TargetEventId = request.TargetEventId,
                RootMessageId = null, // Will be set to ID after save
                ParentMessageId = null
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Set RootMessageId to itself for new threads
            message.RootMessageId = message.Id;

            // Add recipients
            foreach (var recipientId in request.RecipientProfileIds)
            {
                // Validate recipient exists
                var recipientExists = await _context.Profiles.AnyAsync(p => p.Id == recipientId && !p.IsDeleted);
                if (recipientExists)
                {
                    _context.MessageRecipients.Add(new MessageRecipient
                    {
                        MessageId = message.Id,
                        RecipientProfileId = recipientId,
                        Status = "Sent",
                        IsRead = false
                    });
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { messageId = message.Id, message = "Message sent successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpPost("{id}/reply")]
    public async Task<IActionResult> ReplyToMessage(int id, [FromBody] ReplyMessageRequest request)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var parentMessage = await _context.Messages.FindAsync(id);
            if (parentMessage == null) return NotFound("Parent message not found");

            var reply = new Message
            {
                SenderProfileId = request.SenderProfileId,
                Subject = $"Re: {parentMessage.Subject}",
                Body = request.Body,
                Channel = request.Channel,
                ParentMessageId = id,
                RootMessageId = parentMessage.RootMessageId ?? parentMessage.Id, // Ensure root is propagated
                TargetEventId = parentMessage.TargetEventId
            };

            _context.Messages.Add(reply);
            await _context.SaveChangesAsync();

            // Add recipients (Default to replying to the sender of the parent message)
            // In a real app, you might want "Reply All" logic here
            _context.MessageRecipients.Add(new MessageRecipient
            {
                MessageId = reply.Id,
                RecipientProfileId = parentMessage.SenderProfileId,
                Status = "Sent",
                IsRead = false
            });

            await _context.SaveChangesAsync();

            return Ok(new { messageId = reply.Id, message = "Reply sent successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetMyMessages([FromQuery] int profileId, [FromQuery] string box = "inbox")
    {
        try
        {
            if (box.ToLower() == "sent")
            {
                var messages = await _context.Messages
                    .Include(m => m.Recipients).ThenInclude(r => r.Recipient)
                    .Include(m => m.Sender)
                    .Where(m => m.SenderProfileId == profileId && !m.IsDeleted)
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => new
                    {
                        m.Id,
                        m.Subject,
                        m.Body,
                        m.CreatedAt,
                        Sender = new { m.Sender.Name, m.Sender.Id },
                        Recipients = m.Recipients.Select(r => new { r.Recipient.Name, r.Recipient.Id }).ToList(),
                        IsRead = true,
                        Type = "Sent"
                    })
                    .ToListAsync();
                return Ok(messages);
            }
            else if (box.ToLower() == "inbox")
            {
                // Inbox
                var messages = await _context.MessageRecipients
                    .Include(mr => mr.Message).ThenInclude(m => m.Sender)
                    .Include(mr => mr.Message).ThenInclude(m => m.Recipients).ThenInclude(r => r.Recipient)
                    .Where(mr => mr.RecipientProfileId == profileId && !mr.IsDeleted)
                    .OrderByDescending(mr => mr.CreatedAt)
                    .Select(mr => new
                    {
                        mr.Message.Id,
                        mr.Message.Subject,
                        mr.Message.Body,
                        mr.Message.CreatedAt,
                        Sender = new { mr.Message.Sender.Name, mr.Message.Sender.Id },
                        Recipients = mr.Message.Recipients.Select(r => new { r.Recipient.Name, r.Recipient.Id }).ToList(),
                        mr.IsRead,
                        Type = "Received"
                    })
                    .ToListAsync();
                return Ok(messages);
            }
            else
            {
                // All (Unified)
                // Fetch relevant messages first
                var messages = await _context.Messages
                    .Include(m => m.Sender)
                    .Include(m => m.Recipients).ThenInclude(r => r.Recipient)
                    .Where(m =>
                        (m.SenderProfileId == profileId && !m.IsDeleted) ||
                        m.Recipients.Any(r => r.RecipientProfileId == profileId && !r.IsDeleted)
                    )
                    .OrderByDescending(m => m.CreatedAt)
                    .ToListAsync();

                // Group by RootMessageId (or Id if null) to get unique threads and take the latest message
                var threadMessages = messages
                    .GroupBy(m => m.RootMessageId ?? m.Id)
                    .Select(g => g.OrderByDescending(m => m.CreatedAt).First())
                    .OrderByDescending(m => m.CreatedAt) // Re-sort the final list
                    .ToList();

                // Map results in memory to handle the conditional IsRead logic easily
                var result = threadMessages.Select(m =>
                {
                    var isSender = m.SenderProfileId == profileId;
                    var recipientEntry = m.Recipients.FirstOrDefault(r => r.RecipientProfileId == profileId);

                    return new
                    {
                        m.Id,
                        m.Subject,
                        m.Body,
                        m.CreatedAt,
                        Sender = new { m.Sender.Name, m.Sender.Id },
                        Recipients = m.Recipients.Select(r => new { r.Recipient.Name, r.Recipient.Id }).ToList(),
                        IsRead = isSender ? true : (recipientEntry?.IsRead ?? true), // Sent is read, Recipient uses IsRead status
                        Type = isSender ? "Sent" : "Received"
                    };
                });

                return Ok(result);
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMessageDetails(int id, [FromQuery] int profileId)
    {
        try
        {
            var message = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Recipients).ThenInclude(r => r.Recipient)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (message == null) return NotFound("Message not found");

            // Check permission: Sender or Recipient
            var isSender = message.SenderProfileId == profileId;
            var isRecipient = message.Recipients.Any(r => r.RecipientProfileId == profileId);

            if (!isSender && !isRecipient)
            {
                return Forbid();
            }

            // If getting details and user is recipient, mark as read
            if (isRecipient)
            {
                var recipientEntry = await _context.MessageRecipients
                    .FirstOrDefaultAsync(r => r.MessageId == id && r.RecipientProfileId == profileId);
                if (recipientEntry != null && !recipientEntry.IsRead)
                {
                    recipientEntry.IsRead = true;
                    // recipientEntry.ReadAt = DateTime.UtcNow; // If we add this field later
                    await _context.SaveChangesAsync();
                }
            }

            // Fetch thread if desired
            var thread = await _context.Messages
                .Where(m => m.RootMessageId == (message.RootMessageId ?? message.Id) || m.Id == (message.RootMessageId ?? message.Id))
                .OrderBy(m => m.CreatedAt)
                .Select(m => new
                {
                    m.Id,
                    m.Body,
                    m.CreatedAt,
                    SenderName = m.Sender.Name,
                    SenderId = m.Sender.Id
                })
                .ToListAsync();

            return Ok(new
            {
                message.Id,
                message.Subject,
                message.Body,
                message.CreatedAt,
                Sender = new { message.Sender.Name, message.Sender.Id },
                Recipients = message.Recipients
                    .Where(r => r.Recipient != null)
                    .Select(r => new { r.Recipient.Name, r.Recipient.Id })
                    .ToList(),
                Thread = thread
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id, [FromBody] MarkReadRequest request)
    {
        try
        {
            var recipientEntry = await _context.MessageRecipients
                .FirstOrDefaultAsync(r => r.MessageId == id && r.RecipientProfileId == request.ProfileId);

            if (recipientEntry == null) return NotFound("Recipient entry not found");

            recipientEntry.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet("unread-count/{profileId}")]
    public async Task<IActionResult> GetUnreadCount(int profileId)
    {
        try
        {
            var count = await _context.MessageRecipients
                .CountAsync(mr => mr.RecipientProfileId == profileId && !mr.IsRead && !mr.IsDeleted);
            return Ok(new { count });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }
    [HttpGet("recipients")]
    public async Task<IActionResult> GetAvailableRecipients([FromQuery] int profileId)
    {
        try
        {
            // Logic: Get all boats where the user is a member (Crew or Owner).
            // Then get all OTHER members of those boats.

            // 1. Get IDs of boats where user is a member/owner
            var myBoatIds = await _context.BoatCrews
                .Where(bc => bc.ProfileId == profileId && bc.Status == "A" && !bc.IsDeleted) // Accepted crew
                .Select(bc => bc.BoatId)
                .Union(
                    _context.Boats
                        .Where(b => b.ProfileId == profileId && !b.IsDeleted) // Owner
                        .Select(b => b.Id)
                )
                .Distinct()
                .ToListAsync();

            if (!myBoatIds.Any())
            {
                return Ok(new List<object>());
            }

            // 2. Get all profiles associated with these boats (Owner + Crew)
            var recipients = await _context.BoatCrews
                .Where(bc => myBoatIds.Contains(bc.BoatId) && bc.Status == "A" && !bc.IsDeleted && bc.ProfileId != profileId)
                .Include(bc => bc.Profile)
                .Select(bc => new { bc.Profile.Id, bc.Profile.Name, bc.Profile.Email, bc.Profile.Image })
                .Union(
                    _context.Boats
                        .Where(b => myBoatIds.Contains(b.Id) && !b.IsDeleted && b.ProfileId != profileId)
                        .Include(b => b.Profile)
                        .Select(b => new { b.Profile.Id, b.Profile.Name, b.Profile.Email, b.Profile.Image })
                )
                .Distinct()
                .OrderBy(p => p.Name)
                .ToListAsync();

            return Ok(recipients);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }
}

public class SendMessageRequest
{
    [Required]
    public int SenderProfileId { get; set; }
    [Required]
    public List<int> RecipientProfileIds { get; set; } = new List<int>();
    public string? Subject { get; set; }
    [Required]
    public string Body { get; set; } = string.Empty;
    public string Channel { get; set; } = "InApp";
    public int? TargetEventId { get; set; }
}

public class ReplyMessageRequest
{
    [Required]
    public int SenderProfileId { get; set; }
    [Required]
    public string Body { get; set; } = string.Empty;
    public string Channel { get; set; } = "InApp";
}

public class MarkReadRequest
{
    [Required]
    public int ProfileId { get; set; }
}
