using AirportManagement.Models;
using System.Text.Json;

public interface IAircraftService
{
    Task<AircraftData> GetAircraftDataAsync(string aircraftId);
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

    public async Task<AircraftData> GetAircraftDataAsync(string aircraftId)
    {
        try
        {
            // Выполняем POST-запрос к ручке /generate
            var response = await _httpClient.PostAsync("/generate", null); // Если ручка POST
            response.EnsureSuccessStatusCode();

            // Десериализуем ответ
            var aircraftGenerationResponse = await response.Content.ReadFromJsonAsync<AircraftGenerationResponse>();

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

    // Метод для уведомления о прилете
    public async Task<string> NotifyLandingAsync(string flightId)
    {
        try
        {
            var url = $"/{flightId}/landing";
            var response = await _httpClient.PostAsync(url, null);
            response.EnsureSuccessStatusCode();

            // Читаем ответ и возвращаем ID самолета
            var responseContent = await response.Content.ReadAsStringAsync();
            var aircraftId = JsonSerializer.Deserialize<Dictionary<string, string>>(responseContent)?["aircraft_id"];

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