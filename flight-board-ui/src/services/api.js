import axios from 'axios';

/**
 * Сохраняет настройки рейса на бэкенде.
 * @param {Object} settings - Настройки рейса (например, город отправления и время прилета/вылета).
 * @param {string} type - Тип рейса: 'arrival' (на прилет) или 'departure' (на вылет).
 * @returns {Promise<Object>} - Ответ от сервера.
 */
export const saveFlightSettings = async (settings, type) => {
    try {
        const endpoint = type === 'arrival' ? '/api/arrivalflight/create' : '/api/departureflight/create';
        const response = await axios.post(endpoint, settings);
        console.log('Настройки сохранены:', response.data);
        return response.data; // Возвращаем данные от сервера
    } catch (error) {
        console.error('Ошибка при сохранении настроек:', error);
        throw error; // Пробрасываем ошибку для обработки в компоненте
    }
};

/**
 * Получает список рейсов с бэкенда.
 * @param {string} type - Тип рейса: 'arrival' (на прилет) или 'departure' (на вылет).
 * @returns {Promise<Array>} - Список рейсов.
 */
export const fetchFlights = async (type) => {
    try {
        const endpoint = type === 'arrival' ? '/api/arrivalflight/all' : '/api/departureflight/all';
        const response = await axios.get(endpoint);
        console.log('Список рейсов:', response.data);
        return response.data; // Возвращаем данные от сервера
    } catch (error) {
        console.error('Ошибка при загрузке списка рейсов:', error);
        throw error; // Пробрасываем ошибку для обработки в компоненте
    }
};

/**
 * Получает список разрешенных городов с бэкенда.
 * @returns {Promise<Array>} - Список разрешенных городов.
 */
export const fetchAllowedCities = async () => {
    try {
        const response = await axios.get('/api/city/allowed');
        return response.data;
    } catch (error) {
        console.error('Ошибка при загрузке списка городов:', error);
        throw error;
    }
};