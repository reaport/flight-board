using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace AirportManagement.Services
{
    public class ArrivalFlightService
    {
        private readonly ConcurrentDictionary<string, ArrivalFlight> _arrivalFlights = new ConcurrentDictionary<string, ArrivalFlight>();
        private readonly IConfiguration _config;
        private readonly ILogger<ArrivalFlight> _logger;
        private readonly AircraftModuleService _aircraftModuleService; // Добавляем AircraftModuleService

        public ArrivalFlightService(
            IConfiguration config,
            ILogger<ArrivalFlight> logger,
            AircraftModuleService aircraftModuleService) // Добавляем AircraftModuleService
        {
            _config = config;
            _logger = logger;
            _aircraftModuleService = aircraftModuleService;
        }

        public ArrivalFlight CreateArrivalFlight(string departureCity, int arrivalTimeOffset)
        {
            if (string.IsNullOrEmpty(departureCity))
            {
                throw new ArgumentException("Город отправления не может быть пустым.", nameof(departureCity));
            }

            DateTime arrivalTime = DateTime.Now.AddMinutes(arrivalTimeOffset);
            var arrivalFlight = new ArrivalFlight(
                departureCity,
                _logger,
                _config,
                arrivalTime,
                _aircraftModuleService // Передаем AircraftModuleService
            );

            if (!_arrivalFlights.TryAdd(arrivalFlight.FlightId, arrivalFlight))
            {
                _logger.LogWarning($"Рейс с ID {arrivalFlight.FlightId} уже существует.");
                throw new InvalidOperationException($"Рейс с ID {arrivalFlight.FlightId} уже существует.");
            }

            _logger.LogInformation($"Создан новый рейс на прилет {arrivalFlight.FlightId} из {departureCity}.");
            _logger.LogInformation($"Текущее количество рейсов: {_arrivalFlights.Count}");
            return arrivalFlight;
        }

        public ArrivalFlight GetArrivalFlight(string flightId)
        {
            _arrivalFlights.TryGetValue(flightId, out var arrivalFlight);
            return arrivalFlight;
        }

        public ArrivalFlight GetArrivalFlightByLandingTime(DateTime landingTime)
        {
            return _arrivalFlights.Values.FirstOrDefault(flight => flight.ArrivalTime == landingTime);
        }

        public List<ArrivalFlight> GetAllArrivalFlights()
        {
            return _arrivalFlights.Values.ToList();
        }
    }
}