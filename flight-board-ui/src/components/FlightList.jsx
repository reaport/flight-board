import React, { useState, useEffect } from 'react';
import axios from 'axios';

const FlightList = ({ type }) => {
    const [flights, setFlights] = useState([]);

    // Загрузка списка рейсов при монтировании компонента
    useEffect(() => {
        const loadFlights = async () => {
            try {
                const endpoint = type === 'departure' ? '/api/flight/all' : '/api/arrivalflight/all';
                const response = await axios.get(endpoint);
                setFlights(response.data);
                console.log('Данные рейсов:', response.data); // Логируем данные для отладки
            } catch (error) {
                console.error('Ошибка при загрузке списка рейсов:', error);
            }
        };
        loadFlights();
    }, [type]); // Зависимость от type, чтобы список обновлялся при изменении типа

    return (
        <div>
            <h3>{type === 'departure' ? 'Список вылетающих рейсов' : 'Список рейсов на прилет'}</h3>
            {flights.length === 0 ? (
                <p>Рейсов нет.</p>
            ) : (
                <ul>
                    {flights.map((flight) => (
                        <li key={flight.flightId}>
                            {type === 'departure' ? (
                                <>
                                    Рейс {flight.flightId}: {flight.cityFrom} → {flight.cityTo} (Вылет: {new Date(flight.departureTime).toLocaleTimeString()})
                                </>
                            ) : (
                                <>
                                    Рейс {flight.flightId}: {flight.departureCity} → {flight.arrivalCity} (Прилет: {new Date(flight.arrivalTime).toLocaleTimeString()})
                                </>
                            )}
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
};

export default FlightList;