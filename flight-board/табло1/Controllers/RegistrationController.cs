﻿using AirportManagement.Models;
using AirportManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AirportManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrationController : ControllerBase
    {
        private readonly FlightService _flightService;
        private readonly IAircraftService _aircraftService;
        private readonly IRegistrationService _registrationService; // Добавлено поле
        private readonly ILogger<RegistrationController> _logger;

        public RegistrationController(
            FlightService flightService,
            IAircraftService aircraftService,
            IRegistrationService registrationService, // Добавлен параметр
            ILogger<RegistrationController> logger)
        {
            _flightService = flightService;
            _aircraftService = aircraftService;
            _registrationService = registrationService; // Инициализация поля
            _logger = logger;
        }

        [HttpGet("register")]
        public async Task<ActionResult<FlightRegistrationResponse>> GetRegistrationInfo(
            [FromQuery] string flightId,
            [FromQuery] string cityFrom,
            [FromQuery] string cityTo)
        {
            try
            {
                // Проверка входных данных
                if (string.IsNullOrEmpty(flightId))
                {
                    _logger.LogWarning("FlightId is required.");
                    return BadRequest(new ErrorResponse { ErrorCode = 100, Message = "FlightId is required" });
                }

                if (string.IsNullOrEmpty(cityFrom) || string.IsNullOrEmpty(cityTo))
                {
                    _logger.LogWarning("CityFrom and CityTo are required.");
                    return BadRequest(new ErrorResponse { ErrorCode = 102, Message = "CityFrom and CityTo are required" });
                }

                // Получение информации о рейсе
                var flight = _flightService.GetFlight(flightId);
                if (flight == null)
                {
                    _logger.LogWarning($"Рейс {flightId} не найден.");
                    return NotFound(new ErrorResponse { ErrorCode = 101, Message = "Invalid FlightID" });
                }

                // Проверка, что рейс соответствует запросу
                if (flight.CityFrom != cityFrom || flight.CityTo != cityTo)
                {
                    _logger.LogWarning($"Рейс {flightId} не соответствует указанным городам.");
                    return BadRequest(new ErrorResponse { ErrorCode = 103, Message = "Flight does not match the specified cities" });
                }

                // Получение данных о самолете
                var aircraftData = await _aircraftService.GetAircraftDataAsync(flight.AircraftId);

                // Формирование ответа
                var response = new FlightRegistrationResponse
                {
                    FlightId = flight.FlightId,
                    StartRegisterTime = flight.RegistrationStartTime,
                    StartPlantingTime = flight.RegistrationEndTime,
                    DepartureTime = flight.DepartureTime,
                    SeatsAircraft = aircraftData.Seats
                };

                // Формирование запроса для отправки в модуль регистрации
                var registrationRequest = new FlightRegistrationResponse // Используем FlightRegistrationRequest
                {
                    FlightId = flight.FlightId,
                    FlightName = $"SU-{flight.FlightId}", // Пример формирования имени рейса
                    EndRegisterTime = flight.RegistrationEndTime,
                    DepartureTime = flight.DepartureTime,
                    StartPlantingTime = flight.RegistrationEndTime, // Используем RegistrationEndTime как StartPlantingTime
                    SeatsAircraft = aircraftData.Seats
                };

                // Отправка данных в модуль регистрации
                await _registrationService.SendFlightRegistrationDataAsync(registrationRequest);

                _logger.LogInformation($"Информация о регистрации для рейса {flightId} успешно получена и отправлена.");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении информации о регистрации.");
                return StatusCode(500, new ErrorResponse { ErrorCode = 500, Message = "InternalServerError" });
            }
        }
    }
}