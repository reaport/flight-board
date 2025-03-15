public class OrchestratorService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OrchestratorService> _logger;

    public OrchestratorService(HttpClient httpClient, ILogger<OrchestratorService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        _httpClient.BaseAddress = new Uri("https://orchestrator.reaport.ru");
    }

    // Метод для уведомления о завершении посадки
    public async Task NotifyBoardingFinishedAsync(string aircraftId)
    {
        try
        {
            var url = $"/ad_board/{aircraftId}/boarding/finish";
            var response = await _httpClient.PostAsync(url, null);
            response.EnsureSuccessStatusCode();
            _logger.LogInformation($"Уведомление о завершении посадки для самолета {aircraftId} отправлено успешно.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка при отправке уведомления о завершении посадки для самолета {aircraftId}.");
            throw;
        }
    }

    // Метод для уведомления о взлете
    public async Task NotifyTakeoffAsync(string aircraftId)
    {
        try
        {
            var url = $"/ad_board/{aircraftId}/takeoff";
            var response = await _httpClient.PostAsync(url, null);
            response.EnsureSuccessStatusCode();
            _logger.LogInformation($"Уведомление о взлете для самолета {aircraftId} отправлено успешно.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка при отправке уведомления о взлете для самолета {aircraftId}.");
            throw;
        }
    }
}