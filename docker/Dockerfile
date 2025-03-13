FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Копируем файлы проекта и восстанавливаем зависимости
COPY ./flight-board/табло1/*.csproj ./
RUN dotnet restore

# Копируем все файлы и собираем приложение
COPY ./flight-board/табло1/ ./
RUN dotnet publish -c Release -o out ./табло1.csproj

# Собираем финальный образ
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Копируем файлы конфигурации
COPY --from=build /app/out .
COPY .//flight-board/табло1/appsettings*.json ./

ENTRYPOINT ["dotnet", "табло1.dll"]