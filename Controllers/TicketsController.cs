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
    public class TicketsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TicketsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/Tickets/book
        [HttpPost("book")]
        [Authorize(Roles = "client")]
        public async Task<ActionResult<TicketResponseDto>> BookTicket(BookTicketDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.UserId == userId);

            if (client == null)
            {
                return BadRequest("Client not found");
            }

            var eventEntity = await _context.Events
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(e => e.EventId == dto.EventId);

            if (eventEntity == null)
            {
                return NotFound("Event not found");
            }

            if (!eventEntity.IsApproved)
            {
                return BadRequest("Event is not approved");
            }

            if (eventEntity.AvailableTickets < dto.Count)
            {
                return BadRequest("Not enough tickets available");
            }

            if (eventEntity.StartDate <= DateTime.UtcNow)
            {
                return BadRequest("Cannot book tickets for past events");
            }

            var ticket = new Ticket
            {
                EventId = dto.EventId,
                ClientId = client.ClientId,
                Count = dto.Count,
                Price = eventEntity.TicketPrice,
                TicketStatus = "pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.Tickets.Add(ticket);
            
            // Reduce available tickets
            eventEntity.AvailableTickets -= dto.Count;
            
            await _context.SaveChangesAsync();

            var ticketResponse = new TicketResponseDto
            {
                TicketId = ticket.TicketId,
                Price = ticket.Price,
                Count = ticket.Count,
                TicketStatus = ticket.TicketStatus,
                CreatedAt = ticket.CreatedAt,
                EventTitle = eventEntity.Title,
                EventStartDate = eventEntity.StartDate,
                EventEndDate = eventEntity.EndDate,
                VenueName = eventEntity.Venue.VenueName,
                VenueLocation = eventEntity.Venue.Location,
                ClientName = client.User?.Name ?? "Unknown"
            };

            return CreatedAtAction(nameof(GetTicket), new { id = ticket.TicketId }, ticketResponse);
        }

        // GET: api/Tickets/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<TicketResponseDto>> GetTicket(int id)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Event)
                .ThenInclude(e => e.Venue)
                .Include(t => t.Client)
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(t => t.TicketId == id);

            if (ticket == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Check if user has permission to view this ticket
            if (userRole != "admin" && ticket.Client.UserId != userId)
            {
                return Forbid();
            }

            var ticketResponse = new TicketResponseDto
            {
                TicketId = ticket.TicketId,
                Price = ticket.Price,
                Count = ticket.Count,
                TicketStatus = ticket.TicketStatus,
                CreatedAt = ticket.CreatedAt,
                EventTitle = ticket.Event?.Title ?? string.Empty,
                EventStartDate = ticket.Event?.StartDate ?? DateTime.MinValue,
                EventEndDate = ticket.Event?.EndDate ?? DateTime.MinValue,
                VenueName = ticket.Event?.Venue?.VenueName ?? string.Empty,
                VenueLocation = ticket.Event?.Venue?.Location ?? string.Empty,
                ClientName = ticket.Client?.User?.Name ?? string.Empty
            };

            return Ok(ticketResponse);
        }

        // GET: api/Tickets/my-tickets
        [HttpGet("my-tickets")]
        [Authorize(Roles = "client")]
        public async Task<ActionResult<IEnumerable<TicketResponseDto>>> GetMyTickets()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.UserId == userId);

            if (client == null)
            {
                return BadRequest("Client not found");
            }

            var tickets = await _context.Tickets
                .Include(t => t.Event)
                .ThenInclude(e => e.Venue)
                .Include(t => t.Client)
                .ThenInclude(c => c.User)
                .Where(t => t.ClientId == client.ClientId)
                .Select(t => new TicketResponseDto
                {
                    TicketId = t.TicketId,
                    Price = t.Price,
                    Count = t.Count,
                    TicketStatus = t.TicketStatus,
                    CreatedAt = t.CreatedAt,
                    EventTitle = t.Event.Title,
                    EventStartDate = t.Event.StartDate,
                    EventEndDate = t.Event.EndDate,
                    VenueName = t.Event.Venue.VenueName,
                    VenueLocation = t.Event.Venue.Location,
                    ClientName = t.Client.User.Name
                })
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return Ok(tickets);
        }

        // POST: api/Tickets/5/confirm-payment
        [HttpPost("{id}/confirm-payment")]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> ConfirmPayment(int id, PaymentDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.UserId == userId);

            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.TicketId == id && t.ClientId == client.ClientId);

            if (ticket == null)
            {
                return NotFound();
            }

            if (ticket.TicketStatus != "pending")
            {
                return BadRequest("Ticket payment already processed");
            }

            // Create payment record
            var payment = new Payment
            {
                TicketId = ticket.TicketId,
                Amount = ticket.Price * ticket.Count,
                PaymentDate = DateTime.UtcNow,
                PaymentStatus = "completed",
                PaymentType = dto.PaymentType,
                PaymentReference = dto.PaymentReference
            };

            _context.Payments.Add(payment);
            
            // Update ticket status
            ticket.TicketStatus = "confirmed";
            
            await _context.SaveChangesAsync();

            return Ok(new { message = "Payment confirmed successfully", ticketId = ticket.TicketId });
        }

        // DELETE: api/Tickets/5/cancel
        [HttpDelete("{id}/cancel")]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> CancelTicket(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.UserId == userId);

            var ticket = await _context.Tickets
                .Include(t => t.Event)
                .FirstOrDefaultAsync(t => t.TicketId == id && t.ClientId == client.ClientId);

            if (ticket == null)
            {
                return NotFound();
            }

            if (ticket.TicketStatus == "cancelled")
            {
                return BadRequest("Ticket already cancelled");
            }

            // Check if event hasn't started yet (allow cancellation up to 24 hours before)
            if (ticket.Event.StartDate <= DateTime.UtcNow.AddHours(24))
            {
                return BadRequest("Cannot cancel tickets within 24 hours of event start");
            }

            // Restore available tickets
            ticket.Event.AvailableTickets += ticket.Count;
            
            // Update ticket status
            ticket.TicketStatus = "cancelled";
            
            await _context.SaveChangesAsync();

            return Ok(new { message = "Ticket cancelled successfully" });
        }

        // GET: api/Tickets/event/5
        [HttpGet("event/{eventId}")]
        [Authorize(Roles = "organizer,admin")]
        public async Task<ActionResult<IEnumerable<TicketResponseDto>>> GetTicketsByEvent(int eventId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Check if organizer owns this event
            if (userRole == "organizer")
            {
                var organizer = await _context.Organizers.FirstOrDefaultAsync(o => o.UserId == userId);
                if (organizer != null)
                {
                    var eventEntity = await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId && e.OrganizerId == organizer.OrganizerId);
                    if (eventEntity == null)
                    {
                        return Forbid();
                    }
                }
            }

            var tickets = await _context.Tickets
                .Include(t => t.Event)
                .ThenInclude(e => e.Venue)
                .Include(t => t.Client)
                .ThenInclude(c => c.User)
                .Where(t => t.EventId == eventId)
                .Select(t => new TicketResponseDto
                {
                    TicketId = t.TicketId,
                    Price = t.Price,
                    Count = t.Count,
                    TicketStatus = t.TicketStatus,
                    CreatedAt = t.CreatedAt,
                    EventTitle = t.Event.Title,
                    EventStartDate = t.Event.StartDate,
                    EventEndDate = t.Event.EndDate,
                    VenueName = t.Event.Venue.VenueName,
                    VenueLocation = t.Event.Venue.Location,
                    ClientName = t.Client.User.Name
                })
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return Ok(tickets);
        }
    }
}