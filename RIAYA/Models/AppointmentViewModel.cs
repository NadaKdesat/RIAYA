namespace RIAYA.Models
{
    public class AppointmentViewModel
    {
        public string BookingType { get; set; }
        public string PatientFullName { get; set; }
        public string PatientGender { get; set; }
        public DateOnly? PatientBirthDate { get; set; }
        public string CategoryName { get; set; }
        public string ProviderName { get; set; }
        public DateTime AppointmentDateTime { get; set; }
        public string Status { get; set; }
        public string Location { get; set; }
        public string ConsultationLink { get; set; }
    }
}
