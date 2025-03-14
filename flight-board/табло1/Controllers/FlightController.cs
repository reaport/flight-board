using AirportManagement.Dto;
using AirportManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AirportManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlightController : ControllerBase
    {
        private readonly FlightService _flightService;
        private readonly ILogger<FlightController> _logger;
        private readonly FlightSettings _settings;
        private readonly ILoggerFactory _loggerFactory;

        public FlightController(
            FlightService flightService,
            ILogger<FlightController> logger,
            FlightSettings settings,
            ILoggerFactory loggerFactory)
        {
            _flightService = flightService;
            _logger = logger;
            _settings = settings;
            _loggerFactory = loggerFactory;
        }

        [HttpPost("create")]
        public async Task<ActionResult<FlightDto>> CreateDepartureFlight([FromBody] string destination)
        {
            try
            {
                // Создаем логгер для DepartureFlightGenerator
                var departureFlightLogger = _loggerFactory.CreateLogger<DepartureFlightGenerator>();

                // Создаем рейс (город назначения берется из аргумента или CityService)
                var flight = await _flightService.CreateFlightAsync(destination, departureFlightLogger, _settings);

                // Преобразуем рейс в DTO
                var flightDto = new FlightDto
                {
                    FlightId = flight.FlightId,
                    CityFrom = flight.CityFrom,
                    CityTo = flight.CityTo,
                    TicketSalesStart = flight.TicketSalesStart,
                    RegistrationStartTime = flight.RegistrationStartTime,
                    RegistrationEndTime = flight.RegistrationEndTime,
                    BoardingStartTime = flight.BoardingStartTime,
                    BoardingEndTime = flight.BoardingEndTime,
                    DepartureTime = flight.DepartureTime,
                    IsBoardingClosed = flight.IsBoardingClosed,
                    IsRegistrationClosed = flight.IsRegistrationClosed,
                    IsTicketSalesClosed = flight.IsTicketSalesClosed
                };

                return Ok(flightDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании рейса.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{flightId}/status")]
        public ActionResult<string> GetFlightStatus(string flightId)
        {
            var flight = _flightService.GetFlight(flightId);
            if (flight == null)
            {
                return NotFound("Рейс не найден.");
            }

            if (flight.IsBoardingClosed && DateTime.Now >= flight.DepartureTime)
            {
                return Ok("Рейс вылетел.");
            }
            else if (flight.IsBoardingClosed)
            {
                return Ok("Подготовка ко взлету.");
            }
            else if (flight.IsRegistrationClosed)
            {
                return Ok("Посадка открыта.");
            }
            else if (flight.IsTicketSalesClosed)
            {
                return Ok("Регистрация открыта.");
            }
            else
            {
                return Ok("Продажа билетов открыта.");
            }
        }

        [HttpGet("all")]
        public ActionResult<List<FlightDto>> GetAllFlights()
        {
            try
            {
                // Получаем все рейсы из сервиса
                var flights = _flightService.GetAllFlights();

                // Преобразуем каждый рейс в DTO
                var flightDtos = flights.Select(flight => new FlightDto
                {
                    FlightId = flight.FlightId,
                    CityFrom = flight.CityFrom,
                    CityTo = flight.CityTo,
                    TicketSalesStart = flight.TicketSalesStart,
                    RegistrationStartTime = flight.RegistrationStartTime,
                    RegistrationEndTime = flight.RegistrationEndTime,
                    BoardingStartTime = flight.BoardingStartTime,
                    BoardingEndTime = flight.BoardingEndTime,
                    DepartureTime = flight.DepartureTime,
                    IsBoardingClosed = flight.IsBoardingClosed,
                    IsRegistrationClosed = flight.IsRegistrationClosed,
                    IsTicketSalesClosed = flight.IsTicketSalesClosed
                }).ToList();

                return Ok(flightDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка рейсов.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}