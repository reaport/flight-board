namespace AirportManagement.Models
{
    public class TicketPurchaseResponse
    {
        public string FlightId { get; set; }
        public string AircraftId { get; set; }
        public string CityFrom { get; set; }
        public string CityTo { get; set; }
        public List<SeatAvailability> AvailableSeats { get; set; }
        public string Baggage { get; set; }
        public DateTime TakeoffDateTime { get; set; }
        public DateTime LandingDateTime { get; set; }
    }
}