public class FlightSettingsDto
{
    public int PurchaseToRegistrationMinutes { get; set; }
    public int RegistrationToBoardingMinutes { get; set; }
    public int BoardingToEndBoardingMinutes { get; set; }
    public int EndBoardingToDepartureMinutes { get; set; }
    public string Destination { get; set; } // Добавляем поле для города назначения
}