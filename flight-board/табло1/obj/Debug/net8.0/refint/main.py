import copy
from enum import Enum
from fastapi import FastAPI, HTTPException
import uvicorn
from typing import List, Dict, Optional
from pydantic import BaseModel
import datetime
import requests
import json
import uuid
import threading
import time
from apscheduler.schedulers.background import BackgroundScheduler
from apscheduler.triggers.date import DateTrigger
import logging

# Настройка логирования
logging.basicConfig(level=logging.INFO, 
                   format='%(asctime)s - %(name)s - %(levelname)s - %(message)s')
logger = logging.getLogger("tablo")

app = FastAPI(title="Табло рейсов аэропорта", version="1.0.0")

# Конфигурация URL сервисов
# В реальном приложении эти URL должны быть получены из конфигурации
AIRCRAFT_SERVICE_URL = "https://airplane.reaport.ru"
ORCHESTRATOR_SERVICE_URL = "https://airport.reaport.ru"
REGISTRATION_SERVICE_URL = "https://registration.reaport.ru"
# Модели данных
class SeatClass(str, Enum):
    FIRST = "first"
    BUSINESS = "business"
    PREMIUM_ECONOMY = "premium_economy"
    ECONOMY = "economy"

class Seat(BaseModel):
    """Модель места в самолете"""
    seat_number: str
    seat_class: SeatClass

class GenerateRequest(BaseModel):
    flightId: str
    
class GenerateResponse(BaseModel):
    flightId: str
    aircraft_model: str
    passengers_count: int
    baggage_kg: int
    water_kg: int
    fuel_kg: int
    max_passengers: int
    max_baggage_kg: int
    max_water_kg: int
    max_fuel_kg: int
    seats: List[Seat]

class FlightDetails(BaseModel):
    flightId: str
    departureCity: str
    arrivalCity: str
    arrivalTime: datetime.datetime
    aircraftId: Optional[str] = None
    cityFrom: str
    cityTo: str
    ticketSalesStart: datetime.datetime
    registrationStartTime: datetime.datetime
    registrationEndTime: datetime.datetime
    boardingStartTime: datetime.datetime
    boardingEndTime: datetime.datetime
    departureTime: datetime.datetime
    isBoardingClosed: bool = False
    isRegistrationClosed: bool = False
    isTicketSalesClosed: bool = False

class ArrivalRequest(BaseModel):
    flightId: str

class ArrivalResponse(BaseModel):
    aircraftId: str

class DepartureRequest(BaseModel):
    aircraftId: str

class BoardingCompletedRequest(BaseModel):
    aircraftId: str

# Хранилище данных
flights_db: Dict[str, FlightDetails] = {}
aircraft_data: Dict[str, GenerateResponse] = {}
flight_to_aircraft: Dict[str, str] = {}  # flight_id -> aircraft_id
arrival_departure_pairs: Dict[str, str] = {}  # arrival_flight_id -> departure_flight_id

# Инициализация планировщика
scheduler = BackgroundScheduler()

# Функции взаимодействия с другими сервисами
def notify_aircraft_arrival(flight_id: str):
    """Уведомить сервис самолетов о прилете рейса"""
    logger.info(f"Уведомление о прилете рейса {flight_id}")
    
    try:
        response = requests.post(f"{AIRCRAFT_SERVICE_URL}/{flight_id}/landing",
                               timeout=50)
        
        # В учебном проекте - имитация ответа
        aircraft_id = response.json()["aircraftId"]
        
        # Обновление информации о рейсе
        flight = flights_db[flight_id]
        flight.aircraftId = aircraft_id
        
        # Обновление информации для соответствующего рейса на вылет
        if flight_id in arrival_departure_pairs:
            departure_id = arrival_departure_pairs[flight_id]
            if departure_id in flights_db:
                flights_db[departure_id].aircraftId = aircraft_id
        
        # Сохраняем связь между самолетом и рейсами
        flight_to_aircraft[flight_id] = aircraft_id
        
        logger.info(f"Рейс {flight_id} прибыл. Назначен самолет {aircraft_id}")
        
        # Планируем уведомление о вылете
        if flight_id in arrival_departure_pairs:
            departure_id = arrival_departure_pairs[flight_id]
            if departure_id in flights_db:
                departure_flight = flights_db[departure_id]
                schedule_departure_events(departure_flight)
    
    except Exception as e:
        logger.error(f"Ошибка при обработке прилета рейса {flight_id}: {e}")

