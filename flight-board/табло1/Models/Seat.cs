namespace AirportManagement.Models
{
    public class Seat
    {
        /// <summary>
        /// Номер места (например, "1A").
        /// </summary>
        public string SeatNumber { get; set; }

        /// <summary>
        /// Класс места (например, "business" или "economy").
        /// </summary>
        public string SeatClass { get; set; }
    }
}