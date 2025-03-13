namespace AirportManagement
{
    public class FlightSettings
    {
        public int PurchaseToRegistrationMinutes { get; set; } = 120;
        public int RegistrationToBoardingMinutes { get; set; } = 60;
        public int BoardingToEndBoardingMinutes { get; set; } = 30;
        public int EndBoardingToDepartureMinutes { get; set; } = 15;
    }
}