import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Card, Alert, Badge, Button, Form, Table } from 'react-bootstrap';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer, BarChart, Bar } from 'recharts';
import apiService from '../../services/apiService';

const CircuitBreakerReport = () => {
  const [circuitStatus, setCircuitStatus] = useState(null);
  const [testResults, setTestResults] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [loadTestConfig, setLoadTestConfig] = useState({
    requests: 10,
    failureRate: 0.5
  });

  useEffect(() => {
    loadCircuitBreakerStatus();
  }, []);

  const loadCircuitBreakerStatus = async () => {
    try {
      const status = await apiService.getCircuitBreakerStatus();
      setCircuitStatus(status);
    } catch (err) {
      setError(err.message);
    }
  };

  const testDatabaseCircuitBreaker = async (simulateFailure = false) => {
    setLoading(true);
    try {
      const result = await apiService.testDatabaseCircuitBreaker(simulateFailure);
      setTestResults(prev => [
        ...prev,
        {
          id: Date.now(),
          type: 'Database',
          simulateFailure,
          result,
          timestamp: new Date().toLocaleTimeString()
        }
      ]);
      await loadCircuitBreakerStatus();
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const testApiCircuitBreaker = async (simulateFailure = false) => {
    setLoading(true);
    try {
      const result = await apiService.testApiCircuitBreaker(simulateFailure);
      setTestResults(prev => [
        ...prev,
        {
          id: Date.now(),
          type: 'API',
          simulateFailure,
          result,
          timestamp: new Date().toLocaleTimeString()
        }
      ]);
      await loadCircuitBreakerStatus();
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const runLoadTest = async () => {
    setLoading(true);
    try {
      const result = await apiService.simulateLoad(loadTestConfig.requests, loadTestConfig.failureRate);
      setTestResults(prev => [
        ...prev,
        {
          id: Date.now(),
          type: 'Load Test',
          simulateFailure: false,
          result,
          timestamp: new Date().toLocaleTimeString()
        }
      ]);
      await loadCircuitBreakerStatus();
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const resetCounters = async () => {
    try {
      await apiService.resetCircuitBreakerCounters();
      setTestResults([]);
      await loadCircuitBreakerStatus();
    } catch (err) {
      setError(err.message);
    }
  };

  const getCircuitStateColor = (state) => {
    switch (state?.toLowerCase()) {
      case 'closed':
        return 'success';
      case 'open':
        return 'danger';
      case 'half-open':
      case 'halfopen':
        return 'warning';
      default:
        return 'secondary';
    }
  };

  const getResultStatusBadge = (result) => {
    if (result.CircuitState === 'Open') {
      return <Badge bg="danger">Circuit Open</Badge>;
    }
    if (result.Error) {
      return <Badge bg="danger">Error</Badge>;
    }
    if (result.Message?.includes('successful')) {
      return <Badge bg="success">Success</Badge>;
    }
    return <Badge bg="secondary">Unknown</Badge>;
  };

  return (
    <Container>
      <Row className="mb-4">
        <Col>
          <h1 className="display-5">🔄 Circuit Breaker Monitor</h1>
          <p className="lead">Monitor and test circuit breaker states: Open, Closed, and Half-Open</p>
        </Col>
      </Row>

      {error && (
        <Alert variant="danger" dismissible onClose={() => setError(null)}>
          <Alert.Heading>Circuit Breaker Error</Alert.Heading>
          {error}
        </Alert>
      )}

      {/* Circuit Breaker Status */}
      {circuitStatus && (
        <Row className="mb-4">
          <Col md={6}>
            <Card className="h-100">
              <Card.Body>
                <h5 className="mb-3">
                  <i className="bi bi-database me-2"></i>
                  Database Circuit Breaker
                </h5>
                <div className="mb-3">
                  <h6>Configuration:</h6>
                  <ul className="list-unstyled">
                    <li><strong>Failure Ratio:</strong> {circuitStatus.DatabaseCircuitBreaker?.Configuration?.FailureRatio || '50%'}</li>
                    <li><strong>Sampling Duration:</strong> {circuitStatus.DatabaseCircuitBreaker?.Configuration?.SamplingDuration || '30 seconds'}</li>
                    <li><strong>Minimum Throughput:</strong> {circuitStatus.DatabaseCircuitBreaker?.Configuration?.MinimumThroughput || 3}</li>
                    <li><strong>Break Duration:</strong> {circuitStatus.DatabaseCircuitBreaker?.Configuration?.BreakDuration || '30 seconds'}</li>
                  </ul>
                </div>
                <div className="d-flex gap-2">
                  <Button
                    variant="outline-success"
                    size="sm"
                    onClick={() => testDatabaseCircuitBreaker(false)}
                    disabled={loading}
                  >
                    Test Success
                  </Button>
                  <Button
                    variant="outline-danger"
                    size="sm"
                    onClick={() => testDatabaseCircuitBreaker(true)}
                    disabled={loading}
                  >
                    Test Failure
                  </Button>
                </div>
              </Card.Body>
            </Card>
          </Col>
          <Col md={6}>
            <Card className="h-100">
              <Card.Body>
                <h5 className="mb-3">
                  <i className="bi bi-cloud me-2"></i>
                  API Circuit Breaker
                </h5>
                <div className="mb-3">
                  <h6>Configuration:</h6>
                  <ul className="list-unstyled">
                    <li><strong>Failure Ratio:</strong> {circuitStatus.ApiCircuitBreaker?.Configuration?.FailureRatio || '30%'}</li>
                    <li><strong>Sampling Duration:</strong> {circuitStatus.ApiCircuitBreaker?.Configuration?.SamplingDuration || '20 seconds'}</li>
                    <li><strong>Minimum Throughput:</strong> {circuitStatus.ApiCircuitBreaker?.Configuration?.MinimumThroughput || 5}</li>
                    <li><strong>Break Duration:</strong> {circuitStatus.ApiCircuitBreaker?.Configuration?.BreakDuration || '15 seconds'}</li>
                  </ul>
                </div>
                <div className="d-flex gap-2">
                  <Button
                    variant="outline-success"
                    size="sm"
                    onClick={() => testApiCircuitBreaker(false)}
                    disabled={loading}
                  >
                    Test Success
                  </Button>
                  <Button
                    variant="outline-danger"
                    size="sm"
                    onClick={() => testApiCircuitBreaker(true)}
                    disabled={loading}
                  >
                    Test Failure
                  </Button>
                </div>
              </Card.Body>
            </Card>
          </Col>
        </Row>
      )}

      {/* Statistics */}
      {circuitStatus?.Statistics && (
        <Row className="mb-4">
          <Col md={3}>
            <Card className="metric-card h-100">
              <Card.Body className="text-center">
                <h6 className="text-light">Total Requests</h6>
                <h3>{circuitStatus.Statistics.TotalRequests?.toLocaleString()}</h3>
              </Card.Body>
            </Card>
          </Col>
          <Col md={3}>
            <Card className="metric-card h-100">
              <Card.Body className="text-center">
                <h6 className="text-light">Total Failures</h6>
                <h3>{circuitStatus.Statistics.TotalFailures?.toLocaleString()}</h3>
              </Card.Body>
            </Card>
          </Col>
          <Col md={3}>
            <Card className="metric-card h-100">
              <Card.Body className="text-center">
                <h6 className="text-light">Success Rate</h6>
                <h3>{circuitStatus.Statistics.SuccessRate?.toFixed(1)}%</h3>
              </Card.Body>
            </Card>
          </Col>
          <Col md={3}>
            <Card className="text-center h-100">
              <Card.Body>
                <h6>Actions</h6>
                <Button
                  variant="outline-secondary"
                  size="sm"
                  onClick={resetCounters}
                  disabled={loading}
                >
                  Reset Counters
                </Button>
              </Card.Body>
            </Card>
          </Col>
        </Row>
      )}

      {/* Load Testing */}
      <Row className="mb-4">
        <Col>
          <Card>
            <Card.Body>
              <h5 className="mb-3">Load Testing</h5>
              <Form>
                <Row>
                  <Col md={4}>
                    <Form.Group className="mb-3">
                      <Form.Label>Number of Requests</Form.Label>
                      <Form.Control
                        type="number"
                        value={loadTestConfig.requests}
                        onChange={(e) => setLoadTestConfig(prev => ({
                          ...prev,
                          requests: parseInt(e.target.value) || 10
                        }))}
                        min="1"
                        max="100"
                      />
                    </Form.Group>
                  </Col>
                  <Col md={4}>
                    <Form.Group className="mb-3">
                      <Form.Label>Failure Rate</Form.Label>
                      <Form.Control
                        type="number"
                        step="0.1"
                        value={loadTestConfig.failureRate}
                        onChange={(e) => setLoadTestConfig(prev => ({
                          ...prev,
                          failureRate: parseFloat(e.target.value) || 0.5
                        }))}
                        min="0"
                        max="1"
                      />
                      <Form.Text className="text-muted">
                        0.0 = No failures, 1.0 = All failures
                      </Form.Text>
                    </Form.Group>
                  </Col>
                  <Col md={4} className="d-flex align-items-end">
                    <Button
                      variant="primary"
                      onClick={runLoadTest}
                      disabled={loading}
                      className="mb-3"
                    >
                      {loading ? (
                        <>
                          <span className="spinner-border spinner-border-sm me-2" />
                          Testing...
                        </>
                      ) : (
                        <>
                          <i className="bi bi-lightning me-2"></i>
                          Run Load Test
                        </>
                      )}
                    </Button>
                  </Col>
                </Row>
              </Form>
            </Card.Body>
          </Card>
        </Col>
      </Row>

      {/* Test Results */}
      {testResults.length > 0 && (
        <Row>
          <Col>
            <Card>
              <Card.Body>
                <h5 className="mb-3">Test Results</h5>
                <Table striped hover responsive>
                  <thead className="table-dark">
                    <tr>
                      <th>Time</th>
                      <th>Test Type</th>
                      <th>Failure Simulated</th>
                      <th>Result</th>
                      <th>Circuit State</th>
                      <th>Details</th>
                    </tr>
                  </thead>
                  <tbody>
                    {testResults.slice(-20).reverse().map((test) => (
                      <tr key={test.id}>
                        <td>{test.timestamp}</td>
                        <td>
                          <Badge bg={test.type === 'Database' ? 'primary' : test.type === 'API' ? 'info' : 'secondary'}>
                            {test.type}
                          </Badge>
                        </td>
                        <td>
                          {test.type === 'Load Test' ? 'N/A' : (
                            <Badge bg={test.simulateFailure ? 'danger' : 'success'}>
                              {test.simulateFailure ? 'Yes' : 'No'}
                            </Badge>
                          )}
                        </td>
                        <td>{getResultStatusBadge(test.result)}</td>
                        <td>
                          <Badge bg={getCircuitStateColor(test.result.CircuitState)}>
                            {test.result.CircuitState || 'Unknown'}
                          </Badge>
                        </td>
                        <td>
                          <small>
                            {test.result.Message || test.result.Error || 
                             (test.result.Results && `${test.result.Results.Successful}/${test.result.LoadTest?.TotalRequests} successful`) ||
                             'No details'}
                          </small>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </Table>
              </Card.Body>
            </Card>
          </Col>
        </Row>
      )}

      {/* Circuit Breaker States Explanation */}
      <Row className="mt-4">
        <Col>
          <Card>
            <Card.Body>
              <h5 className="mb-3">Circuit Breaker States</h5>
              <Row>
                <Col md={4}>
                  <div className="border-start border-success border-3 ps-3 mb-3">
                    <h6 className="text-success">🟢 Closed State</h6>
                    <p className="mb-0 small">
                      Normal operation. Requests pass through and are executed normally.
                      Failure rate is monitored continuously.
                    </p>
                  </div>
                </Col>
                <Col md={4}>
                  <div className="border-start border-danger border-3 ps-3 mb-3">
                    <h6 className="text-danger">🔴 Open State</h6>
                    <p className="mb-0 small">
                      Circuit is open. Requests are immediately rejected to prevent cascade failures.
                      System waits for the break duration before moving to half-open.
                    </p>
                  </div>
                </Col>
                <Col md={4}>
                  <div className="border-start border-warning border-3 ps-3 mb-3">
                    <h6 className="text-warning">🟡 Half-Open State</h6>
                    <p className="mb-0 small">
                      Testing phase. A limited number of requests are allowed to test if the service has recovered.
                      Success closes the circuit, failure reopens it.
                    </p>
                  </div>
                </Col>
              </Row>
            </Card.Body>
          </Card>
        </Col>
      </Row>
    </Container>
  );
};

export default CircuitBreakerReport;