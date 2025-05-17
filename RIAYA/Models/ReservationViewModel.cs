namespace RIAYA.Models
{
    public class ReservationViewModel
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string PatientFullName { get; set; }
        public string CategoryName { get; set; }
        public string ServiceName { get; set; }
        public string ProviderName { get; set; }
        public string ContactPhone { get; set; }
        public string AppointmentDate { get; set; }
        public string AppointmentTime { get; set; }
        public bool? IsConfirmed { get; set; }
        public string LocationType { get; set; }
        public string BuildingName { get; set; }
        public string StreetName { get; set; }
        public string ConsultationLink { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
