using PlanNGo_Backend.Model;

namespace PlanNGo_Backend.Dto
{
    public class AuthResponseDto
    {
        public string Token { get; set; }
        public User User { get; set; }
        public int ExpiresIn { get; set; }
    }
}
