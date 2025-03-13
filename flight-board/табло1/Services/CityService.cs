using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace AirportManagement.Services
{
    public class CityService
    {
        private readonly List<string> _allowedCities;

        public CityService(IConfiguration configuration)
        {
            // Загружаем список городов из конфигурации
            _allowedCities = configuration.GetSection("AllowedOrigins").Get<List<string>>();
        }

        // Метод для получения списка городов
        public List<string> GetAllowedCities()
        {
            return _allowedCities;
        }

        // Метод для получения случайного города из списка
        public string GetRandomCity()
        {
            if (_allowedCities == null || _allowedCities.Count == 0)
            {
                throw new InvalidOperationException("Список городов пуст.");
            }

            var random = new Random();
            int index = random.Next(_allowedCities.Count);
            return _allowedCities[index];
        }
    }
}