import React from 'react';
import { Routes, Route } from 'react-router-dom';
import { Container } from 'react-bootstrap';
import NavigationBar from './components/NavigationBar';
import Dashboard from './components/Dashboard';
import SalesReport from './components/reports/SalesReport';
import PerformanceReport from './components/reports/PerformanceReport';
import AnalyticsReport from './components/reports/AnalyticsReport';
import CircuitBreakerReport from './components/reports/CircuitBreakerReport';

function App() {
  return (
    <div className="App">
      <NavigationBar />
      <Container fluid className="py-4">
        <Routes>
          <Route path="/" element={<Dashboard />} />
          <Route path="/sales" element={<SalesReport />} />
          <Route path="/performance" element={<PerformanceReport />} />
          <Route path="/analytics" element={<AnalyticsReport />} />
          <Route path="/circuit-breaker" element={<CircuitBreakerReport />} />
        </Routes>
      </Container>
    </div>
  );
}

export default App;