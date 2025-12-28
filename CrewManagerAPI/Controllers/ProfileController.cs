using CrewManagerData;
using CrewManagerData.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrewManagerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : CMControllerBase
    {
        public ProfileController(CMDBContext context) : base(context) { }

        // GET: api/Profile
        [HttpGet]
        [Authorize(Policy = "Auth0")]
        public async Task<ActionResult<IEnumerable<Profile>>> GetProfiles([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var profiles = await _context.Profiles
                .Where(p => !p.IsDeleted)
                .Include(p => p.Boats)
                .Include(p => p.BoatCrews)
                    .ThenInclude(bc => bc.Boat)
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalCount = await _context.Profiles.Where(p => !p.IsDeleted).CountAsync();

            return Ok(new
            {
                profiles,
                pagination = new
                {
                    page,
                    pageSize,
                    totalCount,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                }
            });
        }

        // GET: api/Profile/5
        [HttpGet("{id}")]
        [Authorize(Policy = "Auth0")]
        public async Task<ActionResult<Profile>> GetProfile(int id)
        {
            var profile = await _context.Profiles
                .Where(p => p.Id == id && !p.IsDeleted)
                .Include(p => p.Boats)
                .Include(p => p.BoatCrews)
                    .ThenInclude(bc => bc.Boat)
                .FirstOrDefaultAsync();

            if (profile == null)
            {
                return NotFound();
            }

            return profile;
        }

        // GET: api/Profile/search/by-email?email=example@email.com
        [HttpGet("search/by-email")]
        [Authorize(Policy = "Auth0")]
        public async Task<ActionResult<IEnumerable<Profile>>> GetProfilesByEmail([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email parameter is required");
            }

            var profiles = await _context.Profiles
                .Where(p => p.Email.ToLower().Contains(email.ToLower()) && !p.IsDeleted)
                .Include(p => p.Boats)
                .Include(p => p.BoatCrews)
                    .ThenInclude(bc => bc.Boat)
                .ToListAsync();

            return Ok(profiles);
        }

        // GET: api/Profile/search?query=...
        [HttpGet("search")]
        [Authorize(Policy = "Auth0")]
        public async Task<ActionResult<IEnumerable<Profile>>> SearchProfiles([FromQuery] string query)
        {
            if (string.IsNullOrEmpty(query)) return BadRequest("Query parameter required");

            var profiles = await _context.Profiles
                .Where(p => !p.IsDeleted && (p.Name.ToLower().Contains(query.ToLower()) || p.Email.ToLower().Contains(query.ToLower())))
                .OrderBy(p => p.Name)
                .Take(20)
                .ToListAsync();

            return Ok(profiles);
        }

        // GET: api/Profile/search/exact-email?email=example@email.com
        [HttpGet("search/exact-email")]
        [Authorize(Policy = "Auth0")]
        public async Task<ActionResult<Profile>> GetProfileByExactEmail([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email parameter is required");
            }

            var profile = await _context.Profiles
                .Where(p => p.Email.ToLower() == email.ToLower() && !p.IsDeleted)
                .Include(p => p.Boats)
                .Include(p => p.BoatCrews)
                    .ThenInclude(bc => bc.Boat)
                .FirstOrDefaultAsync();

            if (profile == null)
            {
                return NotFound($"No profile found with email: {email}");
            }

            return Ok(profile);
        }

        // GET: api/Profile/by-email/{email}
        [HttpGet("by-email/{email}")]
        [Authorize(Policy = "Auth0")]
        public async Task<ActionResult<Profile>> GetProfileDataByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email parameter is required");
            }

            var profile = await _context.Profiles
                .Where(p => p.Email.ToLower() == email.ToLower() && !p.IsDeleted)
                .Select(p => new Profile
                {
                    Id = p.Id,
                    LoginId = p.LoginId,
                    Name = p.Name,
                    Email = p.Email,
                    Phone = p.Phone,
                    Address = p.Address,
                    City = p.City,
                    State = p.State,
                    Zip = p.Zip,
                    Image = p.Image,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    CreatedBy = p.CreatedBy,
                    UpdatedBy = p.UpdatedBy,
                    IsDeleted = p.IsDeleted,
                    DeletedAt = p.DeletedAt,
                    DeletedBy = p.DeletedBy
                })
                .FirstOrDefaultAsync();

            if (profile == null)
            {
                return NotFound($"No profile found with email: {email}");
            }

            return Ok(profile);
        }

        // PUT: api/Profile/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> Putprofile(int id, Profile profile)
        {
            if (id != profile.Id)
            {
                return BadRequest();
            }

            _context.Entry(profile).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!profileExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Profile
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Profile>> Postprofile(Profile profile)
        {
            _context.Profiles.Add(profile);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Getprofile", new { id = profile.Id }, profile);
        }

        // DELETE: api/Profile/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deleteprofile(int id)
        {
            var profile = await _context.Profiles.FindAsync(id);
            if (profile == null)
            {
                return NotFound();
            }

            _context.Profiles.Remove(profile);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool profileExists(int id)
        {
            return _context.Profiles.Any(e => e.Id == id);
        }
    }
}
