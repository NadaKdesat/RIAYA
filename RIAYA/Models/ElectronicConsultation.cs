using System;
using System.Collections.Generic;

namespace RIAYA.Models;

public partial class ElectronicConsultation
{
    public int Id { get; set; }

    public int? PatientId { get; set; }

    public int? ProviderId { get; set; }

    public int? ServiceId { get; set; }

    public string? PatientFullName { get; set; }

    public string? PatientGender { get; set; }

    public DateOnly? PatientBirthDate { get; set; }

    public string? CategoryName { get; set; }

    public string? ServiceName { get; set; }

    public DateOnly AppointmentDate { get; set; }

    public TimeOnly AppointmentTime { get; set; }

    public bool? IsConfirmed { get; set; }

    public string? PatientConditionDescription { get; set; }

    public string? ConsultationLink { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? Patient { get; set; }

    public virtual Provider? Provider { get; set; }

    public virtual Service? Service { get; set; }
}