def notify_boarding_completed(flight_id: str):
    """Уведомить оркестратор о завершении посадки"""
    logger.info(f"Уведомление о завершении посадки на рейс {flight_id}")
    
    try:
        flight = flights_db[flight_id]
        if flight.aircraftId is None:
            logger.warning(f"Невозможно завершить посадку для рейса {flight_id}: самолет не назначен")
            return
        
        # В реальном приложении - отправка запроса к оркестратору
        requests.post(f"{ORCHESTRATOR_SERVICE_URL}/{flight.aircraftId}/boarding/finish", 
                               timeout=50)
        
        flight.isBoardingClosed = True
        logger.info(f"Посадка на рейс {flight_id} закрыта. Самолет {flight.aircraftId}")
    
    except Exception as e:
        logger.error(f"Ошибка при закрытии посадки на рейс {flight_id}: {e}")

def notify_departure(flight_id: str):
    """Уведомить оркестратор о вылете самолета"""
    logger.info(f"Уведомление о вылете рейса {flight_id}")
    
    try:
        flight = flights_db[flight_id]
        if flight.aircraftId is None:
            logger.warning(f"Невозможно выполнить вылет для рейса {flight_id}: самолет не назначен")
            return
        
        if not flight.isBoardingClosed:
            logger.warning(f"Невозможно выполнить вылет для рейса {flight_id}: посадка не завершена")
            return
        
        # В реальном приложении - отправка запроса к оркестратору
        requests.post(f"{ORCHESTRATOR_SERVICE_URL}/{flight.aircraftId}/takeoff", 
                               timeout=50)
        
        logger.info(f"Рейс {flight_id} вылетел. Самолет {flight.aircraftId}")
    
    except Exception as e:
        logger.error(f"Ошибка при обработке вылета рейса {flight_id}: {e}")

# Функции планирования событий
def schedule_arrival_event(flight: FlightDetails):
    """Запланировать событие прилета самолета"""
    arrival_time = flight.arrivalTime
    logger.info(f"Планирование события прилета для рейса {flight.flightId} на {arrival_time}")
    
    # Добавляем задачу в планировщик
    scheduler.add_job(
        notify_aircraft_arrival,
        trigger=DateTrigger(run_date=arrival_time),
        args=[flight.flightId],
        id=f"arrival_{flight.flightId}"
    )

def schedule_departure_events(flight: FlightDetails):
    """Запланировать события для рейса на вылет (завершение посадки и вылет)"""
    boarding_end_time = flight.boardingEndTime
    departure_time = flight.departureTime
    open_registration_time = flight.registrationStartTime
    
    logger.info(f"Планирование события завершения посадки для рейса {flight.flightId} на {boarding_end_time}")
    
    scheduler.add_job(
        notify_registration_open,
        trigger=DateTrigger(run_date=open_registration_time),
        args=[flight.flightId],
        id=f"registration_open_{flight.flightId}"
    )
    
    scheduler.add_job(
        notify_boarding_completed,
        trigger=DateTrigger(run_date=boarding_end_time),
        args=[flight.flightId],
        id=f"boarding_end_{flight.flightId}"
    )
    
    logger.info(f"Планирование события вылета для рейса {flight.flightId} на {departure_time}")
    scheduler.add_job(
        notify_departure,
        trigger=DateTrigger(run_date=departure_time),
        args=[flight.flightId],
        id=f"departure_{flight.flightId}"
    )
    
