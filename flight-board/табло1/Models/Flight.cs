namespace AirportManagement.Models
{
    public class Flight
    {
        public string FlightId { get; set; }
        public string CityFrom { get; set; }
        public string CityTo { get; set; }
        public DateTime DepartureTime { get; set; }
        public string AircraftId { get; set; }
    }
}
