using System.ComponentModel.DataAnnotations;

namespace PlanNGo_Backend.Dto
{
    public class OrganizerProfileDto
    {
        public int OrganizerId { get; set; }
        public string? Bio { get; set; }
        public bool? IsVerified { get; set; }
        public decimal? Revenue { get; set; }
        public string? Organization { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Pfp { get; set; }
    }

    public class ApproveOrganizerDto
    {
        [Required]
        public int OrganizerId { get; set; }
        [Required]
        public bool IsApproved { get; set; }
        public string? RejectionReason { get; set; }
    }

    public class UpdateOrganizerProfileDto
    {
        public string? Bio { get; set; }
        public string? Organization { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }
}