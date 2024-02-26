using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BaldurBillsApp.Models;

public partial class BaldurBillsDbContext : DbContext
{
    public BaldurBillsDbContext()
    {
    }

    public BaldurBillsDbContext(DbContextOptions<BaldurBillsDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AppUser> AppUsers { get; set; }

    public virtual DbSet<Attachment> Attachments { get; set; }

    public virtual DbSet<CostType> CostTypes { get; set; }

    public virtual DbSet<InvoiceItem> InvoiceItems { get; set; }

    public virtual DbSet<InvoicesList> InvoicesLists { get; set; }

    public virtual DbSet<Prepayment> Prepayments { get; set; }

    public virtual DbSet<Settlement> Settlements { get; set; }

    public virtual DbSet<ToPlnRate> ToPlnRates { get; set; }

    public virtual DbSet<Vendor> Vendors { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-AB6HGHF\\SQLEXPRESS;Database=BaldurBillsDB;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__AppUser__1788CCAC88C4AA4E");

            entity.ToTable("AppUser");

            entity.HasIndex(e => e.Email, "UQ__AppUser__A9D105348C6B265E").IsUnique();

            entity.HasIndex(e => e.UserName, "UQ__AppUser__C9F284565EDA2AB7").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.LastLogin).HasColumnType("datetime");
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PasswordHash).HasMaxLength(64);
            entity.Property(e => e.UserName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.HasKey(e => e.AttachmentId).HasName("PK__Attachme__442C64DEDECA16AD");

            entity.ToTable("Attachment");

            entity.Property(e => e.AttachmentId).HasColumnName("AttachmentID");
            entity.Property(e => e.FilePath)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.InvoiceId).HasColumnName("InvoiceID");

            entity.HasOne(d => d.Invoice).WithMany(p => p.Attachments)
                .HasForeignKey(d => d.InvoiceId)
                .HasConstraintName("FK__Attachmen__Invoi__32E0915F");
        });

        modelBuilder.Entity<CostType>(entity =>
        {
            entity.HasKey(e => e.CostId).HasName("PK__CostType__8285231E1A72704F");

            entity.ToTable("CostType");

            entity.Property(e => e.CostId).HasColumnName("CostID");
            entity.Property(e => e.CostName)
                .HasMaxLength(30)
                .IsUnicode(false);
        });

        modelBuilder.Entity<InvoiceItem>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PK__InvoiceI__727E83EB5845487C");

            entity.ToTable("InvoiceItem");

            entity.Property(e => e.ItemId).HasColumnName("ItemID");
            entity.Property(e => e.CostId).HasColumnName("CostID");
            entity.Property(e => e.GrossAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.InvoiceId).HasColumnName("InvoiceID");
            entity.Property(e => e.NetAmount).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Cost).WithMany(p => p.InvoiceItems)
                .HasForeignKey(d => d.CostId)
                .HasConstraintName("FK__InvoiceIt__CostI__300424B4");

            entity.HasOne(d => d.Invoice).WithMany(p => p.InvoiceItems)
                .HasForeignKey(d => d.InvoiceId)
                .HasConstraintName("FK__InvoiceIt__Invoi__2F10007B");
        });

        modelBuilder.Entity<InvoicesList>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("PK__Invoices__D796AAD52A2FF53B");

            entity.ToTable("InvoicesList");

            entity.HasIndex(e => e.RegistryNumber, "UQ__Invoices__F451997582BCCBEF").IsUnique();

            entity.Property(e => e.InvoiceId).HasColumnName("InvoiceID");
            entity.Property(e => e.Comment)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.GrossAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.InvoiceNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NetAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.RateId).HasColumnName("RateID");
            entity.Property(e => e.RegistryNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Title)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.VendorId).HasColumnName("VendorID");

            entity.HasOne(d => d.Rate).WithMany(p => p.InvoicesLists)
                .HasForeignKey(d => d.RateId)
                .HasConstraintName("FK__InvoicesL__RateI__2C3393D0");

            entity.HasOne(d => d.Vendor).WithMany(p => p.InvoicesLists)
                .HasForeignKey(d => d.VendorId)
                .HasConstraintName("FK__InvoicesL__Vendo__2B3F6F97");
        });

        modelBuilder.Entity<Prepayment>(entity =>
        {
            entity.HasKey(e => e.PrepaymentId).HasName("PK__Prepayme__A27D5B3D6644E38D");

            entity.ToTable("Prepayment");

            entity.HasIndex(e => e.PrepaymentRegistryNumber, "UQ__Prepayme__83CFFBD6B5979B8D").IsUnique();

            entity.Property(e => e.PrepaymentId).HasColumnName("PrepaymentID");
            entity.Property(e => e.PrepaymentAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PrepaymentCurrency)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.PrepaymentRegistryNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.RemainingAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.VendorId).HasColumnName("VendorID");

            entity.HasOne(d => d.Vendor).WithMany(p => p.Prepayments)
                .HasForeignKey(d => d.VendorId)
                .HasConstraintName("FK__Prepaymen__Vendo__3A81B327");
        });

        modelBuilder.Entity<Settlement>(entity =>
        {
            entity.HasKey(e => e.SettlementId).HasName("PK__Settleme__771254BAE3A98151");

            entity.ToTable("Settlement");

            entity.Property(e => e.SettlementId).HasColumnName("SettlementID");
            entity.Property(e => e.InvoiceId).HasColumnName("InvoiceID");
            entity.Property(e => e.PrepaymentId).HasColumnName("PrepaymentID");
            entity.Property(e => e.SettlementAmount).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Invoice).WithMany(p => p.Settlements)
                .HasForeignKey(d => d.InvoiceId)
                .HasConstraintName("FK__Settlemen__Invoi__3D5E1FD2");

            entity.HasOne(d => d.Prepayment).WithMany(p => p.Settlements)
                .HasForeignKey(d => d.PrepaymentId)
                .HasConstraintName("FK__Settlemen__Prepa__3E52440B");
        });

        modelBuilder.Entity<ToPlnRate>(entity =>
        {
            entity.HasKey(e => e.RateId).HasName("PK__ToPlnRat__58A7CCBC3EAFFFBC");

            entity.ToTable("ToPlnRate");

            entity.Property(e => e.RateId).HasColumnName("RateID");
            entity.Property(e => e.RateCurrency)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.RateValue).HasColumnType("decimal(5, 4)");
        });

        modelBuilder.Entity<Vendor>(entity =>
        {
            entity.HasKey(e => e.VendorId).HasName("PK__Vendor__FC8618D363A2ACDA");

            entity.ToTable("Vendor");

            entity.Property(e => e.VendorId).HasColumnName("VendorID");
            entity.Property(e => e.AccountNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Address)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Country)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Swift)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.VatId)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("VatID");
            entity.Property(e => e.VendorName)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
