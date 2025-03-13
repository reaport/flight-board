using AirportManagement.Models;
using AirportManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace AirportManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketDataController : ControllerBase
    {
        private readonly FlightService _flightService;
        private readonly ILogger<TicketController> _logger;

        public TicketDataController(FlightService flightService, ILogger<TicketController> logger)
        {
            _flightService = flightService;
            _logger = logger;
        }

        [HttpPost("tickets")]
        public ActionResult SaveTicketPurchaseInfo([FromBody] TicketPurchaseSaveRequest request)
        {
            try
            {
                // Проверка входных данных
                if (request.PurchasedSeats == null || request.PurchasedSeats.Count == 0)
                {
                    _logger.LogWarning("PurchasedSeats is required.");
                    return BadRequest(new ErrorResponse { ErrorCode = 600, Message = "PurchasedSeats is required" });
                }

                if (string.IsNullOrEmpty(request.Baggage))
                {
                    _logger.LogWarning("Baggage information is required.");
                    return BadRequest(new ErrorResponse { ErrorCode = 601, Message = "Baggage information is required" });
                }

                // Сохранение данных о покупке билетов
                // Здесь можно добавить логику для сохранения данных в базу данных или другой сервис
                _logger.LogInformation("Данные о покупке билетов успешно сохранены.");

                // Возврат успешного ответа
                return Ok(new
                {
                    status = "success",
                    message = "Ticket purchase info saved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении данных о покупке билетов.");
                return StatusCode(500, new ErrorResponse { ErrorCode = 500, Message = "InternalServerError" });
            }
        }
    }
}