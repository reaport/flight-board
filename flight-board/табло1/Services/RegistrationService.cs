using AirportManagement.Models;

public interface IRegistrationService
{
    Task SendFlightRegistrationDataAsync(FlightRegistrationResponse request);
}

public class RegistrationService : IRegistrationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RegistrationService> _logger;

    public RegistrationService(HttpClient httpClient, ILogger<RegistrationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task SendFlightRegistrationDataAsync(FlightRegistrationResponse request)
    {
        try
        {
            var url = "https://register.reaport.ru/flights"; // Основной URL
            var response = await _httpClient.PostAsJsonAsync(url, request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Ошибка при отправке данных: {response.StatusCode}");
                throw new HttpRequestException($"Ошибка при отправке данных: {response.StatusCode}");
            }

            _logger.LogInformation("Данные о рейсе успешно отправлены.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке данных о рейсе.");
            throw;
        }
    }
}