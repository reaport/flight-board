using System;
using System.Timers;
using AirportManagement.Services;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;
using AirportManagement.Models;

namespace AirportManagement
{
    public class DepartureFlightGenerator : IDisposable
    {
        private static int _lastFlightNumber = 0;
        private Timer _timer;
        private readonly ILogger<DepartureFlightGenerator> _logger;
        private readonly FlightSettings _settings;
        private readonly IRegistrationService _registrationService; // Добавляем сервис
        private readonly IAircraftService _aircraftService; // Добавляем AircraftService

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

        public DepartureFlightGenerator(string destination, ILogger<DepartureFlightGenerator> logger, FlightSettings settings, IRegistrationService registrationService, IAircraftService aircraftService)
        {
            if (string.IsNullOrEmpty(destination))
            {
                throw new ArgumentException("Город назначения не может быть пустым.", nameof(destination));
            }

            _lastFlightNumber++;
            FlightId = "KU" + _lastFlightNumber.ToString("D3");
            CityTo = destination; // Используем выбранный город
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _registrationService = registrationService ?? throw new ArgumentNullException(nameof(registrationService));
            _aircraftService = aircraftService ?? throw new ArgumentNullException(nameof(aircraftService));

            InitializeFlightSchedule(DateTime.Now);
            _logger.LogInformation($"Рейс {FlightId} создан. Направление: {CityTo}. Время вылета: {DepartureTime}");

            _timer = new Timer(1000); // Таймер с интервалом 1 секунда
            _timer.Elapsed += OnTimedEvent;
            _timer.AutoReset = true;
            _timer.Enabled = true;

            OpenTicketSales();
            _aircraftService = aircraftService;
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

        private async void OnTimedEvent(object? sender, ElapsedEventArgs e)
        {
            DateTime currentTime = DateTime.Now;

            // Регистрация
            if (!IsRegistrationClosed && currentTime >= RegistrationStartTime && !_hasLoggedRegistrationOpened)
            {
                _hasLoggedRegistrationOpened = true;
                IsRegistrationClosed = false;
                _logger.LogInformation($"Регистрация по рейсу {FlightId} открыта. Время начала: {RegistrationStartTime}");

                try
                {
                    // Получаем данные о самолете
                    var aircraftData = await _aircraftService.GetAircraftDataAsync(AircraftId);

                    // Преобразуем данные о самолете в список мест
                    var seats = aircraftData.Seats.Select(s => new Seat
                    {
                        SeatNumber = s.SeatNumber,
                        SeatClass = s.SeatClass
                    }).ToList();

                    // Формируем запрос для модуля регистрации
                    var request = new FlightRegistrationResponse
                    {
                        FlightId = FlightId,
                        FlightName = $"KU-{_lastFlightNumber}",
                        EndRegisterTime = RegistrationEndTime,
                        DepartureTime = DepartureTime,
                        StartPlantingTime = BoardingStartTime,
                        SeatsAircraft = seats
                    };

                    // Отправляем данные модулю регистрации
                    await _registrationService.SendFlightRegistrationDataAsync(request); // Исправлено имя метода
                    _logger.LogInformation($"Данные для рейса {FlightId} успешно отправлены модулю регистрации.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Ошибка при отправке данных для рейса {FlightId}.");
                }
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

        private List<Seat> GetSeats()
        {
            // Пример данных о местах
            return new List<Seat>
        {
            new Seat { SeatNumber = "1A", SeatClass = "business" },
            new Seat { SeatNumber = "1B", SeatClass = "business" },
            new Seat { SeatNumber = "2A", SeatClass = "economy" },
            new Seat { SeatNumber = "2B", SeatClass = "economy" }
        };
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