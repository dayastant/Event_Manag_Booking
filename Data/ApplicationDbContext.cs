using Microsoft.EntityFrameworkCore;
using Event_Management_System.Models;

namespace Event_Management_System.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Member> Members { get; set; }
        public DbSet<Guest> Guests { get; set; }
        public DbSet<Venue> Venues { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Inquiry> Inquiries { get; set; }
        public DbSet<Admin> Admins { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure table names to match existing database
            modelBuilder.Entity<Member>().ToTable("Member");
            modelBuilder.Entity<Guest>().ToTable("Guest");
            modelBuilder.Entity<Venue>().ToTable("Venue");
            modelBuilder.Entity<Event>().ToTable("Event");
            modelBuilder.Entity<Ticket>().ToTable("Ticket");
            modelBuilder.Entity<Booking>().ToTable("Booking");
            modelBuilder.Entity<Review>().ToTable("Review");
            modelBuilder.Entity<Inquiry>().ToTable("Inquiry");

            // Configure relationships
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Venue)
                .WithMany(v => v.Events);
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Event)
                .WithMany(e => e.Tickets)
                .HasForeignKey(t => t.EventID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Member)
                .WithMany(m => m.Bookings)
                .HasForeignKey(b => b.MemberID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Event)
                .WithMany(e => e.Bookings)
                .HasForeignKey(b => b.EventID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Member)
                .WithMany(m => m.Reviews)
                .HasForeignKey(r => r.MemberID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Event)
                .WithMany(e => e.Reviews)
                .HasForeignKey(r => r.EventID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inquiry>()
                .HasOne(i => i.Guest)
                .WithMany(g => g.Inquiries)
                .HasForeignKey(i => i.GuestID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inquiry>()
                .HasOne(i => i.Member)
                .WithMany(m => m.Inquiries)
                .HasForeignKey(i => i.MemberID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure unique constraints
            modelBuilder.Entity<Member>()
                .HasIndex(m => m.Email)
                .IsUnique();

            // Configure Admin table
            modelBuilder.Entity<Admin>().ToTable("Admin");
            modelBuilder.Entity<Admin>()
                .HasIndex(a => a.Email)
                .IsUnique();

            // Seed default admin account
            // Password: Admin@123 (pre-hashed with BCrypt work factor 11)
            // IMPORTANT: This is a pre-generated hash - do not call HashPassword at compile time
            // modelBuilder.Entity<Admin>().HasData(new Admin
            // {
            //     AdminID = 1,
            //     Email = "admin@culturapass.com",
            //     PasswordHash = "$2a$11$JNcZ7qPvJkKXzLZRZ8vqHOu8tGjC5xY5Y85oN/h8qCxqX3qO3qO3q",
            //     FullName = "System Administrator",
            //     CreatedDate = new DateTime(2026, 1, 1),
            //     Status = "Active"
            // });


modelBuilder.Entity<Admin>().HasData(new Admin
{
    AdminID = 1,
    Email = "admin@culturapass.com",
    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
    FullName = "System Administrator",
    CreatedDate = new DateTime(2026, 1, 1),
    Status = "Active"
});
    

        }
    }
}
