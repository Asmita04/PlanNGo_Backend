
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

            modelBuilder.Entity<Ticket>()
        .HasOne(t => t.Client)
        .WithOne()
        .HasForeignKey<Ticket>(t => t.ClientId)
        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
        .HasOne(t => t.Event)
        .WithMany() 
        .HasForeignKey(t => t.EventId)
        .OnDelete(DeleteBehavior.Cascade);

            
            modelBuilder.Entity<Payment>()
       .HasOne(p => p.Ticket)
       .WithOne() 
       .HasForeignKey<Payment>(p => p.TicketId)
       .OnDelete(DeleteBehavior.Cascade);
        }






    }
}
