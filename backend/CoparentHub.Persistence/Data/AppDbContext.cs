using CoparentHub.Domain.Common;
using CoparentHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Globalization;

namespace CoparentHub.Persistence.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options, IFieldEncryptor encryptor) : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Family> Families => Set<Family>();
        public DbSet<FamilyMember> Members => Set<FamilyMember>();
        public DbSet<Child> Children => Set<Child>();
        public DbSet<ScheduledEvent> Events => Set<ScheduledEvent>();
        public DbSet<Attendance> Attendances => Set<Attendance>();
        public DbSet<Notification> Notifications => Set<Notification>();

        private static DateOnly? ParseEncryptedDate(string? decrypted) =>
            decrypted is null ? null : DateOnly.Parse(decrypted, CultureInfo.InvariantCulture);

        protected override void OnModelCreating(ModelBuilder m)
        {
            var encryptedString = new ValueConverter<string, string>(
                v => encryptor.Encrypt(v)!,
                v => encryptor.Decrypt(v)!);

            var encryptedNullableString = new ValueConverter<string?, string?>(
                v => encryptor.Encrypt(v),
                v => encryptor.Decrypt(v));

            var encryptedDateOnly = new ValueConverter<DateOnly?, string?>(
                v => encryptor.Encrypt(v.HasValue ? v.Value.ToString("O", CultureInfo.InvariantCulture) : null),
                v => ParseEncryptedDate(encryptor.Decrypt(v)));

            m.Entity<User>(b =>
            {
                b.HasKey(u => u.Id);
                b.Property(u => u.Id).ValueGeneratedNever();

                b.HasIndex(u => u.Email).IsUnique();
                b.Property(u => u.Email).HasMaxLength(256).IsRequired();

                b.Property(u => u.FirstName).HasConversion(encryptedString).HasColumnType("text").IsRequired();
                b.Property(u => u.LastName).HasConversion(encryptedString).HasColumnType("text").IsRequired();
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

                b.HasIndex(x => new { x.FamilyId, x.UserId }).IsUnique();

                b.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            m.Entity<Child>(b =>
            {
                b.HasKey(c => c.Id);
                b.Property(c => c.Id).ValueGeneratedNever();

                b.Property(c => c.Name).HasConversion(encryptedString).HasColumnType("text").IsRequired();
                b.Property(c => c.DateOfBirth).HasConversion(encryptedDateOnly).HasColumnType("text");
            });

            m.Entity<ScheduledEvent>(b =>
            {
                b.HasKey(e => e.Id);
                b.Property(e => e.Id).ValueGeneratedNever();

                b.Property(e => e.Title).HasConversion(encryptedString).HasColumnType("text").IsRequired();
                b.Property(e => e.Notes).HasConversion(encryptedNullableString).HasColumnType("text");

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

            m.Entity<Notification>(b =>
            {
                b.HasKey(n => n.Id);
                b.Property(n => n.Id).ValueGeneratedNever();

                b.Property(n => n.Type)
                    .HasConversion<string>();

                b.Property(n => n.Message).HasConversion(encryptedString).HasColumnType("text").IsRequired();

                b.HasIndex(n => n.UserId);
            });
        }
    }
}
