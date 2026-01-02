using Microsoft.EntityFrameworkCore;
using PlanNGo_Backend.Model;
using WebAppApi13.Data;

namespace PlanNGo_Backend.Service
{
    public class DataSeeder
    {
        private readonly ApplicationDbContext _context;

        public DataSeeder(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            // Check if data already exists
            if (await _context.Users.AnyAsync())
                return;

            // Create Admin User
            var adminUser = new User
            {
                Name = "Admin User",
                Email = "admin@planngo.com",
                IsEmailVerified = true,
                HashedPassword = BCrypt.Net.BCrypt.EnhancedHashPassword("admin123", 13),
                Role = "admin"
            };
            _context.Users.Add(adminUser);

            // Create Sample Venues
            var venues = new List<Venue>
            {
                new Venue { VenueName = "Grand Convention Center", Location = "Mumbai, Maharashtra", Capacity = 1000, IsAvailable = true },
                new Venue { VenueName = "Tech Hub Auditorium", Location = "Bangalore, Karnataka", Capacity = 500, IsAvailable = true },
                new Venue { VenueName = "Cultural Center", Location = "Delhi, NCR", Capacity = 800, IsAvailable = true },
                new Venue { VenueName = "Sports Complex", Location = "Pune, Maharashtra", Capacity = 2000, IsAvailable = true },
                new Venue { VenueName = "Art Gallery", Location = "Chennai, Tamil Nadu", Capacity = 300, IsAvailable = true }
            };
            _context.Venues.AddRange(venues);

            // Save to get IDs
            await _context.SaveChangesAsync();

            // Create Sample Organizer Users
            var organizerUser1 = new User
            {
                Name = "John Smith",
                Email = "john@events.com",
                IsEmailVerified = true,
                HashedPassword = BCrypt.Net.BCrypt.EnhancedHashPassword("organizer123", 13),
                Role = "organizer",
                Phone = "+91-9876543210",
                Address = "Mumbai, Maharashtra"
            };

            var organizerUser2 = new User
            {
                Name = "Sarah Johnson",
                Email = "sarah@techevents.com",
                IsEmailVerified = true,
                HashedPassword = BCrypt.Net.BCrypt.EnhancedHashPassword("organizer123", 13),
                Role = "organizer",
                Phone = "+91-9876543211",
                Address = "Bangalore, Karnataka"
            };

            _context.Users.AddRange(organizerUser1, organizerUser2);

            // Create Sample Client Users
            var clientUser1 = new User
            {
                Name = "Alice Brown",
                Email = "alice@gmail.com",
                IsEmailVerified = true,
                HashedPassword = BCrypt.Net.BCrypt.EnhancedHashPassword("client123", 13),
                Role = "client",
                Phone = "+91-9876543212",
                Address = "Delhi, NCR"
            };

            var clientUser2 = new User
            {
                Name = "Bob Wilson",
                Email = "bob@gmail.com",
                IsEmailVerified = true,
                HashedPassword = BCrypt.Net.BCrypt.EnhancedHashPassword("client123", 13),
                Role = "client",
                Phone = "+91-9876543213",
                Address = "Pune, Maharashtra"
            };

            _context.Users.AddRange(clientUser1, clientUser2);
            await _context.SaveChangesAsync();

            // Create Organizers
            var organizer1 = new Organizer
            {
                UserId = organizerUser1.UserId,
                Bio = "Professional event organizer with 10+ years experience",
                IsVerified = true,
                Organization = "Elite Events Co.",
                Revenue = 0
            };

            var organizer2 = new Organizer
            {
                UserId = organizerUser2.UserId,
                Bio = "Tech conference specialist",
                IsVerified = true,
                Organization = "TechMeet Solutions",
                Revenue = 0
            };

            _context.Organizers.AddRange(organizer1, organizer2);

            // Create Clients
            var client1 = new Client
            {
                UserId = clientUser1.UserId,
                Dob = new DateOnly(1990, 5, 15),
                Gender = "Female"
            };

            var client2 = new Client
            {
                UserId = clientUser2.UserId,
                Dob = new DateOnly(1985, 8, 22),
                Gender = "Male"
            };

            _context.Clients.AddRange(client1, client2);
            await _context.SaveChangesAsync();

            // Create Sample Events
            var events = new List<Event>
            {
                new Event
                {
                    Title = "Tech Innovation Summit 2024",
                    Category = "Technology",
                    Description = "Join industry leaders for the biggest tech summit of the year. Explore cutting-edge innovations, network with professionals, and discover the future of technology.",
                    Location = "Mumbai Tech District",
                    StartDate = DateTime.UtcNow.AddDays(30),
                    EndDate = DateTime.UtcNow.AddDays(32),
                    VenueId = venues[0].VenueId,
                    OrganizerId = organizer1.OrganizerId,
                    TicketPrice = 2500,
                    AvailableTickets = 800,
                    IsApproved = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Event
                {
                    Title = "Music Festival 2024",
                    Category = "Music",
                    Description = "Experience the best of Indian and international music with top artists performing live. A celebration of music, culture, and community.",
                    Location = "Bangalore Music Grounds",
                    StartDate = DateTime.UtcNow.AddDays(45),
                    EndDate = DateTime.UtcNow.AddDays(47),
                    VenueId = venues[1].VenueId,
                    OrganizerId = organizer2.OrganizerId,
                    TicketPrice = 1500,
                    AvailableTickets = 450,
                    IsApproved = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Event
                {
                    Title = "Startup Pitch Competition",
                    Category = "Business",
                    Description = "Watch innovative startups pitch their ideas to top investors. Network with entrepreneurs and discover the next big thing in business.",
                    Location = "Delhi Business Hub",
                    StartDate = DateTime.UtcNow.AddDays(20),
                    EndDate = DateTime.UtcNow.AddDays(20),
                    VenueId = venues[2].VenueId,
                    OrganizerId = organizer1.OrganizerId,
                    TicketPrice = 1000,
                    AvailableTickets = 600,
                    IsApproved = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Event
                {
                    Title = "Art & Culture Exhibition",
                    Category = "Art",
                    Description = "Explore contemporary art from emerging and established artists. A showcase of creativity, culture, and artistic expression.",
                    Location = "Chennai Art District",
                    StartDate = DateTime.UtcNow.AddDays(15),
                    EndDate = DateTime.UtcNow.AddDays(25),
                    VenueId = venues[4].VenueId,
                    OrganizerId = organizer2.OrganizerId,
                    TicketPrice = 500,
                    AvailableTickets = 250,
                    IsApproved = false, // Pending approval
                    CreatedAt = DateTime.UtcNow
                },
                new Event
                {
                    Title = "Sports Championship Finals",
                    Category = "Sports",
                    Description = "Witness the ultimate showdown in this year's championship finals. Cheer for your favorite teams in this thrilling sporting event.",
                    Location = "Pune Sports Complex",
                    StartDate = DateTime.UtcNow.AddDays(60),
                    EndDate = DateTime.UtcNow.AddDays(62),
                    VenueId = venues[3].VenueId,
                    OrganizerId = organizer1.OrganizerId,
                    TicketPrice = 800,
                    AvailableTickets = 1800,
                    IsApproved = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _context.Events.AddRange(events);
            await _context.SaveChangesAsync();

            // Create Sample Tickets (some bookings)
            var tickets = new List<Ticket>
            {
                new Ticket
                {
                    EventId = events[0].EventId,
                    ClientId = client1.ClientId,
                    Count = 2,
                    Price = events[0].TicketPrice,
                    TicketStatus = "confirmed",
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new Ticket
                {
                    EventId = events[1].EventId,
                    ClientId = client2.ClientId,
                    Count = 1,
                    Price = events[1].TicketPrice,
                    TicketStatus = "confirmed",
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new Ticket
                {
                    EventId = events[2].EventId,
                    ClientId = client1.ClientId,
                    Count = 3,
                    Price = events[2].TicketPrice,
                    TicketStatus = "pending",
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            _context.Tickets.AddRange(tickets);
            await _context.SaveChangesAsync();

            // Create Sample Payments
            var payments = new List<Payment>
            {
                new Payment
                {
                    TicketId = tickets[0].TicketId,
                    Amount = tickets[0].Price * tickets[0].Count,
                    PaymentDate = DateTime.UtcNow.AddDays(-5),
                    PaymentStatus = "completed",
                    PaymentType = "razorpay",
                    PaymentReference = "pay_" + Guid.NewGuid().ToString("N")[..10]
                },
                new Payment
                {
                    TicketId = tickets[1].TicketId,
                    Amount = tickets[1].Price * tickets[1].Count,
                    PaymentDate = DateTime.UtcNow.AddDays(-3),
                    PaymentStatus = "completed",
                    PaymentType = "card",
                    PaymentReference = "pay_" + Guid.NewGuid().ToString("N")[..10]
                }
            };

            _context.Payments.AddRange(payments);

            // Update organizer revenues
            var confirmedTickets = tickets.Where(t => t.TicketStatus == "confirmed").ToList();
            foreach (var organizer in new[] { organizer1, organizer2 })
            {
                var organizerRevenue = confirmedTickets
                    .Where(t => events.Any(e => e.EventId == t.EventId && e.OrganizerId == organizer.OrganizerId))
                    .Sum(t => t.Price * t.Count);
                organizer.Revenue = organizerRevenue;
            }

            await _context.SaveChangesAsync();
        }
    }
}