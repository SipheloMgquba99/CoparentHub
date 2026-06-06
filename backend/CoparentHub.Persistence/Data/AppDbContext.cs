using CoparentHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoparentHub.Persistence.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Family> Families => Set<Family>();
        public DbSet<FamilyMember> Members => Set<FamilyMember>();
        public DbSet<Child> Children => Set<Child>();
        public DbSet<ScheduledEvent> Events => Set<ScheduledEvent>();
        public DbSet<Attendance> Attendances => Set<Attendance>();

        protected override void OnModelCreating(ModelBuilder m)
        {
            m.Entity<User>(b =>
            {
                b.HasKey(u => u.Id);
                b.Property(u => u.Id).ValueGeneratedNever();

                b.HasIndex(u => u.Email).IsUnique();
                b.Property(u => u.Email).HasMaxLength(256).IsRequired();
                b.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
                b.Property(u => u.LastName).HasMaxLength(100).IsRequired();
            });

            m.Entity<Family>(b =>
            {
                b.HasKey(f => f.Id);
                b.Property(f => f.Id).ValueGeneratedNever();

                b.Property(f => f.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                b.HasMany(f => f.Members)
                    .WithOne()
                    .HasForeignKey(x => x.FamilyId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasMany(f => f.Children)
                    .WithOne()
                    .HasForeignKey(x => x.FamilyId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.Navigation(f => f.Members)
                    .HasField("_members")
                    .UsePropertyAccessMode(PropertyAccessMode.Field);

                b.Navigation(f => f.Children)
                    .HasField("_children")
                    .UsePropertyAccessMode(PropertyAccessMode.Field);
            });

            m.Entity<FamilyMember>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).ValueGeneratedNever();
            });

            m.Entity<Child>(b =>
            {
                b.HasKey(c => c.Id);
                b.Property(c => c.Id).ValueGeneratedNever();

                b.Property(c => c.Name)
                    .HasMaxLength(100)
                    .IsRequired();
            });

            m.Entity<ScheduledEvent>(b =>
            {
                b.HasKey(e => e.Id);          
                b.Property(e => e.Id).ValueGeneratedNever();

                b.Property(e => e.Title)
                    .HasMaxLength(200)
                    .IsRequired();

                b.Property(e => e.Notes)
                    .HasMaxLength(1000);

                b.Property(e => e.Type)
                    .HasConversion<string>();

                b.HasMany(e => e.Attendances)
                    .WithOne()
                    .HasForeignKey(a => a.EventId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.Navigation(e => e.Attendances)
                    .HasField("_attendances")
                    .UsePropertyAccessMode(PropertyAccessMode.Field);

                b.HasIndex(e => e.FamilyId);
                b.HasIndex(e => e.StartsAt);
            });

            m.Entity<Attendance>(b =>
            {
                b.HasKey(a => a.Id);
                b.Property(a => a.Id).ValueGeneratedNever();

                b.Property(a => a.Status)
                    .HasConversion<string>();

                b.HasIndex(a => new { a.EventId, a.UserId })
                    .IsUnique();
            });
        }
    }
}