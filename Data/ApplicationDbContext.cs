
using Microsoft.EntityFrameworkCore;
using PlanNGo_Backend.Model;


namespace WebAppApi13.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Venue> Venues { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Organizer> Organizers { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Ticket> Tickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ------------------------
            // Client → User (1–1, Uni)
            // ------------------------
            modelBuilder.Entity<Client>()
                .HasOne(c => c.User)
                .WithOne() // NO navigation on User
                .HasForeignKey<Client>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ------------------------
            // Organizer → User (1–1, Uni)
            // ------------------------
            modelBuilder.Entity<Organizer>()
                .HasOne(o => o.User)
                .WithOne() // NO navigation on User
                .HasForeignKey<Organizer>(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ------------------------
            // Organizer → Event (1–Many)
            // ------------------------
            modelBuilder.Entity<Organizer>()
                .HasMany(o => o.Events)
                .WithOne(e => e.Organizer)
                .HasForeignKey(e => e.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict);

            // ------------------------
            // Event → Venue (Many–1)
            // ------------------------
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Venue)
                .WithMany()
                .HasForeignKey(e => e.VenueId)
                .OnDelete(DeleteBehavior.Restrict);

            // ------------------------
            // Event → Tickets (1–Many)
            // ------------------------
            modelBuilder.Entity<Event>()
                .HasMany(e => e.Tickets)
                .WithOne(t => t.Event)
                .HasForeignKey(t => t.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // ------------------------
            // Client → Ticket (1–Many)
            // ------------------------
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Client)
                .WithMany()
                .HasForeignKey(t => t.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            // ------------------------
            // Ticket → Payment (1–1)
            // ------------------------
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Ticket)
                .WithOne()
                .HasForeignKey<Payment>(p => p.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            // ------------------------
            // Configure decimal precision
            // ------------------------
            modelBuilder.Entity<Event>()
                .Property(e => e.TicketPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Organizer>()
                .Property(o => o.Revenue)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Ticket>()
                .Property(t => t.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            // ------------------------
            // Default values
            // ------------------------
            modelBuilder.Entity<Event>()
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<Ticket>()
                .Property(t => t.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<Payment>()
                .Property(p => p.PaymentDate)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<Organizer>()
                .Property(o => o.IsVerified)
                .HasDefaultValue(false);

            modelBuilder.Entity<Event>()
                .Property(e => e.IsApproved)
                .HasDefaultValue(false);
        }






    }
}
