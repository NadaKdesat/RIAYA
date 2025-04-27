using System;
using System.Collections.Generic;

namespace RIAYA.Models;

public partial class HomeCareAppointment
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

    public DateTime? AppointmentTime { get; set; }

    public bool? IsConfirmed { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public string? LocationType { get; set; }

    public string? BuildingName { get; set; }

    public string? FloorNumber { get; set; }

    public string? ApartmentNumber { get; set; }

    public string? StreetName { get; set; }

    public string? ContactPhone { get; set; }

    public string? PatientConditionDescription { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? Patient { get; set; }

    public virtual Provider? Provider { get; set; }

    public virtual Service? Service { get; set; }
}
