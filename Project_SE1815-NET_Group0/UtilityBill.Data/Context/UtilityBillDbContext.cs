using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using UtilityBill.Data.Models;

namespace UtilityBill.Data.Context;

public partial class UtilityBillDbContext : DbContext
{
    public UtilityBillDbContext()
    {
    }

    public UtilityBillDbContext(DbContextOptions<UtilityBillDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<InvoiceDetail> InvoiceDetails { get; set; }

    public virtual DbSet<MaintenanceSchedule> MaintenanceSchedules { get; set; }

    public virtual DbSet<MeterReading> MeterReadings { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<PushSubscription> PushSubscriptions { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<TenantHistory> TenantHistories { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Invoices__3214EC0799069A92");

            entity.HasIndex(e => new { e.RoomId, e.InvoicePeriodYear, e.InvoicePeriodMonth }, "IX_Invoices_RoomPeriod");

            entity.HasIndex(e => e.Status, "IX_Invoices_Status");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Room).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Invoices__RoomId__5070F446");
        });

        modelBuilder.Entity<InvoiceDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__InvoiceD__3214EC079E6FC9B2");

            entity.HasIndex(e => e.InvoiceId, "IX_InvoiceDetails_InvoiceId");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Invoice).WithMany(p => p.InvoiceDetails)
                .HasForeignKey(d => d.InvoiceId)
                .HasConstraintName("FK__InvoiceDe__Invoi__534D60F1");
        });

        modelBuilder.Entity<MaintenanceSchedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Maintena__3214EC07CE17A211");

            entity.Property(e => e.Block).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CreatedByUserId).HasMaxLength(450);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.MaintenanceSchedules)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Maintenan__Creat__5BE2A6F2");

            entity.HasOne(d => d.Room).WithMany(p => p.MaintenanceSchedules)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK__Maintenan__RoomI__5AEE82B9");
        });

        modelBuilder.Entity<MeterReading>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MeterRea__3214EC07CCF74BA8");

            entity.HasIndex(e => new { e.RoomId, e.ReadingYear, e.ReadingMonth }, "IX_MeterReadings_RoomMonthYear");

            entity.HasIndex(e => new { e.RoomId, e.ReadingMonth, e.ReadingYear }, "UQ__MeterRea__247DDF995FA867EA").IsUnique();

            entity.Property(e => e.ElectricReading).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.RecordedByUserId).HasMaxLength(450);
            entity.Property(e => e.WaterReading).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.RecordedByUser).WithMany(p => p.MeterReadings)
                .HasForeignKey(d => d.RecordedByUserId)
                .HasConstraintName("FK__MeterRead__Recor__4BAC3F29");

            entity.HasOne(d => d.Room).WithMany(p => p.MeterReadings)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MeterRead__RoomI__4AB81AF0");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Notifica__3214EC07F4E936F9");

            entity.Property(e => e.RelatedEntityId).HasMaxLength(255);
            entity.Property(e => e.SentAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Type).HasMaxLength(50);
            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__UserI__60A75C0F");
        });

        modelBuilder.Entity<PushSubscription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PushSubs__3214EC07A1B2C3D4");

            entity.HasIndex(e => e.Endpoint, "IX_PushSubscriptions_Endpoint").IsUnique();

            entity.Property(e => e.Endpoint).IsRequired();
            entity.Property(e => e.P256Dh).IsRequired();
            entity.Property(e => e.Auth).IsRequired();
            entity.Property(e => e.UserId).HasMaxLength(450);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PushSubsc__UserI__70A75C0F");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Payments__3214EC0776E8803B");

            entity.HasIndex(e => e.InvoiceId, "IX_Payments_InvoiceId");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TransactionCode).HasMaxLength(255);

            entity.HasOne(d => d.Invoice).WithMany(p => p.Payments)
                .HasForeignKey(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payments__Invoic__571DF1D5");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC07ACBD1954");

            entity.Property(e => e.Name).HasMaxLength(256);
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Rooms__3214EC076D36A9C6");

            entity.HasIndex(e => e.Status, "IX_Rooms_Status");

            entity.HasIndex(e => e.RoomNumber, "UQ__Rooms__AE10E07A2A9FB89F").IsUnique();

            entity.HasIndex(e => e.QRCodeData, "UQ__Rooms__E8D1C96FD9285C5E").IsUnique();

            entity.Property(e => e.Area).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Block).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.QRCodeData)
                .HasMaxLength(255)
                .HasColumnName("QRCodeData");
            entity.Property(e => e.RoomNumber).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<TenantHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TenantHi__3214EC071C5E23E0");

            entity.HasIndex(e => e.RoomId, "IX_TenantHistories_RoomId");

            entity.HasIndex(e => e.TenantId, "IX_TenantHistories_TenantId");

            entity.HasOne(d => d.Room).WithMany(p => p.TenantHistories)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TenantHis__RoomI__45F365D3");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TenantHistories)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TenantHis__Tenan__46E78A0C");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07C0A05EA1");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.FullName).HasMaxLength(256);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserRole",
                    r => r.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .HasConstraintName("FK__UserRoles__RoleI__3E52440B"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("FK__UserRoles__UserI__3D5E1FD2"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId").HasName("PK__UserRole__AF2760AD2D95938C");
                        j.ToTable("UserRoles");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
