using AirportManagement.Models;

public class TicketPurchaseResponse
{
    public string FlightId { get; set; }
    public string SeatClass { get; set; }
    public string Direction { get; set; } // CityTo
    public DateTime DepartureTime { get; set; }
    public List<SeatAvailability> AvailableSeats { get; set; } // Список доступных мест
}