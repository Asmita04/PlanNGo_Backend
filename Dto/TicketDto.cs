using System.ComponentModel.DataAnnotations;

namespace PlanNGo_Backend.Dto
{
    public class BookTicketDto
    {
        [Required]
        public int EventId { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Count must be at least 1")]
        public int Count { get; set; }
    }

    public class TicketResponseDto
    {
        public int TicketId { get; set; }
        public decimal Price { get; set; }
        public int Count { get; set; }
        public string TicketStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public DateTime EventStartDate { get; set; }
        public DateTime EventEndDate { get; set; }
        public string VenueName { get; set; } = string.Empty;
        public string VenueLocation { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
    }

    public class PaymentDto
    {
        [Required]
        public int TicketId { get; set; }
        [Required]
        public string PaymentType { get; set; } = string.Empty; // "razorpay", "card", etc.
        public string? PaymentReference { get; set; }
    }
}