using AirportManagement.Models;

namespace AirportManagement.Models
{
    public class ArrivalFlight
    {
        public string FlightId { get; set; } // Идентификатор рейса
        public string DepartureCity { get; set; } // Город отправления
        public DateTime ArrivalTime { get; set; } // Время прибытия
        public AircraftData AircraftData { get; set; } // Данные о самолете
    }

    public class AircraftData
    {
        public string AircraftId { get; set; }
        public List<Seat> Seats { get; set; } // Список мест в самолете
    }

    public class Seat
    {
        public string SeatNumber { get; set; }
        public string SeatClass { get; set; }
    }
}