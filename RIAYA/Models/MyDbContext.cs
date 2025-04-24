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
            entity.HasKey(e => e.Id).HasName("PK__Certific__3214EC074C80A6FA");

            entity.Property(e => e.CertificateUrl).HasMaxLength(255);
            entity.Property(e => e.Institution).HasMaxLength(255);
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.Provider).WithMany(p => p.Certificates)
                .HasForeignKey(d => d.ProviderId)
                .HasConstraintName("FK__Certifica__Provi__45F365D3");
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Contact__3214EC072AFBBF99");

            entity.ToTable("Contact");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Subject).HasMaxLength(255);
        });

        modelBuilder.Entity<ElectronicConsultation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Electron__3214EC07757979B3");

            entity.Property(e => e.AppointmentTime).HasColumnType("datetime");
            entity.Property(e => e.ConsultationLink).HasMaxLength(500);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsConfirmed).HasDefaultValue(false);
            entity.Property(e => e.PatientFullName).HasMaxLength(100);
            entity.Property(e => e.PatientGender).HasMaxLength(10);

            entity.HasOne(d => d.Patient).WithMany(p => p.ElectronicConsultations)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("FK__Electroni__Patie__4E88ABD4");

            entity.HasOne(d => d.Provider).WithMany(p => p.ElectronicConsultations)
                .HasForeignKey(d => d.ProviderId)
                .HasConstraintName("FK__Electroni__Provi__4F7CD00D");

            entity.HasOne(d => d.Service).WithMany(p => p.ElectronicConsultations)
                .HasForeignKey(d => d.ServiceId)
                .HasConstraintName("FK__Electroni__Servi__5070F446");
        });

        modelBuilder.Entity<HealthBlog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__HealthBl__3214EC078295FF55");

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
            entity.HasKey(e => e.Id).HasName("PK__HomeCare__3214EC07B970B17B");

            entity.Property(e => e.ApartmentNumber).HasMaxLength(50);
            entity.Property(e => e.AppointmentTime).HasColumnType("datetime");
            entity.Property(e => e.BuildingName).HasMaxLength(100);
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
            entity.Property(e => e.Street).HasMaxLength(100);

            entity.HasOne(d => d.Patient).WithMany(p => p.HomeCareAppointments)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("FK__HomeCareA__Patie__5535A963");

            entity.HasOne(d => d.Provider).WithMany(p => p.HomeCareAppointments)
                .HasForeignKey(d => d.ProviderId)
                .HasConstraintName("FK__HomeCareA__Provi__5629CD9C");

            entity.HasOne(d => d.Service).WithMany(p => p.HomeCareAppointments)
                .HasForeignKey(d => d.ServiceId)
                .HasConstraintName("FK__HomeCareA__Servi__571DF1D5");
        });

        modelBuilder.Entity<InstantHomeCareAppointment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__InstantH__3214EC07FACDEE43");

            entity.Property(e => e.ApartmentNumber).HasMaxLength(50);
            entity.Property(e => e.AppointmentTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.BuildingName).HasMaxLength(100);
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
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");
            entity.Property(e => e.Street).HasMaxLength(100);

            entity.HasOne(d => d.Patient).WithMany(p => p.InstantHomeCareAppointments)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("FK__InstantHo__Patie__5BE2A6F2");

            entity.HasOne(d => d.Provider).WithMany(p => p.InstantHomeCareAppointments)
                .HasForeignKey(d => d.ProviderId)
                .HasConstraintName("FK__InstantHo__Provi__5DCAEF64");

            entity.HasOne(d => d.Service).WithMany(p => p.InstantHomeCareAppointments)
                .HasForeignKey(d => d.ServiceId)
                .HasConstraintName("FK__InstantHo__Servi__5CD6CB2B");
        });

        modelBuilder.Entity<JoinU>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__JoinUs__3214EC07FF37CB11");

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
            entity.HasKey(e => e.Id).HasName("PK__Provider__3214EC07ED8FE009");

            entity.HasIndex(e => e.UserId, "UQ__Provider__1788CC4DF149F395").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.LicenseUrl).HasMaxLength(255);
            entity.Property(e => e.Location).HasMaxLength(100);
            entity.Property(e => e.Specialization).HasMaxLength(100);

            entity.HasOne(d => d.Category).WithMany(p => p.Providers)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__Providers__Categ__4222D4EF");

            entity.HasOne(d => d.User).WithOne(p => p.Provider)
                .HasForeignKey<Provider>(d => d.UserId)
                .HasConstraintName("FK__Providers__UserI__412EB0B6");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Services__3214EC076D28837E");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Duration).HasDefaultValue(new TimeOnly(1, 0, 0));
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ServiceType).HasMaxLength(50);

            entity.HasOne(d => d.Category).WithMany(p => p.Services)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__Services__Catego__49C3F6B7");
        });

        modelBuilder.Entity<ServiceCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ServiceC__3214EC07B213D39C");

            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<SessionRating>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SessionR__3214EC0728095A1C");

            entity.Property(e => e.AppointmentType).HasMaxLength(20);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07E5434A0E");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534A5943D6D").IsUnique();

            entity.HasIndex(e => e.GoogleId, "UX_Users_GoogleId")
                .IsUnique()
                .HasFilter("([GoogleId] IS NOT NULL)");

            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
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
