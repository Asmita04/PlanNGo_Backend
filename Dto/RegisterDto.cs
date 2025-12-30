namespace PlanNGo_Backend.Dto
{
    public class RegisterDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }

        // Requested role from frontend
        public string Role { get; set; }  // "User" or "Organizer"
    }

}
