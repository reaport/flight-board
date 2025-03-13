import axios from 'axios';

/**
 * ��������� ��������� ����� �� �������.
 * @param {Object} settings - ��������� ����� (��������, ����� ����������� � ����� �������/������).
 * @param {string} type - ��� �����: 'arrival' (�� ������) ��� 'departure' (�� �����).
 * @returns {Promise<Object>} - ����� �� �������.
 */
export const saveFlightSettings = async (settings, type) => {
    try {
        const endpoint = type === 'arrival' ? '/api/arrivalflight/create' : '/api/departureflight/create';
        const response = await axios.post(endpoint, settings);
        console.log('��������� ���������:', response.data);
        return response.data; // ���������� ������ �� �������
    } catch (error) {
        console.error('������ ��� ���������� ��������:', error);
        throw error; // ������������ ������ ��� ��������� � ����������
    }
};

/**
 * �������� ������ ������ � �������.
 * @param {string} type - ��� �����: 'arrival' (�� ������) ��� 'departure' (�� �����).
 * @returns {Promise<Array>} - ������ ������.
 */
export const fetchFlights = async (type) => {
    try {
        const endpoint = type === 'arrival' ? '/api/arrivalflight/all' : '/api/departureflight/all';
        const response = await axios.get(endpoint);
        console.log('������ ������:', response.data);
        return response.data; // ���������� ������ �� �������
    } catch (error) {
        console.error('������ ��� �������� ������ ������:', error);
        throw error; // ������������ ������ ��� ��������� � ����������
    }
};

/**
 * �������� ������ ����������� ������� � �������.
 * @returns {Promise<Array>} - ������ ����������� �������.
 */
export const fetchAllowedCities = async () => {
    try {
        const response = await axios.get('/api/city/allowed');
        return response.data;
    } catch (error) {
        console.error('������ ��� �������� ������ �������:', error);
        throw error;
    }
};