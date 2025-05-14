namespace RIAYA.Models
{
    public class BookingViewModel
    {
        public int Id { get; set; }
        public string ServiceName { get; set; }
        public DateTime AppointmentDateTime { get; set; }
        public string Location { get; set; } // يمكن أن يكون "Online" أو عنوان فعلي
        public string Status { get; set; }
        public string BookingType { get; set; } // "ElectronicConsultation", "HomeCareAppointment", "InstantHomeCareAppointment"
        public bool CanCancel { get; set; }
    }
}
