using AirportManagement.Models;
using System.Text.Json;

public interface IAircraftService
{
    Task<AircraftData> GetAircraftDataAsync(string aircraftId);
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
            // Используем правильный URL: airplane.reaport.ru
            var response = await _httpClient.GetAsync($"https://airplane.reaport.ru/generate");
            response.EnsureSuccessStatusCode();

            var aircraftData = await response.Content.ReadFromJsonAsync<AircraftData>();
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