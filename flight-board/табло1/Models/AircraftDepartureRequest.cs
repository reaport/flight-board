namespace AirportManagement.Models
{
    public class AircraftDepartureRequest
    {
        public string FlightId { get; set; }
        public DateTime BoardingStartTime { get; set; }
        public DateTime BoardingEndTime { get; set; }
        public List<SeatAvailability> BoughtSeats { get; set; }
    }
}