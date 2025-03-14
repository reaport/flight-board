using AirportManagement.Dto;
using AirportManagement.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AirportManagement.Services
{
    public class FlightService
    {
        private readonly ConcurrentBag<DepartureFlightGenerator> _flights = new ConcurrentBag<DepartureFlightGenerator>();
        private readonly CityService _cityService;
        private readonly IRegistrationService _registrationService;
        private readonly IAircraftService _aircraftService;
        private readonly OrchestratorService _orchestratorService;
        private readonly ILogger<FlightService> _logger;

        public FlightService(
            CityService cityService,
            IRegistrationService registrationService,
            IAircraftService aircraftService,
            OrchestratorService orchestratorService,
            ILogger<FlightService> logger)
        {
            _cityService = cityService;
            _registrationService = registrationService;
            _aircraftService = aircraftService;
            _orchestratorService = orchestratorService;
            _logger = logger;
        }

        public async Task<DepartureFlightGenerator> CreateFlightAsync(
            string destination,
            ILogger<DepartureFlightGenerator> logger,
            FlightSettings settings)
        {
            if (string.IsNullOrEmpty(destination))
            {
                throw new ArgumentException("Город назначения не может быть пустым.", nameof(destination));
            }

            logger.LogInformation("Создание рейса с городом назначения: {Destination}", destination);

            AircraftData aircraftData = null;
            try
            {
                aircraftData = await _aircraftService.GetAircraftDataAsync("dummyId");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при получении данных о самолете.");
                throw;
            }

            var flight = new DepartureFlightGenerator(
                destination,
                logger,
                settings,
                _registrationService,
                _aircraftService,
                _orchestratorService,
                aircraftData
            );

            _flights.Add(flight);

            logger.LogInformation("Рейс успешно создан: {@Flight}", flight);

            return flight;
        }

        public DepartureFlightGenerator GetFlight(string flightId)
        {
            return _flights.FirstOrDefault(f => f.FlightId == flightId);
        }

        public List<DepartureFlightGenerator> GetAllFlights()
        {
            return _flights.ToList();
        }
    }
}