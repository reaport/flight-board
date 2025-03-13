namespace AirportManagement.Models
{
    public class AircraftDepartureResponse
    {
        public string FlightId { get; set; } // Добавлено
        public DateTime BoardingStartTime { get; set; } // Добавлено
        public DateTime BoardingEndTime { get; set; } // Добавлено
    }
}