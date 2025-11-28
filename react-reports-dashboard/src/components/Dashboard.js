import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Card, Alert, Badge, Button } from 'react-bootstrap';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts';
import apiService from '../services/apiService';

const Dashboard = () => {
  const [healthStatus, setHealthStatus] = useState(null);
  const [quickMetrics, setQuickMetrics] = useState({
    salesData: null,
    performanceData: null,
    analyticsData: null
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    loadDashboardData();
  }, []);

  const loadDashboardData = async () => {
    setLoading(true);
    setError(null);
    
    try {
      // Load quick metrics in parallel
      const [health, sales, performance, analytics] = await Promise.all([
        apiService.healthCheck(),
        apiService.getSalesReport(3, 0), // Quick 3-month sales
        apiService.getPerformanceReport(30, 0), // 30-second performance
        apiService.getAnalyticsReport(100, 0) // Quick 100 records
      ]);

      setHealthStatus(health);
      setQuickMetrics({
        salesData: sales,
        performanceData: performance,
        analyticsData: analytics
      });
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const formatCurrency = (value) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(value);
  };

  const getPerformanceBadge = (responseTime) => {
    if (responseTime < 100) return <Badge bg="success">Excellent</Badge>;
    if (responseTime < 500) return <Badge bg="warning">Good</Badge>;
    return <Badge bg="danger">Needs Attention</Badge>;
  };

  if (loading) {
    return (
      <Container>
        <div className="loading-spinner">
          <div className="spinner-border text-primary" role="status">
            <span className="visually-hidden">Loading...</span>
          </div>
        </div>
      </Container>
    );
  }

  return (
    <Container>
      <Row className="mb-4">
        <Col>
          <h1 className="display-4 mb-3">Reports Dashboard</h1>
          <p className="lead">Real-time performance monitoring and business intelligence</p>
        </Col>
        <Col xs="auto">
          <Button variant="outline-primary" onClick={loadDashboardData}>
            <i className="bi bi-arrow-clockwise me-2"></i>
            Refresh Data
          </Button>
        </Col>
      </Row>

      {error && (
        <Alert variant="danger" dismissible onClose={() => setError(null)}>
          <Alert.Heading>Error Loading Dashboard</Alert.Heading>
          {error}
        </Alert>
      )}

      {/* Health Status */}
      <Row className="mb-4">
        <Col>
          <Card className="metric-card">
            <Card.Body>
              <Row>
                <Col>
                  <h5>System Health</h5>
                  <h3>
                    {healthStatus?.status === 'Healthy' ? (
                      <Badge bg="success">🟢 Healthy</Badge>
                    ) : (
                      <Badge bg="danger">🔴 Unhealthy</Badge>
                    )}
                  </h3>
                </Col>
                <Col xs="auto">
                  <div className="text-end">
                    <small>API Endpoint</small>
                    <br />
                    <code>localhost:8081</code>
                  </div>
                </Col>
              </Row>
            </Card.Body>
          </Card>
        </Col>
      </Row>

      {/* Quick Metrics */}
      <Row className="mb-4">
        <Col md={4}>
          <Card className="report-card h-100">
            <Card.Body>
              <div className="d-flex justify-content-between align-items-start mb-3">
                <h5 className="card-title">💰 Sales Overview</h5>
                {quickMetrics.salesData && getPerformanceBadge(quickMetrics.salesData.processingTimeMs)}
              </div>
              
              {quickMetrics.salesData ? (
                <>
                  <h3 className="text-primary">
                    {formatCurrency(quickMetrics.salesData.totalRevenue)}
                  </h3>
                  <p className="text-muted mb-2">
                    {quickMetrics.salesData.totalRecords} transactions
                  </p>
                  <small className="text-muted">
                    Processed in {quickMetrics.salesData.processingTimeMs}ms
                  </small>
                </>
              ) : (
                <div className="text-center py-3">
                  <div className="spinner-border spinner-border-sm" role="status"></div>
                </div>
              )}
            </Card.Body>
          </Card>
        </Col>

        <Col md={4}>
          <Card className="report-card h-100">
            <Card.Body>
              <div className="d-flex justify-content-between align-items-start mb-3">
                <h5 className="card-title">⚡ Performance</h5>
                {quickMetrics.performanceData && getPerformanceBadge(quickMetrics.performanceData.processingTimeMs)}
              </div>
              
              {quickMetrics.performanceData ? (
                <>
                  <h3 className="text-success">
                    {quickMetrics.performanceData.averageCpuUsage?.toFixed(1)}%
                  </h3>
                  <p className="text-muted mb-2">
                    Memory: {quickMetrics.performanceData.averageMemoryUsage?.toFixed(1)}%
                  </p>
                  <small className="text-muted">
                    {quickMetrics.performanceData.totalDataPoints} data points
                  </small>
                </>
              ) : (
                <div className="text-center py-3">
                  <div className="spinner-border spinner-border-sm" role="status"></div>
                </div>
              )}
            </Card.Body>
          </Card>
        </Col>

        <Col md={4}>
          <Card className="report-card h-100">
            <Card.Body>
              <div className="d-flex justify-content-between align-items-start mb-3">
                <h5 className="card-title">📊 Analytics</h5>
                {quickMetrics.analyticsData && getPerformanceBadge(quickMetrics.analyticsData.processingTimeMs)}
              </div>
              
              {quickMetrics.analyticsData ? (
                <>
                  <h3 className="text-info">
                    {quickMetrics.analyticsData.totalTransactions?.toLocaleString()}
                  </h3>
                  <p className="text-muted mb-2">
                    Avg: {formatCurrency(quickMetrics.analyticsData.averageTransactionValue)}
                  </p>
                  <small className="text-muted">
                    Peak: {formatCurrency(quickMetrics.analyticsData.peakTransactionValue)}
                  </small>
                </>
              ) : (
                <div className="text-center py-3">
                  <div className="spinner-border spinner-border-sm" role="status"></div>
                </div>
              )}
            </Card.Body>
          </Card>
        </Col>
      </Row>

      {/* Performance Chart */}
      {quickMetrics.performanceData?.dataPoints && (
        <Row>
          <Col>
            <Card className="chart-container">
              <Card.Body>
                <h5 className="mb-3">System Performance Overview</h5>
                <ResponsiveContainer width="100%" height={300}>
                  <LineChart data={quickMetrics.performanceData.dataPoints.slice(0, 20)}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="timestamp" />
                    <YAxis />
                    <Tooltip />
                    <Legend />
                    <Line 
                      type="monotone" 
                      dataKey="cpuUsage" 
                      stroke="#8884d8" 
                      name="CPU Usage (%)"
                      strokeWidth={2}
                    />
                    <Line 
                      type="monotone" 
                      dataKey="memoryUsage" 
                      stroke="#82ca9d" 
                      name="Memory Usage (%)"
                      strokeWidth={2}
                    />
                  </LineChart>
                </ResponsiveContainer>
              </Card.Body>
            </Card>
          </Col>
        </Row>
      )}

      {/* Quick Actions */}
      <Row className="mt-4">
        <Col>
          <Card>
            <Card.Body>
              <h5 className="mb-3">Quick Actions</h5>
              <div className="d-flex gap-2 flex-wrap">
                <Button variant="outline-primary" href="/sales">
                  View Detailed Sales Report
                </Button>
                <Button variant="outline-success" href="/performance">
                  Performance Analysis
                </Button>
                <Button variant="outline-info" href="/analytics">
                  Advanced Analytics
                </Button>
                <Button variant="outline-warning" href="/circuit-breaker">
                  Circuit Breaker Monitor
                </Button>
                <Button 
                  variant="outline-success" 
                  onClick={async () => {
                    try {
                      const result = await apiService.testCors();
                      alert(`CORS Test Successful!\\nMessage: ${result.Message}\\nTimestamp: ${result.Timestamp}`);
                    } catch (error) {
                      alert(`CORS Test Failed!\\nError: ${error.message}`);
                    }
                  }}
                >
                  Test CORS Connection
                </Button>
                <Button variant="outline-secondary" href="http://localhost:16686" target="_blank">
                  Open Jaeger Tracing
                </Button>
              </div>
            </Card.Body>
          </Card>
        </Col>
      </Row>
    </Container>
  );
};

export default Dashboard;