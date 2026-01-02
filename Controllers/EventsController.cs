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
    public class EventsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Events
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventResponseDto>>> GetEvents([FromQuery] bool? approved = null)
        {
            var query = _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Organizer)
                .ThenInclude(o => o.User)
                .AsQueryable();

            if (approved.HasValue)
            {
                query = query.Where(e => e.IsApproved == approved.Value);
            }

            var events = await query.Select(e => new EventResponseDto
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
                VenueName = e.Venue.VenueName,
                OrganizerName = e.Organizer.User.Name,
                OrganizerOrganization = e.Organizer.Organization ?? "Independent"
            }).ToListAsync();

            return Ok(events);
        }

        // GET: api/Events/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EventResponseDto>> GetEvent(int id)
        {
            var eventEntity = await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Organizer)
                .ThenInclude(o => o.User)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventEntity == null)
            {
                return NotFound();
            }

            var eventDto = new EventResponseDto
            {
                EventId = eventEntity.EventId,
                Title = eventEntity.Title,
                Category = eventEntity.Category,
                EventImage = eventEntity.EventImage,
                Description = eventEntity.Description,
                Location = eventEntity.Location,
                StartDate = eventEntity.StartDate,
                EndDate = eventEntity.EndDate,
                IsApproved = eventEntity.IsApproved,
                TicketPrice = eventEntity.TicketPrice,
                AvailableTickets = eventEntity.AvailableTickets,
                VenueName = eventEntity.Venue.VenueName,
                OrganizerName = eventEntity.Organizer.User.Name,
                OrganizerOrganization = eventEntity.Organizer.Organization ?? "Independent"
            };

            return Ok(eventDto);
        }

        // POST: api/Events
        [HttpPost]
        public async Task<ActionResult<EventResponseDto>> CreateEvent(CreateEventDto dto)
        {
            try
            {
                // For now, use a default organizer for testing
                var organizer = await _context.Organizers
                    .Include(o => o.User)
                    .FirstOrDefaultAsync();
                if (organizer == null)
                {
                    return BadRequest("No organizers found in database");
                }

                var venue = await _context.Venues.FindAsync(dto.VenueId);
                if (venue == null)
                {
                    return BadRequest("Venue not found");
                }
                
                if (!venue.IsAvailable)
                {
                    return BadRequest("Venue not available");
                }

                var eventEntity = new Event
                {
                    Title = dto.Title,
                    Category = dto.Category,
                    EventImage = dto.EventImage,
                    Description = dto.Description,
                    Location = venue.Location,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    VenueId = dto.VenueId,
                    OrganizerId = organizer.OrganizerId,
                    TicketPrice = dto.TicketPrice,
                    AvailableTickets = dto.AvailableTickets,
                    IsApproved = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Events.Add(eventEntity);
                await _context.SaveChangesAsync();

                var responseDto = new EventResponseDto
                {
                    EventId = eventEntity.EventId,
                    Title = eventEntity.Title,
                    Category = eventEntity.Category,
                    EventImage = eventEntity.EventImage,
                    Description = eventEntity.Description,
                    Location = eventEntity.Location,
                    StartDate = eventEntity.StartDate,
                    EndDate = eventEntity.EndDate,
                    IsApproved = eventEntity.IsApproved,
                    TicketPrice = eventEntity.TicketPrice,
                    AvailableTickets = eventEntity.AvailableTickets,
                    VenueName = venue.VenueName,
                    OrganizerName = organizer.User?.Name ?? "Unknown",
                    OrganizerOrganization = organizer.Organization ?? "Independent"
                };

                return CreatedAtAction(nameof(GetEvent), new { id = eventEntity.EventId }, responseDto);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating event: {ex.Message}");
            }
        }

        // PUT: api/Events/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, CreateEventDto dto)
        {
            try
            {
                Console.WriteLine($"Updating event {id} with data: {System.Text.Json.JsonSerializer.Serialize(dto)}");
                
                var eventEntity = await _context.Events.FindAsync(id);
                if (eventEntity == null)
                {
                    Console.WriteLine($"Event {id} not found");
                    return NotFound($"Event {id} not found");
                }

                var venue = await _context.Venues.FindAsync(dto.VenueId);
                if (venue == null)
                {
                    Console.WriteLine($"Venue {dto.VenueId} not found");
                    return BadRequest("Venue not found");
                }
                
                if (!venue.IsAvailable)
                {
                    Console.WriteLine($"Venue {dto.VenueId} not available");
                    return BadRequest("Venue not available");
                }

                eventEntity.Title = dto.Title;
                eventEntity.Category = dto.Category;
                eventEntity.EventImage = dto.EventImage;
                eventEntity.Description = dto.Description;
                eventEntity.Location = venue.Location;
                eventEntity.StartDate = dto.StartDate;
                eventEntity.EndDate = dto.EndDate;
                eventEntity.VenueId = dto.VenueId;
                eventEntity.TicketPrice = dto.TicketPrice;
                eventEntity.AvailableTickets = dto.AvailableTickets;
                eventEntity.IsApproved = false;

                await _context.SaveChangesAsync();
                Console.WriteLine($"Event {id} updated successfully");
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating event {id}: {ex.Message}");
                return BadRequest($"Error updating event: {ex.Message}");
            }
        }

        // POST: api/Events/approve
        [HttpPost("approve")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ApproveEvent(ApproveEventDto dto)
        {
            var eventEntity = await _context.Events.FindAsync(dto.EventId);
            if (eventEntity == null)
            {
                return NotFound();
            }

            eventEntity.IsApproved = dto.IsApproved;
            if (!dto.IsApproved)
            {
                eventEntity.RejectionReason = dto.RejectionReason;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = dto.IsApproved ? "Event approved" : "Event rejected" });
        }

        // GET: api/Events/organizer/{organizerId}
        [HttpGet("organizer/{organizerId}")]
        public async Task<ActionResult<IEnumerable<EventResponseDto>>> GetEventsByOrganizer(int organizerId)
        {
            var events = await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Organizer)
                .ThenInclude(o => o.User)
                .Where(e => e.OrganizerId == organizerId)
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
                    VenueName = e.Venue.VenueName,
                    OrganizerName = e.Organizer.User.Name,
                    OrganizerOrganization = e.Organizer.Organization ?? "Independent"
                }).ToListAsync();

            return Ok(events);
        }

        // DELETE: api/Events/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var eventEntity = await _context.Events.FindAsync(id);
            if (eventEntity == null)
            {
                return NotFound();
            }

            _context.Events.Remove(eventEntity);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
