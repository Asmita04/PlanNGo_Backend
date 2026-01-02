using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanNGo_Backend.Dto;
using PlanNGo_Backend.Model;
using WebAppApi13.Data;

namespace PlanNGo_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Admin/dashboard
        [HttpGet("dashboard")]
        public async Task<ActionResult<AdminDashboardDto>> GetDashboard()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalClients = await _context.Clients.CountAsync();
            var totalOrganizers = await _context.Organizers.CountAsync();
            var totalEvents = await _context.Events.CountAsync();
            var approvedEvents = await _context.Events.CountAsync(e => e.IsApproved);
            var pendingEvents = await _context.Events.CountAsync(e => !e.IsApproved);

            var totalRevenue = await _context.Tickets
                .Where(t => t.TicketStatus == "confirmed")
                .SumAsync(t => t.Price * t.Count);

            var totalTicketsSold = await _context.Tickets
                .Where(t => t.TicketStatus == "confirmed")
                .SumAsync(t => t.Count);

            var categoryStats = await _context.Events
                .Include(e => e.Tickets)
                .GroupBy(e => e.Category)
                .Select(g => new CategoryStatsDto
                {
                    Category = g.Key,
                    EventCount = g.Count(),
                    Revenue = g.SelectMany(e => e.Tickets ?? new List<Ticket>())
                        .Where(t => t.TicketStatus == "confirmed")
                        .Sum(t => t.Price * t.Count)
                })
                .ToListAsync();

            var monthlyRevenue = await _context.Tickets
                .Include(t => t.Event)
                .Where(t => t.TicketStatus == "confirmed")
                .GroupBy(t => new { t.CreatedAt.Year, t.CreatedAt.Month })
                .Select(g => new MonthlyRevenueDto
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Revenue = g.Sum(t => t.Price * t.Count),
                    EventCount = g.Select(t => t.EventId).Distinct().Count()
                })
                .OrderBy(m => m.Month)
                .ToListAsync();

            var dashboard = new AdminDashboardDto
            {
                TotalUsers = totalUsers,
                TotalClients = totalClients,
                TotalOrganizers = totalOrganizers,
                TotalEvents = totalEvents,
                ApprovedEvents = approvedEvents,
                PendingEvents = pendingEvents,
                TotalRevenue = totalRevenue,
                TotalTicketsSold = totalTicketsSold,
                CategoryStats = categoryStats,
                MonthlyRevenue = monthlyRevenue
            };

            return Ok(dashboard);
        }

        // GET: api/Admin/users
        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new UserDto
                {
                    UserId = u.UserId,
                    Name = u.Name,
                    Email = u.Email,
                    IsEmailVerified = u.IsEmailVerified,
                    Phone = u.Phone,
                    Address = u.Address,
                    Pfp = u.Pfp,
                    Role = u.Role
                })
                .OrderBy(u => u.Name)
                .ToListAsync();

            return Ok(users);
        }

        // GET: api/Admin/pending-organizers
        [HttpGet("pending-organizers")]
        public async Task<ActionResult<IEnumerable<OrganizerProfileDto>>> GetPendingOrganizers()
        {
            var organizers = await _context.Organizers
                .Include(o => o.User)
                .Where(o => o.IsVerified == null || o.IsVerified == false)
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
                })
                .ToListAsync();

            return Ok(organizers);
        }

        // GET: api/Admin/pending-events
        [HttpGet("pending-events")]
        public async Task<ActionResult<IEnumerable<EventResponseDto>>> GetPendingEvents()
        {
            var events = await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Organizer)
                .ThenInclude(o => o.User)
                .Where(e => !e.IsApproved)
                .Select(e => new EventResponseDto
                {
                    EventId = e.EventId,
                    Title = e.Title,
                    Category = e.Category,
                    EventImage = e.EventImage,
                    Description = e.Description,
                    Location = e.Location,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    IsApproved = e.IsApproved,
                    TicketPrice = e.TicketPrice,
                    AvailableTickets = e.AvailableTickets,
                    VenueName = e.Venue != null ? e.Venue.VenueName : string.Empty,
                    OrganizerName = e.Organizer != null && e.Organizer.User != null ? e.Organizer.User.Name : string.Empty,
                    OrganizerOrganization = e.Organizer != null ? (e.Organizer.Organization ?? "Independent") : "Independent"
                })
                .OrderBy(e => e.StartDate)
                .ToListAsync();

            return Ok(events);
        }

        // DELETE: api/Admin/users/5
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (user.Role == "admin")
            {
                return BadRequest("Cannot delete admin users");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Admin/revenue-summary
        [HttpGet("revenue-summary")]
        public async Task<ActionResult<RevenueSummaryDto>> GetRevenueSummary()
        {
            var totalPlatformRevenue = await _context.Tickets
                .Where(t => t.TicketStatus == "confirmed")
                .SumAsync(t => t.Price * t.Count);

            var organizerRevenues = await _context.Organizers
                .Include(o => o.User)
                .Include(o => o.Events)
                .ThenInclude(e => e.Tickets)
                .Select(o => new OrganizerRevenueDto
                {
                    OrganizerId = o.OrganizerId,
                    OrganizerName = o.User != null ? o.User.Name : string.Empty,
                    Organization = o.Organization ?? string.Empty,
                    Revenue = o.Events != null ? o.Events.SelectMany(e => e.Tickets ?? new List<Ticket>())
                        .Where(t => t.TicketStatus == "confirmed")
                        .Sum(t => t.Price * t.Count) : 0,
                    EventCount = o.Events != null ? o.Events.Count(e => e.IsApproved) : 0,
                    TicketsSold = o.Events != null ? o.Events.SelectMany(e => e.Tickets ?? new List<Ticket>())
                        .Where(t => t.TicketStatus == "confirmed")
                        .Sum(t => t.Count) : 0
                })
                .OrderByDescending(o => o.Revenue)
                .ToListAsync();

            var summary = new RevenueSummaryDto
            {
                TotalPlatformRevenue = totalPlatformRevenue,
                OrganizerRevenues = organizerRevenues
            };

            return Ok(summary);
        }

        // GET: api/Admin/system-stats
        [HttpGet("system-stats")]
        public async Task<ActionResult<SystemStatsDto>> GetSystemStats()
        {
            var popularCategories = await _context.Events
                .GroupBy(e => e.Category)
                .Select(g => new CategoryCountDto { Category = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            var stats = new SystemStatsDto
            {
                TotalUsers = await _context.Users.CountAsync(),
                ActiveOrganizers = await _context.Organizers.CountAsync(o => o.IsVerified == true),
                PendingOrganizers = await _context.Organizers.CountAsync(o => o.IsVerified == null || o.IsVerified == false),
                TotalEvents = await _context.Events.CountAsync(),
                ApprovedEvents = await _context.Events.CountAsync(e => e.IsApproved),
                PendingEvents = await _context.Events.CountAsync(e => !e.IsApproved),
                TotalVenues = await _context.Venues.CountAsync(),
                AvailableVenues = await _context.Venues.CountAsync(v => v.IsAvailable),
                TotalTicketsSold = await _context.Tickets.Where(t => t.TicketStatus == "confirmed").SumAsync(t => t.Count),
                TotalRevenue = await _context.Tickets.Where(t => t.TicketStatus == "confirmed").SumAsync(t => t.Price * t.Count),
                RecentRegistrations = await _context.Users.CountAsync(u => u.UserId > 0),
                PopularCategories = popularCategories
            };

            return Ok(stats);
        }
    }
}