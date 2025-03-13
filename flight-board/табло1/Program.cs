using Serilog;
using AirportManagement;
using AirportManagement.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

// Настройка Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console() // Логи в консоль
    .WriteTo.File( // Логи в файл
        path: "Logs/log.txt", // Путь к файлу логов
        rollingInterval: RollingInterval.Day, // Новый файл каждый день
        retainedFileCountLimit: 7, // Хранить логи за последние 7 дней
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Использование Serilog для логгирования
    builder.Host.UseSerilog();

    builder.Services.AddHttpClient();

    // Регистрация сервисов
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Регистрация FlightService
    builder.Services.AddSingleton<FlightService>();
    builder.Services.AddScoped<ArrivalFlightService>();
    builder.Services.AddSingleton<ArrivalFlightService>();
    builder.Services.AddHttpClient<IAircraftService, AircraftService>(client =>
    {
        client.BaseAddress = new Uri("https://aircraft.reaport.ru"); // Замените на реальный URL
    }); builder.Services.AddHttpClient<IRegistrationService, RegistrationService>();


    builder.Services.AddSingleton<CityService>();

    // Регистрация настроек FlightSettings
    builder.Services.Configure<FlightSettings>(builder.Configuration.GetSection("FlightSettings"));
    builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<FlightSettings>>().Value);

    // Настройка CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
    });

    var app = builder.Build();

    // Настройка middleware
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    app.UseCors("AllowAll");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Приложение завершилось с ошибкой.");
}
finally
{
    Log.CloseAndFlush(); // Закрыть и сохранить логи
}