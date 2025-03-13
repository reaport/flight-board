using AirportManagement.Models;
using AirportManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace AirportManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AircraftDataController : ControllerBase
    {
        private readonly FlightService _flightService;
        private readonly ILogger<AircraftDataController> _logger;

        public AircraftDataController(FlightService flightService, ILogger<AircraftDataController> logger)
        {
            _flightService = flightService;
            _logger = logger;
        }

        [HttpPost("aircraft-departure")]
        public ActionResult SaveAircraftDepartureInfo([FromBody] AircraftDepartureSaveRequest request)
        {
            try
            {
                // Проверка входных данных
                if (string.IsNullOrEmpty(request.AircraftId))
                {
                    _logger.LogWarning("AircraftId is required.");
                    return BadRequest(new ErrorResponse { ErrorCode = 500, Message = "AircraftId is required" });
                }

                if (request.AvailableSeats == null || request.AvailableSeats.Count == 0)
                {
                    _logger.LogWarning("Invalid available seats data.");
                    return BadRequest(new ErrorResponse { ErrorCode = 501, Message = "Invalid available seats data" });
                }

                if (string.IsNullOrEmpty(request.Baggage))
                {
                    _logger.LogWarning("Baggage information is required.");
                    return BadRequest(new ErrorResponse { ErrorCode = 502, Message = "Baggage information is required" });
                }

                // Сохранение данных о вылетающем самолете
                // Здесь можно добавить логику для сохранения данных в базу данных или другой сервис
                _logger.LogInformation($"Данные о вылетающем самолете {request.AircraftId} успешно сохранены.");

                // Возврат успешного ответа
                return Ok(new
                {
                    status = "success",
                    message = "Aircraft departure info saved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении данных о вылетающем самолете.");
                return StatusCode(500, new ErrorResponse { ErrorCode = 500, Message = "InternalServerError" });
            }
        }
    }
}