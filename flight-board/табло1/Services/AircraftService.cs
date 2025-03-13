using AirportManagement.Models;

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
            var response = await _httpClient.GetAsync($"/api/aircraft/{aircraftId}");
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
}