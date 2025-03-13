namespace AirportManagement.Models
{
    public class AircraftDepartureRequest
    {
        public string FlightId { get; set; }
        public DateTime BoardingStartTime { get; set; }
        public DateTime BoardingEndTime { get; set; }
        public List<AvailableSeatsInfo> Seats { get; set; } // Исправлено на Seats
    }
}