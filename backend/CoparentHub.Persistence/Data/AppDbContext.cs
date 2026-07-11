using CoparentHub.Domain.Common;
using CoparentHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
        public DbSet<FamilyInvite> FamilyInvites => Set<FamilyInvite>();
        public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
        public DbSet<PushSubscription> PushSubscriptions => Set<PushSubscription>();
        public DbSet<Expense> Expenses => Set<Expense>();
        public DbSet<Message> Messages => Set<Message>();
        public DbSet<Document> Documents => Set<Document>();
        public DbSet<CustodySchedule> CustodySchedules => Set<CustodySchedule>();

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

            var encryptedBytes = new ValueConverter<byte[], byte[]>(
                v => encryptor.EncryptBytes(v)!,
                v => encryptor.DecryptBytes(v)!);

            var byteArrayComparer = new ValueComparer<byte[]>(
                (a, b) => a!.SequenceEqual(b!),
                v => v.Aggregate(0, (hash, b) => HashCode.Combine(hash, b)),
                v => v.ToArray());

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

                b.Property(c => c.Allergies).HasConversion(encryptedNullableString).HasColumnType("text");
                b.Property(c => c.Medications).HasConversion(encryptedNullableString).HasColumnType("text");
                b.Property(c => c.MedicalNotes).HasConversion(encryptedNullableString).HasColumnType("text");
                b.Property(c => c.DoctorName).HasConversion(encryptedNullableString).HasColumnType("text");
                b.Property(c => c.DoctorPhone).HasConversion(encryptedNullableString).HasColumnType("text");
                b.Property(c => c.SchoolName).HasConversion(encryptedNullableString).HasColumnType("text");
                b.Property(c => c.SchoolContact).HasConversion(encryptedNullableString).HasColumnType("text");
                b.Property(c => c.ClothingSize).HasConversion(encryptedNullableString).HasColumnType("text");
                b.Property(c => c.ShoeSize).HasConversion(encryptedNullableString).HasColumnType("text");
                b.Property(c => c.EmergencyContactName).HasConversion(encryptedNullableString).HasColumnType("text");
                b.Property(c => c.EmergencyContactPhone).HasConversion(encryptedNullableString).HasColumnType("text");
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

                b.HasOne<Family>()
                    .WithMany()
                    .HasForeignKey(e => e.FamilyId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasIndex(e => e.FamilyId);
                b.HasIndex(e => e.StartsAt);
            });

            m.Entity<Attendance>(b =>
            {
                b.HasKey(a => a.Id);
                b.Property(a => a.Id).ValueGeneratedNever();

                b.Property(a => a.Status)
                    .HasConversion<string>();

                b.Property(a => a.Reason).HasConversion(encryptedNullableString).HasColumnType("text");

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

                b.HasOne<Family>()
                    .WithMany()
                    .HasForeignKey(n => n.FamilyId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasIndex(n => n.UserId);
                b.HasIndex(n => n.FamilyId);
            });

            m.Entity<FamilyInvite>(b =>
            {
                b.HasKey(i => i.Id);
                b.Property(i => i.Id).ValueGeneratedNever();

                b.Property(i => i.Code).HasMaxLength(8).IsRequired();
                b.HasIndex(i => i.Code).IsUnique();

                b.Property(i => i.InviteeEmail).HasMaxLength(256);
                b.HasIndex(i => i.InviteeEmail);

                b.HasOne<Family>()
                    .WithMany()
                    .HasForeignKey(i => i.FamilyId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasIndex(i => i.FamilyId);
            });

            m.Entity<PasswordResetToken>(b =>
            {
                b.HasKey(t => t.Id);
                b.Property(t => t.Id).ValueGeneratedNever();

                b.Property(t => t.Token).HasMaxLength(64).IsRequired();
                b.HasIndex(t => t.Token).IsUnique();

                b.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasIndex(t => t.UserId);
            });

            m.Entity<PushSubscription>(b =>
            {
                b.HasKey(p => p.Id);
                b.Property(p => p.Id).ValueGeneratedNever();

                // Plaintext, unlike the other per-user string columns above: it needs a unique
                // index and exact-value lookups (dedupe on subscribe, pruning on delivery
                // failure), which this app's random-nonce field encryption can't support.
                b.Property(p => p.Endpoint).HasMaxLength(1000).IsRequired();
                b.HasIndex(p => p.Endpoint).IsUnique();

                b.Property(p => p.P256dh).HasMaxLength(255).IsRequired();
                b.Property(p => p.Auth).HasMaxLength(255).IsRequired();

                b.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasIndex(p => p.UserId);
            });

            m.Entity<Expense>(b =>
            {
                b.HasKey(e => e.Id);
                b.Property(e => e.Id).ValueGeneratedNever();

                b.Property(e => e.Description).HasConversion(encryptedString).HasColumnType("text").IsRequired();

                b.Property(e => e.Category).HasConversion<string>();

                b.Property(e => e.Amount).HasPrecision(12, 2);
                b.Property(e => e.SplitPercentForPayer).HasPrecision(5, 2);

                b.HasOne<Family>()
                    .WithMany()
                    .HasForeignKey(e => e.FamilyId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasIndex(e => e.FamilyId);
                b.HasIndex(e => e.Date);
            });

            m.Entity<Message>(b =>
            {
                b.HasKey(msg => msg.Id);
                b.Property(msg => msg.Id).ValueGeneratedNever();

                b.Property(msg => msg.Body).HasConversion(encryptedString).HasColumnType("text").IsRequired();

                b.HasOne<Family>()
                    .WithMany()
                    .HasForeignKey(msg => msg.FamilyId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasIndex(msg => msg.FamilyId);
            });

            m.Entity<Document>(b =>
            {
                b.HasKey(d => d.Id);
                b.Property(d => d.Id).ValueGeneratedNever();

                b.Property(d => d.FileName).HasConversion(encryptedString).HasColumnType("text").IsRequired();
                b.Property(d => d.Description).HasConversion(encryptedNullableString).HasColumnType("text");
                b.Property(d => d.Content).HasConversion(encryptedBytes, byteArrayComparer).HasColumnType("bytea").IsRequired();

                b.Property(d => d.ContentType).HasMaxLength(255).IsRequired();
                b.Property(d => d.Category).HasConversion<string>();

                b.HasOne<Family>()
                    .WithMany()
                    .HasForeignKey(d => d.FamilyId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasIndex(d => d.FamilyId);
            });
        }
    }
}
