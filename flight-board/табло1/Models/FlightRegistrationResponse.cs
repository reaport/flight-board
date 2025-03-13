namespace AirportManagement.Models
{
    public class FlightRegistrationResponse
    {
        public string FlightId { get; set; }
        public string FlightName { get; set; } // Добавлено свойство FlightName
        public DateTime StartRegisterTime { get; set; }
        public DateTime EndRegisterTime { get; set; } // Добавлено свойство EndRegisterTime
        public DateTime StartPlantingTime { get; set; }
        public DateTime DepartureTime { get; set; }
        public List<Seat> SeatsAircraft { get; set; }
    }
}