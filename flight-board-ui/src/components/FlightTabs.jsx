import React, { useState } from 'react';
import DepartureFlightForm from './FlightSettingsForm';
import ArrivalFlightForm from './ArrivalFlightForm';
import FlightList from './FlightList';

const FlightTabs = () => {
    const [activeTab, setActiveTab] = useState('departure'); // По умолчанию активна вкладка вылетающих рейсов

    return (
        <div>
            <div>
                <button onClick={() => setActiveTab('departure')}>Вылетающие рейсы</button>
                <button onClick={() => setActiveTab('arrival')}>Прилетающие рейсы</button>
            </div>

            {activeTab === 'departure' && (
                <div>
                    <h2>Вылетающие рейсы</h2>
                    <DepartureFlightForm />
                    <FlightList type="departure" />
                </div>
            )}

            {activeTab === 'arrival' && (
                <div>
                    <h2>Прилетающие рейсы</h2>
                    <ArrivalFlightForm />
                    <FlightList type="arrival" />
                </div>
            )}
        </div>
    );
};

export default FlightTabs;