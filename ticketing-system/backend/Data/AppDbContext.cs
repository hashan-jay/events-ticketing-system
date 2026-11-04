using EventsLab.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EventsLab.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<TicketCategory> TicketCategories => Set<TicketCategory>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<CustomField> CustomFields => Set<CustomField>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<TransactionFieldValue> TransactionFieldValues => Set<TransactionFieldValue>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(x => x.Username).IsUnique();
            entity.Property(x => x.Username).HasMaxLength(80).IsRequired();
            entity.Property(x => x.FullName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Role).HasMaxLength(40).IsRequired();
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Color).HasMaxLength(20);
        });

        modelBuilder.Entity<TicketCategory>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.NumberPrefix).HasMaxLength(20);
            entity.Property(x => x.Price).HasColumnType("decimal(18,2)");
            entity.Property(x => x.Color).HasMaxLength(20);
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasIndex(x => x.TicketNumber).IsUnique();
            entity.Property(x => x.TicketNumber).HasMaxLength(40).IsRequired();
            entity.HasOne(x => x.TicketCategory)
                .WithMany(x => x.Tickets)
                .HasForeignKey(x => x.TicketCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CustomField>(entity =>
        {
            entity.HasIndex(x => x.FieldKey).IsUnique();
            entity.Property(x => x.Label).HasMaxLength(120).IsRequired();
            entity.Property(x => x.FieldKey).HasMaxLength(80).IsRequired();
            entity.Property(x => x.FieldType).HasMaxLength(40).IsRequired();
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasIndex(x => x.IssuedAt);
            entity.Property(x => x.AmountCollected).HasColumnType("decimal(18,2)");
            entity.Property(x => x.CustomerName).HasMaxLength(160);
            entity.Property(x => x.CustomerPhone).HasMaxLength(40);
            entity.Property(x => x.Notes).HasMaxLength(500);
            entity.HasOne(x => x.Ticket)
                .WithOne(x => x.Transaction)
                .HasForeignKey<Transaction>(x => x.TicketId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PaymentMethod)
                .WithMany(x => x.Transactions)
                .HasForeignKey(x => x.PaymentMethodId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.IssuedByUser)
                .WithMany(x => x.Transactions)
                .HasForeignKey(x => x.IssuedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TransactionFieldValue>(entity =>
        {
            entity.HasOne(x => x.Transaction)
                .WithMany(x => x.FieldValues)
                .HasForeignKey(x => x.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.CustomField)
                .WithMany(x => x.FieldValues)
                .HasForeignKey(x => x.CustomFieldId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(x => x.Value).HasMaxLength(500);
        });

        modelBuilder.Entity<SystemSetting>(entity =>
        {
            entity.HasIndex(x => x.Key).IsUnique();
            entity.Property(x => x.Key).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Value).HasMaxLength(400).IsRequired();
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.Property(x => x.Username).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Action).HasMaxLength(80).IsRequired();
            entity.Property(x => x.EntityType).HasMaxLength(80).IsRequired();
            entity.Property(x => x.EntityId).HasMaxLength(40);
            entity.Property(x => x.Summary).HasMaxLength(400).IsRequired();
            entity.Property(x => x.Details).HasMaxLength(2000);
            entity.HasIndex(x => x.OccurredAt);
            entity.HasIndex(x => x.Action);
        });
    }
}
