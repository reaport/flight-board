import React, { useEffect, useState } from 'react';
import axios from 'axios';

const ArrivalFlightList = () => {
    const [flights, setFlights] = useState([]);

    // Загружаем список рейсов при монтировании компонента
    useEffect(() => {
        const fetchFlights = async () => {
            try {
                const response = await axios.get('/api/arrivalflight/all');
                setFlights(response.data);
                console.log('Данные рейсов:', response.data); // Логируем данные для отладки
            } catch (error) {
                console.error('Ошибка при загрузке списка рейсов:', error);
            }
        };
        fetchFlights();
    }, []);

    return (
        <div>
            <h2>Список рейсов на прилет</h2>
            {flights.length === 0 ? (
                <p>Рейсов на прилет нет.</p>
            ) : (
                <ul>
                    {flights.map((flight) => (
                        <li key={flight.flightId}>
                            <strong>Рейс:</strong> {flight.flightId} <br />
                            <strong>Город отправления:</strong> {flight.departureCity} <br />
                            <strong>Город прилета:</strong> {flight.arrivalCity} <br />
                            <strong>Время прилета:</strong> {new Date(flight.arrivalTime).toLocaleString()} <br />
                            <strong>Статус:</strong> {flight.hasLanded ? 'Приземлился' : 'В пути'}
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
};

export default ArrivalFlightList;