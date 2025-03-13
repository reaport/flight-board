using AirportManagement.Dto;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace AirportManagement.Services
{
    public class FlightService
    {
        private readonly List<DepartureFlightGenerator> _flights = new List<DepartureFlightGenerator>();
        private readonly CityService _cityService;

        public FlightService(CityService cityService)
        {
            _cityService = cityService;
        }

        public DepartureFlightGenerator CreateFlight(string destination, ILogger<DepartureFlightGenerator> logger, FlightSettings settings)
        {
            if (string.IsNullOrEmpty(destination))
            {
                throw new ArgumentException("Город назначения не может быть пустым.", nameof(destination));
            }

            // Логируем создание рейса
            logger.LogInformation("Создание рейса с городом назначения: {Destination}", destination);

            var flight = new DepartureFlightGenerator(destination, logger, settings);
            _flights.Add(flight);

            // Логируем успешное создание рейса
            logger.LogInformation("Рейс успешно создан: {@Flight}", flight);

            return flight;
        }

        public DepartureFlightGenerator GetFlight(string flightId)
        {
            return _flights.FirstOrDefault(f => f.FlightId == flightId);
        }

        public List<DepartureFlightGenerator> GetAllFlights()
        {
            return _flights;
        }
    }
}