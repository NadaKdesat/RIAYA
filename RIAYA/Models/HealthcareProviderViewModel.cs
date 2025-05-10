namespace RIAYA.Models
{
    public class HealthcareProviderViewModel
    {
        public string FullName { get; set; }
        public string ProfileImage { get; set; }
        public string Specialization { get; set; }
        public string Bio { get; set; }
        public int? YearsOfExperience { get; set; }
        public string Location { get; set; }
        public string LicenseUrl { get; set; }
        public string CategoryName { get; set; }
        public List<ProviderAvailability> Availability { get; set; }

    }
}