def notify_registration_open(flight_id: str):
    """Уведомить регистрацию о начале регистрации"""
    logger.info(f"Уведомление о начале регистрации для рейса {flight_id}")
    
    try:
        requests.post(f"{REGISTRATION_SERVICE_URL}/{flight_id}/flights", 
            json={
                "flightId": flight_id,
                "flightName": flight_id,
                "endRegisterTime": flights_db[flight_id].registrationEndTime,
                "departureTime": flights_db[flight_id].departureTime,
                "startPlantingTime": flights_db[flight_id].boardingStartTime,
                "seatsAircraft": [
                    {"seatNumber": seat.seat_number, "seatClass": seat.seat_class.value}
                    for seat in aircraft_data.get(flight_id, {}).seats
                ] if flight_id in aircraft_data else []
            })
    except Exception as e:
        logger.error(f"Ошибка при уведомлении регистрации о начале регистрации для рейса {flight_id}: {e}")


# Сервисные функции
def generate_flight_pair(cityFrom: str, cityTo: str):
    """Генерирует пару рейсов (прилет и вылет)"""
    now = datetime.datetime.now()
    
    # Генерация ID для рейсов
    arrival_id = f"A{len(flights_db) + 1:03d}"
    departure_id = f"D{len(flights_db) + 1:03d}"
    
    # Время прилета: текущее время + 2 минуты
    arrival_time = now + datetime.timedelta(minutes=2)
    # Время вылета: время прилета + 10 минут
    departure_time = arrival_time + datetime.timedelta(minutes=10)
    
    # Создание рейса на прилет
    arrival_flight = FlightDetails(
        flightId=arrival_id,
        departureCity=cityFrom,
        arrivalCity="Мосипск",
        arrivalTime=arrival_time,
        aircraftId=None,
        cityFrom=cityFrom,
        cityTo="Мосипск",
        ticketSalesStart=now,
        registrationStartTime=now + datetime.timedelta(minutes=2),
        registrationEndTime=now + datetime.timedelta(minutes=5),
        boardingStartTime=now + datetime.timedelta(minutes=5),
        boardingEndTime=now + datetime.timedelta(minutes=8),
        departureTime=arrival_time,
        isBoardingClosed=False,
        isRegistrationClosed=False,
        isTicketSalesClosed=False
    )
    
    # Создание рейса на вылет
    departure_flight = FlightDetails(
        flightId=departure_id,
        departureCity="Мосипск",
        arrivalCity=cityTo,
        arrivalTime=departure_time + datetime.timedelta(minutes=2),  
        aircraftId=None,
        cityFrom="Мосипск",
        cityTo=cityTo,
        ticketSalesStart=now,
        registrationStartTime=now + datetime.timedelta(minutes=2),
        registrationEndTime=now + datetime.timedelta(minutes=5),
        boardingStartTime=now + datetime.timedelta(minutes=5),
        boardingEndTime=now + datetime.timedelta(minutes=8),
        departureTime=departure_time,
        isBoardingClosed=False,
        isRegistrationClosed=False,
        isTicketSalesClosed=False
    )
    
    # Сохранение рейсов в БД
    flights_db[arrival_id] = arrival_flight
    flights_db[departure_id] = departure_flight
    
    # Связываем рейсы прилета и вылета
    arrival_departure_pairs[arrival_id] = departure_id
    
    # Запрос к сервису самолета для генерации самолета
    try:
        aircraft_response = requests.post(f"{AIRCRAFT_SERVICE_URL}/generate",
                                          json={"flightId": arrival_id},
                                          timeout=50)
        
        # Сохраняем данные о самолете
        aircraft_data[arrival_id] = aircraft_response.json()
        
        # Планируем событие прилета
        schedule_arrival_event(arrival_flight)
        
        return arrival_flight, departure_flight
    except Exception as e:
        logger.error(f"Ошибка при генерации самолета: {e}")
        # Удаляем созданные рейсы в случае ошибки
        del flights_db[arrival_id]
        del flights_db[departure_id]
        del arrival_departure_pairs[arrival_id]
        raise HTTPException(status_code=500, detail="Ошибка при генерации рейсов")

class FlightGenerateRequest(BaseModel):
    cityFrom: str
    cityTo: str

# Маршруты API
@app.post("/generate-flights", response_model=List[FlightDetails])
async def generate_flights(request: FlightGenerateRequest):
    """Генерирует пару рейсов (прилет и вылет)"""
    arrival_flight, departure_flight = generate_flight_pair(request.cityFrom, request.cityTo)
    return [arrival_flight, departure_flight]

