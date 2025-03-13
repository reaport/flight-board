import React from 'react';
import FlightTabs from './components/FlightTabs';
import './App.css';

const App = () => {
    return (
        <div className="App">
            <h1>Админка модуля табло</h1>
            {/* Вкладки для вылетающих и прилетающих рейсов */}
            <FlightTabs />
        </div>
    );
};

export default App;