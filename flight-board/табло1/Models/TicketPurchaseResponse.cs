using System.Text.Json.Serialization;

namespace AirportManagement.Models
{
    public class TicketPurchaseResponse
    {
        public string FlightId { get; set; }
        public string CityFrom { get; set; }
        public string CityTo { get; set; }
        [JsonPropertyName("startRegisterTime")] // Указываем имя поля в JSON
        public DateTime RegistrationStartTime { get; set; }
        public DateTime TakeoffDateTime { get; set; }
        public List<AvailableSeatsInfo> AvailableSeats { get; set; }
    }

    public class AvailableSeatsInfo
    {
        public string SeatClass { get; set; }
        public int SeatCount { get; set; }
    }
}