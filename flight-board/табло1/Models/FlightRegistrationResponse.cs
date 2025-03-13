using AirportManagement.Models;

public class FlightRegistrationResponse
{
    public string FlightId { get; set; } // Например, "SU001"
    public DateTime StartPlantingTime { get; set; } // RegistrationEndTime
    public DateTime DepartureTime { get; set; }
    public DateTime StartRegisterTime { get; set; } 
    public List<SeatAvailability> SeatsAircraft { get; set; }
}