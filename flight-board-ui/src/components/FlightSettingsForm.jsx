import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { fetchAllowedCities } from '../services/api';

const FlightSettingsForm = ({ onSubmit }) => {
    const [settings, setSettings] = useState({
        purchaseToRegistrationMinutes: 120,
        registrationToBoardingMinutes: 60,
        boardingToEndBoardingMinutes: 30,
        endBoardingToDepartureMinutes: 15,
        destination: '', // Добавляем поле для выбора города
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
            [name]: parseInt(value, 10) || value, // Для числовых полей и строк
        }));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        try {
            console.log('Отправка настроек на бэкенд:', settings); // Логируем настройки
            const response = await axios.post('/api/flightsettings/save', settings);
            if (response.data.success) {
                alert('Настройки успешно сохранены!');
                onSubmit(); // Обновляем список рейсов
            } else {
                alert('Ошибка при сохранении настроек.');
            }
        } catch (error) {
            console.error('Ошибка при сохранении настроек:', error);
            alert('Ошибка при сохранении настроек.');
        }
    };

    return (
        <form onSubmit={handleSubmit}>
            <div>
                <label>
                    Время между покупкой билетов и регистрацией (минуты):
                    <input
                        type="number"
                        name="purchaseToRegistrationMinutes"
                        value={settings.purchaseToRegistrationMinutes}
                        onChange={handleChange}
                    />
                </label>
            </div>
            <div>
                <label>
                    Время между регистрацией и посадкой (минуты):
                    <input
                        type="number"
                        name="registrationToBoardingMinutes"
                        value={settings.registrationToBoardingMinutes}
                        onChange={handleChange}
                    />
                </label>
            </div>
            <div>
                <label>
                    Время между посадкой и окончанием посадки (минуты):
                    <input
                        type="number"
                        name="boardingToEndBoardingMinutes"
                        value={settings.boardingToEndBoardingMinutes}
                        onChange={handleChange}
                    />
                </label>
            </div>
            <div>
                <label>
                    Время между окончанием посадки и вылетом (минуты):
                    <input
                        type="number"
                        name="endBoardingToDepartureMinutes"
                        value={settings.endBoardingToDepartureMinutes}
                        onChange={handleChange}
                    />
                </label>
            </div>
            <div>
                <label>
                    Город назначения:
                    <select
                        name="destination"
                        value={settings.destination}
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
            <button type="submit">Сохранить настройки</button>
        </form>
    );
};

export default FlightSettingsForm;