@app.get("/flights/{flight_id}", response_model=FlightDetails)
async def get_flight(flight_id: str):
    """Получает информацию о конкретном рейсе
    **НЕ ИСПОЛЬЗУЕТСЯ**
    """
    if flight_id not in flights_db:
        raise HTTPException(status_code=404, detail="Рейс не найден")
    return flights_db[flight_id]

# Модели для информации о покупке билетов
class SeatInfo(BaseModel):
    SeatClass: str
    SeatCount: int

class TicketPurchaseInfoResponse(BaseModel):
    FlightId: Optional[str] = None
    AircraftId: Optional[str] = None
    CityFrom: Optional[str] = None
    CityTo: Optional[str] = None
    AvailableSeats: Optional[List[SeatInfo]] = None
    Baggage: Optional[str] = None
    TakeoffDateTime: datetime.datetime
    LandingDateTime: datetime.datetime
    RegistrationStartTime: datetime.datetime

@app.get("/tickets", response_model=TicketPurchaseInfoResponse)
async def get_tickets(
    flightId: str,
    aircraftId: str,
    cityFrom: str,
    cityTo: str,
    seatClass: str
):
    """
    Получает информацию о рейсе с доступными местами для покупки билетов.
    
    Параметры:
    - flightId: ID рейса
    - aircraftId: ID самолета
    - cityFrom: Город отправления
    - cityTo: Город прибытия
    - seatClass: Класс места
    """
    logger.info(f"Запрос информации о билетах для рейса: flightId={flightId}, aircraftId={aircraftId}, "
                f"cityFrom={cityFrom}, cityTo={cityTo}, seatClass={seatClass}")
    
    # Проверяем, существует ли рейс
    if flightId not in flights_db:
        raise HTTPException(status_code=404, detail=f"Рейс {flightId} не найден")
    
    # Получаем информацию о рейсе
    flight = flights_db[flightId]
    
    # Получаем данные о самолете для этого рейса
    aircraft_flight_id = flightId
    
    # Для рейса на вылет ищем соответствующий рейс на прилет
    if flight.cityFrom == "Мосипск":  # Это рейс на вылет
        for arrival_id, departure_id in arrival_departure_pairs.items():
            if departure_id == flightId:
                aircraft_flight_id = arrival_id
                break
    
    # Получаем данные о самолете
    aircraft = aircraft_data[aircraft_flight_id]
    # Подсчитываем количество мест в каждом классе
    seat_counts = {
        SeatClass.FIRST: 0,
        SeatClass.BUSINESS: 0,
        SeatClass.PREMIUM_ECONOMY: 0,
        SeatClass.ECONOMY: 0
    }
    
    # Считаем количество мест по классам
    for seat in aircraft.seats:
        seat_counts[seat.seat_class] += 1
    
    
    # Формируем список доступных мест
    available_seats = [SeatInfo(
        SeatClass=seat.seat_class,
        SeatCount=seat_counts[seat.seat_class]
    ) for seat in aircraft.seats ]
    
    
    # Получаем информацию о багаже
    max_baggage = f"{aircraft.max_baggage_kg} кг"
    
    # Формируем ответ
    response = TicketPurchaseInfoResponse(
        FlightId=flight.flightId,
        AircraftId=flight.aircraftId or aircraftId,
        CityFrom=flight.cityFrom,
        CityTo=flight.cityTo,
        AvailableSeats=available_seats,
        Baggage=max_baggage,
        TakeoffDateTime=flight.departureTime,
        LandingDateTime=flight.arrivalTime,
        RegistrationStartTime=flight.registrationStartTime
    )
    
    return response

