import React, { useState, useEffect } from 'react';
import axios from 'axios';

const FlightStatus = ({ flightId }) => {
    const [status, setStatus] = useState('');

    useEffect(() => {
        const interval = setInterval(() => {
            axios.get(`/api/flights/${flightId}/status`)
                .then(response => {
                    setStatus(response.data.status);
                })
                .catch(error => {
                    console.error('Ошибка при получении статуса рейса:', error);
                });
        }, 1000); // Обновление каждую секунду

        return () => clearInterval(interval); // Очистка интервала при размонтировании
    }, [flightId]);

    return (
        <div>
            <h2>Статус рейса {flightId}</h2>
            <p>{status}</p>
        </div>
    );
};

export default FlightStatus;