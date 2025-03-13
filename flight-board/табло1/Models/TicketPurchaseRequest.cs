namespace AirportManagement.Models
{
    public class TicketPurchaseRequest
    {
        public string FlightId { get; set; }
        public string AircraftId { get; set; }
        public string CityFrom { get; set; }
        public string CityTo { get; set; }
        public string SeatClass { get; set; }
    }
}