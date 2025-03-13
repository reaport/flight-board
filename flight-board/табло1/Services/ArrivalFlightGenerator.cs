using System;
using System.Timers;
using Timer = System.Timers.Timer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace AirportManagement
{
    public class ArrivalFlight : IDisposable
    {
        private static int _lastFlightId = 0; // Для генерации уникального номера рейса
        private Timer _timer;
        private readonly ILogger<ArrivalFlight> _logger;
        private readonly IConfiguration _config;

        public string FlightId { get; } // Уникальный идентификатор рейса
        public string DepartureCity { get; } // Город отправления
        public string ArrivalCity { get; } = "Мосипск"; // Город прилета (всегда Мосипск)
        public DateTime ArrivalTime { get; set; } // Время прилета
        public bool HasLanded { get; private set; } // Флаг, указывающий, что самолет приземлился

        public ArrivalFlight(string departureCity, ILogger<ArrivalFlight> logger, IConfiguration config, DateTime arrivalTime)
        {
            if (string.IsNullOrEmpty(departureCity))
            {
                throw new ArgumentException("Город отправления не может быть пустым.", nameof(departureCity));
            }

            // Генерация уникального FlightId
            _lastFlightId++;
            FlightId = "SU" + _lastFlightId.ToString("D3");

            DepartureCity = departureCity;
            ArrivalTime = arrivalTime;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));

            // Получаем список разрешенных городов из конфига
            var allowedOrigins = _config.GetSection("AllowedOrigins").Get<List<string>>();

            // Проверяем, что город отправления есть в списке разрешенных
            if (allowedOrigins == null || !allowedOrigins.Contains(departureCity))
            {
                throw new ArgumentException("Город отправления не найден в списке разрешенных городов.", nameof(departureCity));
            }

            _logger.LogInformation($"Рейс на прилет {FlightId} создан. Отправление из: {DepartureCity}, Прилет в: {ArrivalCity}, Время прилета: {ArrivalTime}");

            // Настройка таймера
            _timer = new Timer(1000); // 1 секунда реального времени = 1 минута в имитации
            _timer.Elapsed += OnTimedEvent;
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        private void OnTimedEvent(object? sender, ElapsedEventArgs e)
        {
            DateTime currentTime = DateTime.Now;

            // Проверяем, наступило ли время прилета
            if (!HasLanded && currentTime >= ArrivalTime)
            {
                Land(); // Самолет приземляется
            }
        }

        public void Land()
        {
            HasLanded = true;
            _logger.LogInformation($"Рейс {FlightId} приземлился в {ArrivalCity}.");
            _timer.Stop(); // Останавливаем таймер
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}