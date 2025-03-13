namespace AirportManagement.Models
{
    public class AircraftData
    {
        public string AircraftId { get; set; }
        public List<Seat> Seats { get; set; } // Список мест в самолете
    }

    public class Seat
    {
        public string SeatNumber { get; set; }
        public string SeatClass { get; set; }
    }
}