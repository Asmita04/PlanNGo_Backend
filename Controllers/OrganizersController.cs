using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanNGo_Backend.Dto;
using PlanNGo_Backend.Model;
using System.Security.Claims;
using WebAppApi13.Data;

namespace PlanNGo_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrganizersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Organizers
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<IEnumerable<OrganizerProfileDto>>> GetOrganizers()
        {
            var organizers = await _context.Organizers
                .Include(o => o.User)
                .Select(o => new OrganizerProfileDto
                {
                    OrganizerId = o.OrganizerId,
                    Bio = o.Bio,
                    IsVerified = o.IsVerified,
                    Revenue = o.Revenue,
                    Organization = o.Organization,
                    Name = o.User.Name,
                    Email = o.User.Email,
                    Phone = o.User.Phone,
                    Address = o.User.Address,
                    Pfp = o.User.Pfp
                }).ToListAsync();

            return Ok(organizers);
        }

        // GET: api/Organizers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<OrganizerProfileDto>> GetOrganizer(int id)
        {
            var organizer = await _context.Organizers
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrganizerId == id);

            if (organizer == null)
            {
                return NotFound();
            }

            var organizerDto = new OrganizerProfileDto
            {
                OrganizerId = organizer.OrganizerId,
                Bio = organizer.Bio,
                IsVerified = organizer.IsVerified,
                Revenue = organizer.Revenue,
                Organization = organizer.Organization,
                Name = organizer.User.Name,
                Email = organizer.User.Email,
                Phone = organizer.User.Phone,
                Address = organizer.User.Address,
                Pfp = organizer.User.Pfp
            };

            return Ok(organizerDto);
        }

        // GET: api/Organizers/profile
        [HttpGet("profile")]
        [Authorize(Roles = "organizer")]
        public async Task<ActionResult<OrganizerProfileDto>> GetMyProfile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var organizer = await _context.Organizers
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (organizer == null)
            {
                return NotFound();
            }

            var organizerDto = new OrganizerProfileDto
            {
                OrganizerId = organizer.OrganizerId,
                Bio = organizer.Bio,
                IsVerified = organizer.IsVerified,
                Revenue = organizer.Revenue,
                Organization = organizer.Organization,
                Name = organizer.User.Name,
                Email = organizer.User.Email,
                Phone = organizer.User.Phone,
                Address = organizer.User.Address,
                Pfp = organizer.User.Pfp
            };

            return Ok(organizerDto);
        }

        // PUT: api/Organizers/profile
        [HttpPut("profile")]
        [Authorize(Roles = "organizer")]
        public async Task<IActionResult> UpdateProfile(UpdateOrganizerProfileDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var organizer = await _context.Organizers
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (organizer == null)
            {
                return NotFound();
            }

            organizer.Bio = dto.Bio;
            organizer.Organization = dto.Organization;
            organizer.User.Phone = dto.Phone;
            organizer.User.Address = dto.Address;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST: api/Organizers/approve
        [HttpPost("approve")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ApproveOrganizer(ApproveOrganizerDto dto)
        {
            var organizer = await _context.Organizers.FindAsync(dto.OrganizerId);
            if (organizer == null)
            {
                return NotFound();
            }

            organizer.IsVerified = dto.IsApproved;
            await _context.SaveChangesAsync();

            return Ok(new { message = dto.IsApproved ? "Organizer approved" : "Organizer rejected" });
        }

        // GET: api/Organizers/dashboard
        [HttpGet("dashboard")]
        [Authorize(Roles = "organizer")]
        public async Task<ActionResult<OrganizerDashboardDto>> GetDashboard()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var organizer = await _context.Organizers.FirstOrDefaultAsync(o => o.UserId == userId);

            if (organizer == null)
            {
                return NotFound();
            }

            var events = await _context.Events
                .Include(e => e.Tickets)
                .Where(e => e.OrganizerId == organizer.OrganizerId)
                .ToListAsync();

            var totalRevenue = events.SelectMany(e => e.Tickets ?? new List<Ticket>())
                .Where(t => t.TicketStatus == "confirmed")
                .Sum(t => t.Price * t.Count);

            var totalTicketsSold = events.SelectMany(e => e.Tickets ?? new List<Ticket>())
                .Where(t => t.TicketStatus == "confirmed")
                .Sum(t => t.Count);

            // Update organizer revenue
            organizer.Revenue = totalRevenue;
            await _context.SaveChangesAsync();

            var dashboard = new OrganizerDashboardDto
            {
                TotalEvents = events.Count,
                ApprovedEvents = events.Count(e => e.IsApproved),
                PendingEvents = events.Count(e => !e.IsApproved),
                TotalRevenue = totalRevenue,
                TotalTicketsSold = totalTicketsSold,
                EventStats = events.Select(e => new EventStatsDto
                {
                    EventId = e.EventId,
                    Title = e.Title,
                    TicketsSold = e.Tickets?.Where(t => t.TicketStatus == "confirmed").Sum(t => t.Count) ?? 0,
                    Revenue = e.Tickets?.Where(t => t.TicketStatus == "confirmed").Sum(t => t.Price * t.Count) ?? 0,
                    IsApproved = e.IsApproved
                }).ToList(),
                MonthlyRevenue = events
                    .SelectMany(e => e.Tickets ?? new List<Ticket>())
                    .Where(t => t.TicketStatus == "confirmed")
                    .GroupBy(t => new { t.CreatedAt.Year, t.CreatedAt.Month })
                    .Select(g => new MonthlyRevenueDto
                    {
                        Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                        Revenue = g.Sum(t => t.Price * t.Count),
                        EventCount = g.Select(t => t.EventId).Distinct().Count()
                    }).ToList()
            };

            return Ok(dashboard);
        }
    }
}