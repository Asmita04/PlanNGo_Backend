using Newtonsoft.Json;

namespace PlanNGo_Backend.Model
{
    public class Ticket
    {
        public int TicketId { get; set; }
        public decimal Price { get; set; }
        public int Count { get; set; }
        public string TicketStatus { get; set; }
        public DateTime CreatedAt { get; set; }

        // Foreign Keys
        public int EventId { get; set; }
        [JsonIgnore]
        public Event? Event { get; set; }
        public int ClientId { get; set; }
        [JsonIgnore]
        public Client? Client { get; set; }
        

    }
}
