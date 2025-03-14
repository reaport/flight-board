using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AirportManagement.Services
{
    public class AircraftModuleService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AircraftModuleService> _logger;

        public AircraftModuleService(HttpClient httpClient, ILogger<AircraftModuleService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        // Метод для отправки события прилета
        public async Task<string> NotifyLandingAsync(string flightId)
        {
            try
            {
                // Формируем URL для отправки запроса
                var url = $"/{flightId}/landing";

                // Отправляем POST-запрос
                var response = await _httpClient.PostAsync(url, null);

                // Проверяем успешность запроса
                response.EnsureSuccessStatusCode();

                // Читаем ответ и десериализуем его
                var responseContent = await response.Content.ReadAsStringAsync();
                var landingResponse = JsonSerializer.Deserialize<LandingResponse>(responseContent);

                _logger.LogInformation($"Уведомление о прилете для рейса {flightId} отправлено успешно. ID самолета: {landingResponse?.AircraftId}");
                return landingResponse?.AircraftId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при отправке уведомления о прилете для рейса {flightId}.");
                throw;
            }
        }

        // Модель для ответа от модуля самолета
        private class LandingResponse
        {
            public string AircraftId { get; set; }
        }
    }
}