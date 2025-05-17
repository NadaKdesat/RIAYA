using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RIAYA.Models;

public partial class MyDbContext : DbContext
{
    public MyDbContext()
    {
    }

    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Certificate> Certificates { get; set; }

    public virtual DbSet<Contact> Contacts { get; set; }

    public virtual DbSet<ElectronicConsultation> ElectronicConsultations { get; set; }

    public virtual DbSet<HealthBlog> HealthBlogs { get; set; }

    public virtual DbSet<HomeCareAppointment> HomeCareAppointments { get; set; }

    public virtual DbSet<InstantHomeCareAppointment> InstantHomeCareAppointments { get; set; }

    public virtual DbSet<JoinU> JoinUs { get; set; }

    public virtual DbSet<Provider> Providers { get; set; }

    public virtual DbSet<ProviderAvailability> ProviderAvailabilities { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<ServiceCategory> ServiceCategories { get; set; }

    public virtual DbSet<SessionRating> SessionRatings { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-GTJ4IDU;Database=RIAYA;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Certific__3214EC0747CB9A0D");

            entity.Property(e => e.CertificateUrl).HasMaxLength(255);
            entity.Property(e => e.Institution).HasMaxLength(255);
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.Provider).WithMany(p => p.Certificates)
                .HasForeignKey(d => d.ProviderId)
                .HasConstraintName("FK__Certifica__Provi__46E78A0C");
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Contact__3214EC073BCA5A4F");

            entity.ToTable("Contact");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.IsReplied).HasDefaultValue(false);
            entity.Property(e => e.RepliedAt).HasColumnType("datetime");
            entity.Property(e => e.Subject).HasMaxLength(255);
        });

        modelBuilder.Entity<ElectronicConsultation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Electron__3214EC07A6F613D6");

            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.ConsultationLink).HasMaxLength(500);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsConfirmed).HasDefaultValue(false);
            entity.Property(e => e.PatientFullName).HasMaxLength(100);
            entity.Property(e => e.PatientGender).HasMaxLength(10);
            entity.Property(e => e.ServiceName).HasMaxLength(100);

            entity.HasOne(d => d.Patient).WithMany(p => p.ElectronicConsultations)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("FK__Electroni__Patie__4F7CD00D");

            entity.HasOne(d => d.Provider).WithMany(p => p.ElectronicConsultations)
                .HasForeignKey(d => d.ProviderId)
                .HasConstraintName("FK__Electroni__Provi__5070F446");

            entity.HasOne(d => d.Service).WithMany(p => p.ElectronicConsultations)
                .HasForeignKey(d => d.ServiceId)
                .HasConstraintName("FK__Electroni__Servi__5165187F");
        });

        modelBuilder.Entity<HealthBlog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__HealthBl__3214EC07E7ABDAF1");

            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.ContentUrl).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PublishDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.Type).HasMaxLength(20);
        });

        modelBuilder.Entity<HomeCareAppointment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__HomeCare__3214EC07564C8F9D");

            entity.Property(e => e.ApartmentNumber).HasMaxLength(50);
            entity.Property(e => e.BuildingName).HasMaxLength(100);
            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.ContactPhone).HasMaxLength(20);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FloorNumber).HasMaxLength(50);
            entity.Property(e => e.IsConfirmed).HasDefaultValue(false);
            entity.Property(e => e.Latitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.LocationType).HasMaxLength(50);
            entity.Property(e => e.Longitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.PatientFullName).HasMaxLength(100);
            entity.Property(e => e.PatientGender).HasMaxLength(10);
            entity.Property(e => e.ServiceName).HasMaxLength(100);
            entity.Property(e => e.StreetName).HasMaxLength(100);

            entity.HasOne(d => d.Patient).WithMany(p => p.HomeCareAppointments)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("FK__HomeCareA__Patie__5629CD9C");

            entity.HasOne(d => d.Provider).WithMany(p => p.HomeCareAppointments)
                .HasForeignKey(d => d.ProviderId)
                .HasConstraintName("FK__HomeCareA__Provi__571DF1D5");

            entity.HasOne(d => d.Service).WithMany(p => p.HomeCareAppointments)
                .HasForeignKey(d => d.ServiceId)
                .HasConstraintName("FK__HomeCareA__Servi__5812160E");
        });

        modelBuilder.Entity<InstantHomeCareAppointment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__InstantH__3214EC07D6611B6B");

            entity.Property(e => e.ApartmentNumber).HasMaxLength(50);
            entity.Property(e => e.BuildingName).HasMaxLength(100);
            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.ContactPhone).HasMaxLength(20);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FloorNumber).HasMaxLength(50);
            entity.Property(e => e.IsConfirmed).HasDefaultValue(false);
            entity.Property(e => e.Latitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.LocationType).HasMaxLength(50);
            entity.Property(e => e.Longitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.PatientFullName).HasMaxLength(100);
            entity.Property(e => e.PatientGender).HasMaxLength(10);
            entity.Property(e => e.ServiceName).HasMaxLength(100);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");
            entity.Property(e => e.StreetName).HasMaxLength(100);

            entity.HasOne(d => d.Patient).WithMany(p => p.InstantHomeCareAppointments)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("FK__InstantHo__Patie__5CD6CB2B");

            entity.HasOne(d => d.Provider).WithMany(p => p.InstantHomeCareAppointments)
                .HasForeignKey(d => d.ProviderId)
                .HasConstraintName("FK__InstantHo__Provi__5EBF139D");

            entity.HasOne(d => d.Service).WithMany(p => p.InstantHomeCareAppointments)
                .HasForeignKey(d => d.ServiceId)
                .HasConstraintName("FK__InstantHo__Servi__5DCAEF64");
        });

        modelBuilder.Entity<JoinU>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__JoinUs__3214EC072F708E99");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.InterestedIn).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
        });

        modelBuilder.Entity<Provider>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Provider__3214EC07EB50E0F7");

            entity.HasIndex(e => e.UserId, "UQ__Provider__1788CC4D42C8A322").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LicenseUrl).HasMaxLength(255);
            entity.Property(e => e.Location).HasMaxLength(100);
            entity.Property(e => e.ProfileImage)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Specialization).HasMaxLength(100);

            entity.HasOne(d => d.Category).WithMany(p => p.Providers)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__Providers__Categ__4316F928");

            entity.HasOne(d => d.User).WithOne(p => p.Provider)
                .HasForeignKey<Provider>(d => d.UserId)
                .HasConstraintName("FK__Providers__UserI__4222D4EF");
        });

        modelBuilder.Entity<ProviderAvailability>(entity =>
        {
            entity.HasKey(e => e.ProviderAvailabilityId).HasName("PK__Provider__B8DFFA7469F3CA86");

            entity.ToTable("ProviderAvailability");

            entity.Property(e => e.ProviderAvailabilityId).HasColumnName("ProviderAvailabilityID");
            entity.Property(e => e.ProviderId).HasColumnName("ProviderID");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Services__3214EC07DB6EBB62");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Duration).HasDefaultValue(new TimeOnly(1, 0, 0));
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ServiceType).HasMaxLength(50);

            entity.HasOne(d => d.Category).WithMany(p => p.Services)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__Services__Catego__4AB81AF0");
        });

        modelBuilder.Entity<ServiceCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ServiceC__3214EC07E51CCDA5");

            entity.Property(e => e.CategoryDescription).HasMaxLength(500);
            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<SessionRating>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SessionR__3214EC0782C9EE5D");

            entity.Property(e => e.AppointmentType).HasMaxLength(20);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC071C90B45D");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105342371A592").IsUnique();

            entity.HasIndex(e => e.GoogleId, "UX_Users_GoogleId")
                .IsUnique()
                .HasFilter("([GoogleId] IS NOT NULL)");

            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.GoogleId).HasMaxLength(255);
            entity.Property(e => e.IsGoogleUser).HasDefaultValue(false);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.UserType).HasMaxLength(20);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
