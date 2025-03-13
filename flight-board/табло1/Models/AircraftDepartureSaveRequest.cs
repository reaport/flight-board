namespace AirportManagement.Models
{
    public class AircraftDepartureSaveRequest
    {
        /// <summary>
        /// Уникальный идентификатор самолета.
        /// </summary>
        public string AircraftId { get; set; }

        /// <summary>
        /// Список доступных мест в самолете.
        /// </summary>
        public List<Seat> AvailableSeats { get; set; }

        /// <summary>
        /// Информация о багаже (например, "да" или "нет").
        /// </summary>
        public string Baggage { get; set; }

        /// <summary>
        /// Время начала посадки.
        /// </summary>
        public DateTime BoardingStartTime { get; set; }

        /// <summary>
        /// Время окончания посадки.
        /// </summary>
        public DateTime BoardingEndTime { get; set; }

        /// <summary>
        /// Уникальный идентификатор рейса.
        /// </summary>
        public string FlightId { get; set; }
    }
}