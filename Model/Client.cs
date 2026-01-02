using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;



namespace PlanNGo_Backend.Model
{
    public class Client
    {
        public int ClientId { get; set; }
        public DateOnly? Dob { get; set; }
        public string? Gender { get; set; }
        public string? Bio { get; set; }

        [ForeignKey("UserId")]
        public int UserId { get; set; }
        [JsonIgnore]
        public User? User { get; set; }


    }
}
