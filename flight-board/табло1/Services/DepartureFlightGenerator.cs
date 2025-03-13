using System;
using System.Timers;
using AirportManagement.Services;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace AirportManagement
{
    public class DepartureFlightGenerator : IDisposable
    {
        private static int _lastFlightNumber = 0;
        private Timer _timer;
        private readonly ILogger<DepartureFlightGenerator> _logger;
        private readonly FlightSettings _settings;

        public string FlightId { get; }
        public string CityFrom { get; } = "Мосипск";
        public string CityTo { get; }
        public DateTime TicketSalesStart { get; private set; }
        public DateTime RegistrationStartTime { get; private set; }
        public DateTime RegistrationEndTime { get; private set; }
        public DateTime BoardingStartTime { get; private set; }
        public DateTime BoardingEndTime { get; private set; }
        public DateTime DepartureTime { get; private set; }
        public string AircraftId { get; set; } // Добавляем AircraftId

        public bool IsTicketSalesClosed { get; private set; }
        public bool IsRegistrationClosed { get; private set; }
        public bool IsBoardingClosed { get; private set; }

        private bool _hasLoggedTicketSalesOpened = false;
        private bool _hasLoggedRegistrationOpened = false;
        private bool _hasLoggedBoardingClosed = false;
        private bool _hasLoggedDeparture = false;

        public DepartureFlightGenerator(string destination, ILogger<DepartureFlightGenerator> logger, FlightSettings settings)
        {
            if (string.IsNullOrEmpty(destination))
            {
                throw new ArgumentException("Город назначения не может быть пустым.", nameof(destination));
            }

            _lastFlightNumber++;
            FlightId = "SU" + _lastFlightNumber.ToString("D3");
            CityTo = destination; // Используем выбранный город
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

            InitializeFlightSchedule(DateTime.Now);
            _logger.LogInformation($"Рейс {FlightId} создан. Направление: {CityTo}. Время вылета: {DepartureTime}");

            _timer = new Timer(1000); // Таймер с интервалом 1 секунда
            _timer.Elapsed += OnTimedEvent;
            _timer.AutoReset = true;
            _timer.Enabled = true;

            OpenTicketSales();
        }

        private void InitializeFlightSchedule(DateTime currentTime)
        {
            // Используем настройки для расчета времени каждого события
            TicketSalesStart = currentTime;
            RegistrationStartTime = TicketSalesStart.AddMinutes(_settings.PurchaseToRegistrationMinutes);
            RegistrationEndTime = RegistrationStartTime.AddMinutes(_settings.RegistrationToBoardingMinutes);
            BoardingStartTime = RegistrationEndTime;
            BoardingEndTime = BoardingStartTime.AddMinutes(_settings.BoardingToEndBoardingMinutes);
            DepartureTime = BoardingEndTime.AddMinutes(_settings.EndBoardingToDepartureMinutes);

            _logger.LogInformation($"Расписание рейса {FlightId} инициализировано: " +
                                  $"Начало продажи билетов: {TicketSalesStart}, " +
                                  $"Начало регистрации: {RegistrationStartTime}, " +
                                  $"Закрытие регистрации: {RegistrationEndTime}, " +
                                  $"Начало посадки: {BoardingStartTime}, " +
                                  $"Закрытие посадки: {BoardingEndTime}, " +
                                  $"Время вылета: {DepartureTime}");
        }

        private void OnTimedEvent(object? sender, ElapsedEventArgs e)
        {
            DateTime currentTime = DateTime.Now;

            // Продажа билетов
            if (!IsTicketSalesClosed && currentTime >= TicketSalesStart && !_hasLoggedTicketSalesOpened)
            {
                _hasLoggedTicketSalesOpened = true;
                IsTicketSalesClosed = false;
                _logger.LogInformation($"Продажа билетов по рейсу {FlightId} открыта. Время начала: {TicketSalesStart}");
            }

            // Закрытие продажи билетов и открытие регистрации
            if (!IsTicketSalesClosed && currentTime >= RegistrationStartTime)
            {
                CloseTicketSales();
            }

            // Регистрация
            if (!IsRegistrationClosed && currentTime >= RegistrationStartTime && !_hasLoggedRegistrationOpened)
            {
                _hasLoggedRegistrationOpened = true;
                IsRegistrationClosed = false;
                _logger.LogInformation($"Регистрация по рейсу {FlightId} открыта. Время начала: {RegistrationStartTime}");
            }

            // Закрытие регистрации и открытие посадки
            if (!IsRegistrationClosed && currentTime >= RegistrationEndTime)
            {
                CloseRegistration();
            }

            // Посадка
            if (!IsBoardingClosed && currentTime >= BoardingStartTime)
            {
                CloseRegistration();
                IsBoardingClosed = true;
                _logger.LogInformation($"Посадка по рейсу {FlightId} открыта. Время начала: {BoardingStartTime}");
            }

            // Закрытие посадки и подготовка ко взлету
            if (IsBoardingClosed && currentTime >= BoardingEndTime && !_hasLoggedBoardingClosed)
            {
                CloseBoarding();
                _hasLoggedBoardingClosed = true;
                _logger.LogInformation($"Посадка по рейсу {FlightId} закончена. Время окончания: {BoardingEndTime}");
            }

            // Вылет
            if (IsBoardingClosed && currentTime >= DepartureTime && !_hasLoggedDeparture)
            {
                Depart();
                _hasLoggedDeparture = true;
                _logger.LogInformation($"Рейс {FlightId} вылетел в {CityTo}. Время вылета: {DepartureTime}");
            }
        }

        public void CloseTicketSales()
        {
            IsTicketSalesClosed = true;
            _logger.LogInformation($"Закрытие продажи билетов для рейса {FlightId}.");
        }

        public void CloseRegistration()
        {
            IsRegistrationClosed = true;
            _logger.LogInformation($"Закрытие регистрации для рейса {FlightId}.");
        }

        public void CloseBoarding()
        {
            IsBoardingClosed = true;
            _logger.LogInformation($"Закрытие посадки для рейса {FlightId}.");
        }

        public void Depart()
        {
            _logger.LogInformation($"Рейс {FlightId} вылетел в пункт назначения: {CityTo}");
            _timer.Stop();
        }

        public void OpenTicketSales()
        {
            IsTicketSalesClosed = false;
            _logger.LogInformation($"Продажа билетов по рейсу {FlightId} открыта.");
        }

        public void OpenRegistration()
        {
            IsRegistrationClosed = false;
            _logger.LogInformation($"Регистрация по рейсу {FlightId} открыта.");
        }

        public void OpenBoarding()
        {
            IsBoardingClosed = false;
            _logger.LogInformation($"Посадка по рейсу {FlightId} открыта.");
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}