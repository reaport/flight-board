import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { fetchAllowedCities } from '../services/api';

const ArrivalFlightForm = ({ onSubmit }) => {
    const [settings, setSettings] = useState({
        departureCity: '', // Город отправления
        arrivalTimeOffset: 5, // Время до прилета (минуты)
    });

    const [cities, setCities] = useState([]);

    // Загрузка списка городов при монтировании компонента
    useEffect(() => {
        const loadCities = async () => {
            try {
                const data = await fetchAllowedCities();
                setCities(data);
            } catch (error) {
                console.error('Ошибка при загрузке списка городов:', error);
            }
        };
        loadCities();
    }, []);

    const handleChange = (e) => {
        const { name, value } = e.target;
        setSettings((prev) => ({
            ...prev,
            [name]: name === 'arrivalTimeOffset' ? parseInt(value, 10) : value, // Для числовых полей и строк
        }));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        try {
            console.log('Отправка настроек на бэкенд:', settings); // Логируем настройки
            const response = await axios.post('/api/arrivalflight/create', settings);
            if (response.data) {
                alert('Рейс на прилет успешно создан!');
                onSubmit(); // Обновляем список рейсов
            } else {
                alert('Ошибка при создании рейса на прилет.');
            }
        } catch (error) {
            console.error('Ошибка при создании рейса на прилет:', error);
            alert('Ошибка при создании рейса на прилет.');
        }
    };

    return (
        <form onSubmit={handleSubmit}>
            <div>
                <label>
                    Город отправления:
                    <select
                        name="departureCity"
                        value={settings.departureCity}
                        onChange={handleChange}
                        required
                    >
                        <option value="">Выберите город</option>
                        {cities.map((city) => (
                            <option key={city} value={city}>
                                {city}
                            </option>
                        ))}
                    </select>
                </label>
            </div>
            <div>
                <label>
                    Время до прилета (минуты):
                    <input
                        type="number"
                        name="arrivalTimeOffset"
                        value={settings.arrivalTimeOffset}
                        onChange={handleChange}
                        required
                    />
                </label>
            </div>
            <button type="submit">Создать рейс на прилет</button>
        </form>
    );
};

export default ArrivalFlightForm;