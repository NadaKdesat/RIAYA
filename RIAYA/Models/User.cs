using System;
using System.Collections.Generic;

namespace RIAYA.Models;

public partial class User
{
    public int Id { get; set; }

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? PasswordHash { get; set; }

    public string? Phone { get; set; }

    public string? City { get; set; }

    public string? Gender { get; set; }

    public string? UserType { get; set; }

    public bool? IsGoogleUser { get; set; }

    public string? GoogleId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<ElectronicConsultation> ElectronicConsultations { get; set; } = new List<ElectronicConsultation>();

    public virtual ICollection<HomeCareAppointment> HomeCareAppointments { get; set; } = new List<HomeCareAppointment>();

    public virtual ICollection<InstantHomeCareAppointment> InstantHomeCareAppointments { get; set; } = new List<InstantHomeCareAppointment>();

    public virtual Provider? Provider { get; set; }
}
