namespace AirportManagement.Dto
{
    public class FlightDto
    {
        public string FlightId { get; set; }
        public string CityFrom { get; set; }
        public string CityTo { get; set; }
        public DateTime TicketSalesStart { get; set; }
        public DateTime RegistrationStartTime { get; set; }
        public DateTime RegistrationEndTime { get; set; }
        public DateTime BoardingStartTime { get; set; }
        public DateTime BoardingEndTime { get; set; }
        public DateTime DepartureTime { get; set; }
        public bool IsBoardingClosed { get; set; }
        public bool IsRegistrationClosed { get; set; }
        public bool IsTicketSalesClosed { get; set; }
    }
}