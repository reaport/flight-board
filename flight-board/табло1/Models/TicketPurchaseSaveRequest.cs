namespace AirportManagement.Models
{
    public class TicketPurchaseSaveRequest
    {
        public List<PurchasedSeat> PurchasedSeats { get; set; }
        public string Baggage { get; set; }
    }

    public class PurchasedSeat
    {
        public string SeatClass { get; set; }
        public int Quantity { get; set; }
    }
}