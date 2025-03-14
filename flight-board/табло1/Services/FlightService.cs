using AirportManagement.Dto;
using AirportManagement.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AirportManagement.Services
{
    public class FlightService
    {
        private readonly List<DepartureFlightGenerator> _flights = new List<DepartureFlightGenerator>();
        private readonly CityService _cityService;
        private readonly IRegistrationService _registrationService;
        private readonly IAircraftService _aircraftService;
        private readonly OrchestratorService _orchestratorService; // Добавляем поле для OrchestratorService
        private readonly ILogger<FlightService> _logger;

        // Внедряем зависимости через конструктор
        public FlightService(
            CityService cityService,
            IRegistrationService registrationService,
            IAircraftService aircraftService,
            ILogger<FlightService> logger)
        {
            _cityService = cityService;
            _registrationService = registrationService;
            _aircraftService = aircraftService;
            _logger = logger;
        }

        // Асинхронный метод для создания рейса
        public async Task<DepartureFlightGenerator> CreateFlightAsync(
            string destination,
            ILogger<DepartureFlightGenerator> logger,
            FlightSettings settings)
        {
            if (string.IsNullOrEmpty(destination))
            {
                throw new ArgumentException("Город назначения не может быть пустым.", nameof(destination));
            }

            // Логируем создание рейса
            logger.LogInformation("Создание рейса с городом назначения: {Destination}", destination);

            // Получаем данные о самолете из модуля самолета
            var aircraftData = await _aircraftService.GetAircraftDataAsync("dummyId"); // aircraftId не используется, так как /generate не требует его

            // Передаем все необходимые параметры
            var flight = new DepartureFlightGenerator(
                destination,
                logger,
                settings,
                _registrationService, // Передаем IRegistrationService
                _aircraftService, // Передаем IAircraftService
                _orchestratorService,
                aircraftData // Передаем данные о самолете
            );

            _flights.Add(flight);

            // Логируем успешное создание рейса
            logger.LogInformation("Рейс успешно создан: {@Flight}", flight);

            return flight;
        }

        // Получение рейса по ID
        public DepartureFlightGenerator GetFlight(string flightId)
        {
            return _flights.FirstOrDefault(f => f.FlightId == flightId);
        }

        // Получение всех рейсов
        public List<DepartureFlightGenerator> GetAllFlights()
        {
            return _flights;
        }
    }
}