@app.get("/tickets/available", response_model=List[TicketPurchaseInfoResponse])
async def get_available_flights():
    """
    Получает список всех доступных рейсов на вылет с информацией для покупки билетов.
    """
    logger.info("Запрос списка доступных рейсов")
    
    # Получаем все рейсы на вылет из Мосипска
    available_flights = [flight for flight in flights_db.values() 
                         if flight.cityFrom == "Мосипск" and not flight.isTicketSalesClosed]
    
    if not available_flights:
        return []
    
    result = []
    
    # Для каждого рейса формируем информацию о доступных билетах
    for flight in available_flights:
        # Для рейса на вылет ищем соответствующий рейс на прилет, чтобы получить данные о самолете
        aircraft_flight_id = None
        for arrival_id, departure_id in arrival_departure_pairs.items():
            if departure_id == flight.flightId:
                aircraft_flight_id = arrival_id
                break
        
        if not aircraft_flight_id or aircraft_flight_id not in aircraft_data:
            # Если данные о самолете не найдены, пропускаем рейс
            continue
        
        # Получаем данные о самолете
        aircraft = aircraft_data[aircraft_flight_id]
        
        # Подсчитываем количество мест по классам
        # Формируем словарь для подсчета количества мест по классам
        class_seats = {}
        
        for seat in aircraft.seats:
            seat_class = seat.seat_class.value
            # Преобразуем название класса в формат для клиента
            display_class = seat_class.replace("_", " ").title()  # "premium_economy" -> "Premium Economy"
            
            if display_class not in class_seats:
                class_seats[display_class] = 0
            
            class_seats[display_class] += 1
        
        # Формируем список доступных мест по классам
        available_seats = [
            SeatInfo(SeatClass=class_name, SeatCount=count)
            for class_name, count in class_seats.items()
        ]
        
        # Получаем информацию о багаже
        max_baggage = f"{aircraft.max_baggage_kg} кг"
        
        # Формируем ответ для текущего рейса
        flight_info = TicketPurchaseInfoResponse(
            FlightId=flight.flightId,
            AircraftId=flight.aircraftId or "",
            CityFrom=flight.cityFrom,
            CityTo=flight.cityTo,
            AvailableSeats=available_seats,
            Baggage=max_baggage,
            TakeoffDateTime=flight.departureTime,
            LandingDateTime=flight.arrivalTime,
            RegistrationStartTime=flight.registrationStartTime
        )
        
        result.append(flight_info)
    
    return result

@app.get("/api/flight/all") # отправление
async def get_all_flights():
    """Получает список всех рейсов"""
    result = []
    moscow_tz = pytz.timezone('Europe/Moscow')
    
    for flight in flights_db.values():
        if flight.cityFrom == "Мосипск":
            # Конвертируем все времена в московский часовой пояс
            flight_copy = copy.deepcopy(flight)
            if flight_copy.departureTime:
                flight_copy.departureTime = flight_copy.departureTime.astimezone(moscow_tz)
            if flight_copy.arrivalTime:
                flight_copy.arrivalTime = flight_copy.arrivalTime.astimezone(moscow_tz)
            if flight_copy.registrationStartTime:
                flight_copy.registrationStartTime = flight_copy.registrationStartTime.astimezone(moscow_tz)
            result.append(flight_copy)
    return result

@app.get("/api/arrivalflight/all") # прибытие
async def get_flight():
    """Получает информацию о конкретном рейсе"""
    result = []
    moscow_tz = pytz.timezone('Europe/Moscow')
    
    for flight in flights_db.values():
        if flight.cityTo == "Мосипск":
            # Конвертируем все времена в московский часовой пояс
            flight_copy = copy.deepcopy(flight)
            if flight_copy.departureTime:
                flight_copy.departureTime = flight_copy.departureTime.astimezone(moscow_tz)
            if flight_copy.arrivalTime:
                flight_copy.arrivalTime = flight_copy.arrivalTime.astimezone(moscow_tz)
            if flight_copy.registrationStartTime:
                flight_copy.registrationStartTime = flight_copy.registrationStartTime.astimezone(moscow_tz)
            result.append(flight_copy)
    return result



@app.on_event("startup")
def startup_event():
    """Запускает планировщик при старте приложения"""
    scheduler.start()
    logger.info("Планировщик задач запущен")

@app.on_event("shutdown")
def shutdown_event():
    """Останавливает планировщик при остановке приложения"""
    scheduler.shutdown()
    logger.info("Планировщик задач остановлен")

def main():
    """Запуск приложения"""
    uvicorn.run(app, host="0.0.0.0", port=8000)

if __name__ == "__main__":
    main()
