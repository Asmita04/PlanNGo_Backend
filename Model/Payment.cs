using Newtonsoft.Json;

namespace PlanNGo_Backend.Model
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentStatus { get; set; }
        public string PaymentType { get; set; }

        // Foreign Key
        public int ClientId { get; set; }
        [JsonIgnore]
        public Client? Client { get; set; }
        public int TicketId { get; set; }
        [JsonIgnore]
        public Ticket? Ticket { get; set; }



    }
}
