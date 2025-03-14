namespace AirportManagement.Models
{
    public class AircraftGenerationResponse
    {
        public string FlightId { get; set; }
        public string AircraftModel { get; set; }
        public List<Seat> Seats { get; set; }
    }
}