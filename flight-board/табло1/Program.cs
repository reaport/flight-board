using Serilog;
using AirportManagement;
using AirportManagement.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

// ��������� Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console() // ���� � �������
    .WriteTo.File( // ���� � ����
        path: "Logs/log.txt", // ���� � ����� �����
        rollingInterval: RollingInterval.Day, // ����� ���� ������ ����
        retainedFileCountLimit: 7, // ������� ���� �� ��������� 7 ����
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ������������� Serilog ��� ������������
    builder.Host.UseSerilog();

    builder.Services.AddHttpClient();

    // ����������� ��������
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // ����������� FlightService
    builder.Services.AddSingleton<FlightService>();
    builder.Services.AddScoped<ArrivalFlightService>();
    builder.Services.AddSingleton<ArrivalFlightService>();
    builder.Services.AddHttpClient<IAircraftService, AircraftService>(client =>
    {
        client.BaseAddress = new Uri("https://airplane.reaport.ru"); // �������� �� �������� URL
    }); 
    builder.Services.AddHttpClient<IRegistrationService, RegistrationService>();
    builder.Services.AddHttpClient<OrchestratorService>();
    builder.Services.AddHttpClient<AircraftModuleService>();

    builder.Services.AddSingleton<CityService>();

    // ����������� �������� FlightSettings
    builder.Services.Configure<FlightSettings>(builder.Configuration.GetSection("FlightSettings"));
    builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<FlightSettings>>().Value);

    // ��������� CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAllOrigins", policy =>
        {
            policy.AllowAnyOrigin() // ��������� ������� � ������ ���������
                  .AllowAnyMethod() // ��������� ��� HTTP-������ (GET, POST, PUT � �.�.)
                  .AllowAnyHeader(); // ��������� ��� ���������
        });
    });

    var app = builder.Build();

    // ��������� middleware
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    app.UseCors("AllowAllOrigins");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "���������� ����������� � �������.");
}
finally
{
    Log.CloseAndFlush(); // ������� � ��������� ����
}