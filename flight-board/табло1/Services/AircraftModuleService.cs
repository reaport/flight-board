using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Устанавливаем базовый адрес для HttpClient
            _httpClient.BaseAddress = new Uri("https://airplane.reaport.ru");

            // Добавляем заголовки, если необходимо
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            // _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "your_token");
        }

        // Метод для отправки события прилета
        public async Task<string> NotifyLandingAsync(string flightId)
        {
            try
            {
                // Формируем относительный URI
                var url = $"/{flightId}/landing";

                // Создаем тело запроса (если необходимо)
                var content = new StringContent(JsonSerializer.Serialize(new { FlightId = flightId }), Encoding.UTF8, "application/json");

                // Отправляем POST-запрос
                var response = await _httpClient.PostAsync(url, content);

                // Читаем ответ
                var responseContent = await response.Content.ReadAsStringAsync();

                // Проверяем успешность запроса
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Ошибка при отправке запроса. Код: {response.StatusCode}. Ответ: {responseContent}");
                    throw new HttpRequestException($"Ошибка при отправке запроса. Код: {response.StatusCode}. Ответ: {responseContent}");
                }

                // Десериализуем ответ
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