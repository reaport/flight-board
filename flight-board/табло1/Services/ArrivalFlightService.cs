using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AirportManagement.Services
{
    public class ArrivalFlightService
    {
        private readonly ConcurrentDictionary<string, ArrivalFlight> _arrivalFlights = new();
        private readonly IConfiguration _config;
        private readonly ILogger<ArrivalFlight> _logger;
        private readonly AircraftModuleService _aircraftModuleService;

        public ArrivalFlightService(
            IConfiguration config,
            ILogger<ArrivalFlight> logger,
            AircraftModuleService aircraftModuleService)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _aircraftModuleService = aircraftModuleService ?? throw new ArgumentNullException(nameof(aircraftModuleService));
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
                _aircraftModuleService
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

        public async Task ProcessLandingAsync(string flightId)
        {
            if (string.IsNullOrEmpty(flightId))
            {
                throw new ArgumentException("Идентификатор рейса не может быть пустым.", nameof(flightId));
            }

            if (!_arrivalFlights.TryGetValue(flightId, out var arrivalFlight))
            {
                _logger.LogWarning($"Рейс с ID {flightId} не найден.");
                throw new InvalidOperationException($"Рейс с ID {flightId} не найден.");
            }

            try
            {
                // Получаем AircraftId при приземлении
                var aircraftId = await _aircraftModuleService.NotifyLandingAsync(flightId);
                arrivalFlight.SetAircraftId(aircraftId); // Устанавливаем AircraftId
                _logger.LogInformation($"Самолет {aircraftId} приземлился для рейса {flightId}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при обработке приземления рейса {flightId}.");
                throw;
            }
        }
    }
}