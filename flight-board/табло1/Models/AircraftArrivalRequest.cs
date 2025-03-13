namespace AirportManagement.Models
{
    public class AircraftArrivalRequest
    {
        public DateTime LandingDateTimeToReaport { get; set; }
        public int OccupiedSeats { get; set; }
        public string BaggageToReaport { get; set; }
    }
}