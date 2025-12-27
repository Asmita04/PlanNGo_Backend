using Newtonsoft.Json;

namespace PlanNGo_Backend.Model
{
    public class Organizer
    {   
        public int OrganizerId { get; set; }
        public string Bio { get; set; }
        public bool IsVerified { get; set; }
        public decimal Revenue { get; set; }
        public string Organization { get; set; }

        // Foreign Key
        public int UserId { get; set; }

        [JsonIgnore]
        public User? User { get; set; }

        [JsonIgnore]
        public List<Event>? Events { get; set; }

    }
}
