using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanNGo_Backend.Model;
using System.ComponentModel.DataAnnotations;
using WebAppApi13.Data;

namespace PlanNGo_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VenuesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VenuesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Venues
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VenueResponseDto>>> GetVenues()
        {
            var venues = await _context.Venues
                .Select(v => new VenueResponseDto
                {
                    VenueId = v.VenueId,
                    VenueName = v.VenueName,
                    Location = v.Location,
                    Capacity = v.Capacity,
                    IsAvailable = v.IsAvailable,
                    GoogleMapsUrl = v.GoogleMapsUrl,
                    Address = v.Address,
                    City = v.City,
                    State = v.State,
                    Country = v.Country,
                    PostalCode = v.PostalCode,
                    ContactPhone = v.ContactPhone,
                    ContactEmail = v.ContactEmail,
                    Description = v.Description,
                    Amenities = v.Amenities,
                    CreatedAt = v.CreatedAt
                })
                .OrderBy(v => v.VenueName)
                .ToListAsync();

            return Ok(venues);
        }

        // GET: api/Venues/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VenueResponseDto>> GetVenue(int id)
        {
            var venue = await _context.Venues.FindAsync(id);

            if (venue == null)
            {
                return NotFound();
            }

            var venueDto = new VenueResponseDto
            {
                VenueId = venue.VenueId,
                VenueName = venue.VenueName,
                Location = venue.Location,
                Capacity = venue.Capacity,
                IsAvailable = venue.IsAvailable,
                GoogleMapsUrl = venue.GoogleMapsUrl,
                Address = venue.Address,
                City = venue.City,
                State = venue.State,
                Country = venue.Country,
                PostalCode = venue.PostalCode,
                ContactPhone = venue.ContactPhone,
                ContactEmail = venue.ContactEmail,
                Description = venue.Description,
                Amenities = venue.Amenities,
                CreatedAt = venue.CreatedAt
            };

            return Ok(venueDto);
        }

        // POST: api/Venues
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<VenueResponseDto>> CreateVenue(CreateVenueDto dto)
        {
            var venue = new Venue
            {
                VenueName = dto.VenueName,
                Location = dto.Location,
                Capacity = dto.Capacity,
                IsAvailable = dto.IsAvailable,
                GoogleMapsUrl = dto.GoogleMapsUrl,
                Address = dto.Address,
                City = dto.City,
                State = dto.State,
                Country = dto.Country,
                PostalCode = dto.PostalCode,
                ContactPhone = dto.ContactPhone,
                ContactEmail = dto.ContactEmail,
                Description = dto.Description,
                Amenities = dto.Amenities,
                CreatedAt = DateTime.UtcNow
            };

            _context.Venues.Add(venue);
            await _context.SaveChangesAsync();

            var venueResponse = new VenueResponseDto
            {
                VenueId = venue.VenueId,
                VenueName = venue.VenueName,
                Location = venue.Location,
                Capacity = venue.Capacity,
                IsAvailable = venue.IsAvailable,
                GoogleMapsUrl = venue.GoogleMapsUrl,
                Address = venue.Address,
                City = venue.City,
                State = venue.State,
                Country = venue.Country,
                PostalCode = venue.PostalCode,
                ContactPhone = venue.ContactPhone,
                ContactEmail = venue.ContactEmail,
                Description = venue.Description,
                Amenities = venue.Amenities,
                CreatedAt = venue.CreatedAt
            };

            return CreatedAtAction(nameof(GetVenue), new { id = venue.VenueId }, venueResponse);
        }

        // PUT: api/Venues/5
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateVenue(int id, UpdateVenueDto dto)
        {
            var venue = await _context.Venues.FindAsync(id);
            if (venue == null)
            {
                return NotFound();
            }

            venue.VenueName = dto.VenueName;
            venue.Location = dto.Location;
            venue.Capacity = dto.Capacity;
            venue.IsAvailable = dto.IsAvailable;
            venue.GoogleMapsUrl = dto.GoogleMapsUrl;
            venue.Address = dto.Address;
            venue.City = dto.City;
            venue.State = dto.State;
            venue.Country = dto.Country;
            venue.PostalCode = dto.PostalCode;
            venue.ContactPhone = dto.ContactPhone;
            venue.ContactEmail = dto.ContactEmail;
            venue.Description = dto.Description;
            venue.Amenities = dto.Amenities;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Venues/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteVenue(int id)
        {
            var venue = await _context.Venues.FindAsync(id);
            if (venue == null)
            {
                return NotFound();
            }

            // Check if venue is being used by any events
            var hasEvents = await _context.Events.AnyAsync(e => e.VenueId == id);
            if (hasEvents)
            {
                return BadRequest("Cannot delete venue that has associated events");
            }

            _context.Venues.Remove(venue);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Venues/available
        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<VenueResponseDto>>> GetAvailableVenues()
        {
            var venues = await _context.Venues
                .Where(v => v.IsAvailable)
                .Select(v => new VenueResponseDto
                {
                    VenueId = v.VenueId,
                    VenueName = v.VenueName,
                    Location = v.Location,
                    Capacity = v.Capacity,
                    IsAvailable = v.IsAvailable,
                    GoogleMapsUrl = v.GoogleMapsUrl,
                    Address = v.Address,
                    City = v.City,
                    State = v.State,
                    Country = v.Country,
                    PostalCode = v.PostalCode,
                    ContactPhone = v.ContactPhone,
                    ContactEmail = v.ContactEmail,
                    Description = v.Description,
                    Amenities = v.Amenities,
                    CreatedAt = v.CreatedAt
                })
                .OrderBy(v => v.VenueName)
                .ToListAsync();

            return Ok(venues);
        }
    }

    public class CreateVenueDto
    {
        [Required]
        public string VenueName { get; set; }
        [Required]
        public string Location { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int Capacity { get; set; }
        public bool IsAvailable { get; set; } = true;
        public string? GoogleMapsUrl { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public string? Description { get; set; }
        public string? Amenities { get; set; }
    }

    public class UpdateVenueDto
    {
        [Required]
        public string VenueName { get; set; }
        [Required]
        public string Location { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int Capacity { get; set; }
        public bool IsAvailable { get; set; }
        public string? GoogleMapsUrl { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public string? Description { get; set; }
        public string? Amenities { get; set; }
    }

    public class VenueResponseDto
    {
        public int VenueId { get; set; }
        public string VenueName { get; set; }
        public string Location { get; set; }
        public int Capacity { get; set; }
        public bool IsAvailable { get; set; }
        public string? GoogleMapsUrl { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public string? Description { get; set; }
        public string? Amenities { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
