using AirportManagement.Models;
using System.Text;
using System.Text.Json;

public interface IAircraftService
{
    Task<AircraftData> GetAircraftDataAsync(string flightId); // Используем flightId вместо aircraftId
    Task<string> NotifyLandingAsync(string flightId);
}

public class AircraftService : IAircraftService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AircraftService> _logger;

    public AircraftService(HttpClient httpClient, ILogger<AircraftService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<AircraftData> GetAircraftDataAsync(string flightId)
    {
        try
        {
            // Создаем тело запроса с flightId
            var requestBody = new { flightId = flightId };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            _logger.LogInformation("Отправка запроса на /generate с телом: {RequestBody}", JsonSerializer.Serialize(requestBody));

            // Выполняем POST-запрос к ручке /generate
            var response = await _httpClient.PostAsync("/generate", content);
            response.EnsureSuccessStatusCode();

            // Читаем и логируем ответ
            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Ответ от /generate: {ResponseContent}", responseContent);

            // Десериализуем ответ
            var aircraftGenerationResponse = JsonSerializer.Deserialize<AircraftGenerationResponse>(responseContent);

            // Преобразуем ответ в AircraftData
            var aircraftData = new AircraftData
            {
                AircraftId = aircraftGenerationResponse.FlightId, // Используем FlightId из ответа
                Seats = aircraftGenerationResponse.Seats // Используем список мест из ответа
            };

            return aircraftData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении данных о самолете.");
            throw;
        }
    }

    public async Task<string> NotifyLandingAsync(string flightId)
    {
        try
        {
            // Добавляем flightId в тело запроса
            var requestBody = new { flightId = flightId };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var url = $"/{flightId}/landing";
            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            // Читаем ответ
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseData = JsonSerializer.Deserialize<Dictionary<string, string>>(responseContent);

            // Извлекаем aircraft_id
            var aircraftId = responseData?["aircraft_id"];

            _logger.LogInformation($"Уведомление о прилете для рейса {flightId} отправлено успешно. ID самолета: {aircraftId}");
            return aircraftId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка при отправке уведомления о прилете для рейса {flightId}.");
            throw;
        }
    }
}