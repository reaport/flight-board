namespace AirportManagement.Models
{
    public class AircraftDepartureResponse
    {
        public string FlightId { get; set; }
        public DateTime BoardingStartTime { get; set; }
        public DateTime BoardingEndTime { get; set; }
        public List<SeatAvailability> Seats { get; set; } // Исправлено на Seats
    }
}