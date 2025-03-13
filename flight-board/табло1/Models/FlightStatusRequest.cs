namespace AirportManagement.Models
{
    public class FlightStatusRequest
    {
        public string FlightId { get; set; }
        public string CityFrom { get; set; }
        public string CityTo { get; set; }
        public DateTime RegistrationStartTime { get; set; }
        public DateTime RegistrationEndTime { get; set; }
    }
}