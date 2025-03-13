namespace AirportManagement.Models
{
    public class AircraftArrivalResponse
    {
        public DateTime LandingDateTimeToReaport { get; set; }
        public int OccupiedSeats { get; set; }
        public string BaggageToReaport { get; set; }
        public string AircraftStatus { get; set; }
    }
}