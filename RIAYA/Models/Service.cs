using System;
using System.Collections.Generic;

namespace RIAYA.Models;

public partial class Service
{
    public int Id { get; set; }

    public string? ServiceType { get; set; }

    public string? ServiceDescription { get; set; }

    public decimal? Price { get; set; }

    public int? CategoryId { get; set; }

    public TimeOnly? Duration { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ServiceCategory? Category { get; set; }

    public virtual ICollection<ElectronicConsultation> ElectronicConsultations { get; set; } = new List<ElectronicConsultation>();

    public virtual ICollection<HomeCareAppointment> HomeCareAppointments { get; set; } = new List<HomeCareAppointment>();

    public virtual ICollection<InstantHomeCareAppointment> InstantHomeCareAppointments { get; set; } = new List<InstantHomeCareAppointment>();
}
