using Newtonsoft.Json;

namespace PlanNGo_Backend.Model
{
    public class Event
    {
        public int EventId { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string? EventImage { get; set; }
        public string Description { get; set; }
        public string? Location { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsApproved { get; set; }
        // Foreign Keys
        public int VenueId { get; set; }
        [JsonIgnore]
        public Venue? Venue { get; set; }
        public int OrganizerId { get; set; }
        [JsonIgnore]
        public Organizer? Organizer { get; set; }

    }
}
