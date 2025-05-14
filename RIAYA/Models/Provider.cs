using System;
using System.Collections.Generic;

namespace RIAYA.Models;

public partial class Provider
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public string? Bio { get; set; }

    public int? YearsOfExperience { get; set; }

    public string? Specialization { get; set; }

    public string? Location { get; set; }

    public string LicenseUrl { get; set; } = null!;

    public int? CategoryId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? ProfileImage { get; set; }

    public bool IsActive { get; set; }

    public virtual ServiceCategory? Category { get; set; }

    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    public virtual ICollection<ElectronicConsultation> ElectronicConsultations { get; set; } = new List<ElectronicConsultation>();

    public virtual ICollection<HomeCareAppointment> HomeCareAppointments { get; set; } = new List<HomeCareAppointment>();

    public virtual ICollection<InstantHomeCareAppointment> InstantHomeCareAppointments { get; set; } = new List<InstantHomeCareAppointment>();

    public virtual User? User { get; set; }
}
