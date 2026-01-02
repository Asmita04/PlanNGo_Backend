namespace PlanNGo_Backend.Dto
{
    public class AdminDashboardDto
    {
        public int TotalUsers { get; set; }
        public int TotalClients { get; set; }
        public int TotalOrganizers { get; set; }
        public int TotalEvents { get; set; }
        public int ApprovedEvents { get; set; }
        public int PendingEvents { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalTicketsSold { get; set; }
        public List<CategoryStatsDto> CategoryStats { get; set; } = new();
        public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new();
    }

    public class CategoryStatsDto
    {
        public string Category { get; set; } = string.Empty;
        public int EventCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class MonthlyRevenueDto
    {
        public string Month { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int EventCount { get; set; }
    }

    public class OrganizerRevenueDto
    {
        public int OrganizerId { get; set; }
        public string OrganizerName { get; set; } = string.Empty;
        public string Organization { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int EventCount { get; set; }
        public int TicketsSold { get; set; }
    }

    public class SystemStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveOrganizers { get; set; }
        public int PendingOrganizers { get; set; }
        public int TotalEvents { get; set; }
        public int ApprovedEvents { get; set; }
        public int PendingEvents { get; set; }
        public int TotalVenues { get; set; }
        public int AvailableVenues { get; set; }
        public int TotalTicketsSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public int RecentRegistrations { get; set; }
        public List<CategoryCountDto> PopularCategories { get; set; } = new();
    }

    public class CategoryCountDto
    {
        public string Category { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class RevenueSummaryDto
    {
        public decimal TotalPlatformRevenue { get; set; }
        public List<OrganizerRevenueDto> OrganizerRevenues { get; set; } = new();
    }

    public class OrganizerDashboardDto
    {
        public int TotalEvents { get; set; }
        public int ApprovedEvents { get; set; }
        public int PendingEvents { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalTicketsSold { get; set; }
        public List<EventStatsDto> EventStats { get; set; } = new();
        public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new();
    }

    public class EventStatsDto
    {
        public int EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int TicketsSold { get; set; }
        public decimal Revenue { get; set; }
        public bool IsApproved { get; set; }
    }
}