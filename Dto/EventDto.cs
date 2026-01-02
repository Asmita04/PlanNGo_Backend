using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PlanNGo_Backend.Dto
{
    public class CreateEventDto
    {
        [Required]
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
        [Required]
        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;
        [JsonPropertyName("eventImage")]
        public string? EventImage { get; set; }
        [Required]
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
        [Required]
        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }
        [Required]
        [JsonPropertyName("endDate")]
        public DateTime EndDate { get; set; }
        [Required]
        [JsonPropertyName("venueId")]
        public int VenueId { get; set; }
        [Required]
        [JsonPropertyName("ticketPrice")]
        public decimal TicketPrice { get; set; }
        [Required]
        [JsonPropertyName("availableTickets")]
        public int AvailableTickets { get; set; }
    }

    public class EventResponseDto
    {
        public int EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? EventImage { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Location { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsApproved { get; set; }
        public decimal TicketPrice { get; set; }
        public int AvailableTickets { get; set; }
        public string VenueName { get; set; } = string.Empty;
        public string OrganizerName { get; set; } = string.Empty;
        public string OrganizerOrganization { get; set; } = string.Empty;
    }

    public class ApproveEventDto
    {
        [Required]
        public int EventId { get; set; }
        [Required]
        public bool IsApproved { get; set; }
        public string? RejectionReason { get; set; }
    }
}