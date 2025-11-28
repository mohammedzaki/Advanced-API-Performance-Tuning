import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Card, Form, Button, Alert, Badge, ProgressBar } from 'react-bootstrap';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer, AreaChart, Area } from 'recharts';
import apiService from '../../services/apiService';

const PerformanceReport = () => {
  const [reportData, setReportData] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [formData, setFormData] = useState({
    duration: 60,
    delay: 0
  });

  useEffect(() => {
    loadReport();
  }, []);

  const loadReport = async () => {
    setLoading(true);
    setError(null);
    
    try {
      const data = await apiService.getPerformanceReport(formData.duration, formData.delay);
      setReportData(data);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    loadReport();
  };

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: parseInt(value) || 0
    }));
  };

  const getHealthStatus = (value, type) => {
    if (type === 'cpu') {
      if (value < 50) return { variant: 'success', text: 'Excellent' };
      if (value < 80) return { variant: 'warning', text: 'Warning' };
      return { variant: 'danger', text: 'Critical' };
    } else if (type === 'memory') {
      if (value < 60) return { variant: 'success', text: 'Good' };
      if (value < 85) return { variant: 'warning', text: 'Warning' };
      return { variant: 'danger', text: 'Critical' };
    } else if (type === 'response') {
      if (value < 100) return { variant: 'success', text: 'Fast' };
      if (value < 500) return { variant: 'warning', text: 'Moderate' };
      return { variant: 'danger', text: 'Slow' };
    }
    return { variant: 'secondary', text: 'Unknown' };
  };

  const formatDataForChart = () => {
    if (!reportData?.dataPoints) return [];
    
    return reportData.dataPoints.map((point, index) => ({
      ...point,
      time: `${index * 5}s`, // Assuming data points are every 5 seconds
      responseTime: Math.random() * 200 + 50 // Simulated response time
    }));
  };

  return (
    <Container>
      <Row className="mb-4">
        <Col>
          <h1 className="display-5">⚡ Performance Report</h1>
          <p className="lead">System performance metrics and resource utilization analysis</p>
        </Col>
      </Row>

      {/* Report Parameters */}
      <Row className="mb-4">
        <Col>
          <Card>
            <Card.Body>
              <h5 className="mb-3">Report Parameters</h5>
              <Form onSubmit={handleSubmit}>
                <Row>
                  <Col md={4}>
                    <Form.Group className="mb-3">
                      <Form.Label>Duration (seconds)</Form.Label>
                      <Form.Control
                        type="number"
                        name="duration"
                        value={formData.duration}
                        onChange={handleInputChange}
                        min="10"
                        max="3600"
                      />
                      <Form.Text className="text-muted">
                        Monitoring period (10s - 1 hour)
                      </Form.Text>
                    </Form.Group>
                  </Col>
                  <Col md={4}>
                    <Form.Group className="mb-3">
                      <Form.Label>Processing Delay (ms)</Form.Label>
                      <Form.Control
                        type="number"
                        name="delay"
                        value={formData.delay}
                        onChange={handleInputChange}
                        min="0"
                        max="5000"
                      />
                      <Form.Text className="text-muted">
                        Simulate processing time
                      </Form.Text>
                    </Form.Group>
                  </Col>
                  <Col md={4} className="d-flex align-items-end">
                    <Button 
                      type="submit" 
                      variant="primary" 
                      disabled={loading}
                      className="mb-3"
                    >
                      {loading ? (
                        <>
                          <span className="spinner-border spinner-border-sm me-2" />
                          Analyzing...
                        </>
                      ) : (
                        <>
                          <i className="bi bi-speedometer2 me-2"></i>
                          Run Analysis
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

      {error && (
        <Alert variant="danger" dismissible onClose={() => setError(null)}>
          <Alert.Heading>Performance Analysis Error</Alert.Heading>
          {error}
        </Alert>
      )}

      {/* Performance Metrics */}
      {reportData && (
        <>
          {/* Key Metrics */}
          <Row className="mb-4">
            <Col md={3}>
              <Card className="h-100">
                <Card.Body className="text-center">
                  <h6 className="text-muted">Average CPU Usage</h6>
                  <h3 className="text-primary">{reportData.averageCpuUsage?.toFixed(1)}%</h3>
                  <ProgressBar 
                    variant={getHealthStatus(reportData.averageCpuUsage, 'cpu').variant}
                    now={reportData.averageCpuUsage} 
                    className="mb-2"
                  />
                  <Badge bg={getHealthStatus(reportData.averageCpuUsage, 'cpu').variant}>
                    {getHealthStatus(reportData.averageCpuUsage, 'cpu').text}
                  </Badge>
                </Card.Body>
              </Card>
            </Col>
            <Col md={3}>
              <Card className="h-100">
                <Card.Body className="text-center">
                  <h6 className="text-muted">Average Memory Usage</h6>
                  <h3 className="text-success">{reportData.averageMemoryUsage?.toFixed(1)}%</h3>
                  <ProgressBar 
                    variant={getHealthStatus(reportData.averageMemoryUsage, 'memory').variant}
                    now={reportData.averageMemoryUsage} 
                    className="mb-2"
                  />
                  <Badge bg={getHealthStatus(reportData.averageMemoryUsage, 'memory').variant}>
                    {getHealthStatus(reportData.averageMemoryUsage, 'memory').text}
                  </Badge>
                </Card.Body>
              </Card>
            </Col>
            <Col md={3}>
              <Card className="h-100">
                <Card.Body className="text-center">
                  <h6 className="text-muted">Peak CPU Usage</h6>
                  <h3 className="text-warning">{reportData.peakCpuUsage?.toFixed(1)}%</h3>
                  <ProgressBar 
                    variant={getHealthStatus(reportData.peakCpuUsage, 'cpu').variant}
                    now={reportData.peakCpuUsage} 
                    className="mb-2"
                  />
                  <Badge bg={getHealthStatus(reportData.peakCpuUsage, 'cpu').variant}>
                    Peak Load
                  </Badge>
                </Card.Body>
              </Card>
            </Col>
            <Col md={3}>
              <Card className="h-100">
                <Card.Body className="text-center">
                  <h6 className="text-muted">Data Points</h6>
                  <h3 className="text-info">{reportData.totalDataPoints?.toLocaleString()}</h3>
                  <div className="mb-2">
                    <small className="text-muted">Processing Time</small>
                  </div>
                  <Badge bg={getHealthStatus(reportData.processingTimeMs, 'response').variant}>
                    {reportData.processingTimeMs}ms
                  </Badge>
                </Card.Body>
              </Card>
            </Col>
          </Row>

          {/* Performance Charts */}
          <Row className="mb-4">
            <Col>
              <Card className="chart-container">
                <Card.Body>
                  <h5 className="mb-3">System Resource Utilization Over Time</h5>
                  <ResponsiveContainer width="100%" height={400}>
                    <AreaChart data={formatDataForChart()}>
                      <defs>
                        <linearGradient id="colorCpu" x1="0" y1="0" x2="0" y2="1">
                          <stop offset="5%" stopColor="#8884d8" stopOpacity={0.8}/>
                          <stop offset="95%" stopColor="#8884d8" stopOpacity={0}/>
                        </linearGradient>
                        <linearGradient id="colorMemory" x1="0" y1="0" x2="0" y2="1">
                          <stop offset="5%" stopColor="#82ca9d" stopOpacity={0.8}/>
                          <stop offset="95%" stopColor="#82ca9d" stopOpacity={0}/>
                        </linearGradient>
                      </defs>
                      <XAxis dataKey="time" />
                      <YAxis />
                      <CartesianGrid strokeDasharray="3 3" />
                      <Tooltip />
                      <Legend />
                      <Area
                        type="monotone"
                        dataKey="cpuUsage"
                        stroke="#8884d8"
                        fillOpacity={1}
                        fill="url(#colorCpu)"
                        name="CPU Usage (%)"
                      />
                      <Area
                        type="monotone"
                        dataKey="memoryUsage"
                        stroke="#82ca9d"
                        fillOpacity={1}
                        fill="url(#colorMemory)"
                        name="Memory Usage (%)"
                      />
                    </AreaChart>
                  </ResponsiveContainer>
                </Card.Body>
              </Card>
            </Col>
          </Row>

          {/* Response Time Chart */}
          <Row className="mb-4">
            <Col lg={8}>
              <Card className="chart-container">
                <Card.Body>
                  <h5 className="mb-3">Response Time Analysis</h5>
                  <ResponsiveContainer width="100%" height={300}>
                    <LineChart data={formatDataForChart()}>
                      <CartesianGrid strokeDasharray="3 3" />
                      <XAxis dataKey="time" />
                      <YAxis />
                      <Tooltip />
                      <Legend />
                      <Line
                        type="monotone"
                        dataKey="responseTime"
                        stroke="#ff7300"
                        strokeWidth={3}
                        name="Response Time (ms)"
                        dot={{ fill: '#ff7300', strokeWidth: 2, r: 4 }}
                      />
                    </LineChart>
                  </ResponsiveContainer>
                </Card.Body>
              </Card>
            </Col>
            <Col lg={4}>
              <Card className="h-100">
                <Card.Body>
                  <h5 className="mb-3">System Health Summary</h5>
                  <div className="mb-3">
                    <div className="d-flex justify-content-between mb-1">
                      <small>CPU Health</small>
                      <small>{reportData.averageCpuUsage?.toFixed(1)}%</small>
                    </div>
                    <ProgressBar 
                      variant={getHealthStatus(reportData.averageCpuUsage, 'cpu').variant}
                      now={reportData.averageCpuUsage}
                      style={{ height: '8px' }}
                    />
                  </div>
                  
                  <div className="mb-3">
                    <div className="d-flex justify-content-between mb-1">
                      <small>Memory Health</small>
                      <small>{reportData.averageMemoryUsage?.toFixed(1)}%</small>
                    </div>
                    <ProgressBar 
                      variant={getHealthStatus(reportData.averageMemoryUsage, 'memory').variant}
                      now={reportData.averageMemoryUsage}
                      style={{ height: '8px' }}
                    />
                  </div>

                  <div className="mb-3">
                    <div className="d-flex justify-content-between mb-1">
                      <small>Response Time</small>
                      <small>{reportData.processingTimeMs}ms</small>
                    </div>
                    <ProgressBar 
                      variant={getHealthStatus(reportData.processingTimeMs, 'response').variant}
                      now={Math.min(reportData.processingTimeMs / 10, 100)}
                      style={{ height: '8px' }}
                    />
                  </div>

                  <hr />
                  
                  <div className="text-center">
                    <h6>Overall System Status</h6>
                    {reportData.averageCpuUsage < 70 && reportData.averageMemoryUsage < 80 ? (
                      <Badge bg="success" className="fs-6">🟢 Healthy</Badge>
                    ) : reportData.averageCpuUsage < 85 && reportData.averageMemoryUsage < 90 ? (
                      <Badge bg="warning" className="fs-6">🟡 Warning</Badge>
                    ) : (
                      <Badge bg="danger" className="fs-6">🔴 Critical</Badge>
                    )}
                  </div>
                </Card.Body>
              </Card>
            </Col>
          </Row>

          {/* Performance Recommendations */}
          <Row>
            <Col>
              <Card>
                <Card.Body>
                  <h5 className="mb-3">Performance Recommendations</h5>
                  <Row>
                    <Col md={4}>
                      <div className="border-start border-primary border-3 ps-3 mb-3">
                        <h6 className="text-primary">CPU Optimization</h6>
                        <p className="mb-0 small">
                          {reportData.averageCpuUsage > 80 ? 
                            "Consider horizontal scaling or code optimization to reduce CPU usage." :
                            "CPU usage is within acceptable limits. Monitor during peak loads."
                          }
                        </p>
                      </div>
                    </Col>
                    <Col md={4}>
                      <div className="border-start border-success border-3 ps-3 mb-3">
                        <h6 className="text-success">Memory Management</h6>
                        <p className="mb-0 small">
                          {reportData.averageMemoryUsage > 85 ? 
                            "Memory usage is high. Check for memory leaks and consider garbage collection tuning." :
                            "Memory usage is healthy. Continue current memory management practices."
                          }
                        </p>
                      </div>
                    </Col>
                    <Col md={4}>
                      <div className="border-start border-info border-3 ps-3 mb-3">
                        <h6 className="text-info">Response Time</h6>
                        <p className="mb-0 small">
                          {reportData.processingTimeMs > 500 ? 
                            "Response times are elevated. Consider caching strategies and database optimization." :
                            "Response times are good. Monitor under increased load."
                          }
                        </p>
                      </div>
                    </Col>
                  </Row>
                </Card.Body>
              </Card>
            </Col>
          </Row>
        </>
      )}
    </Container>
  );
};

export default PerformanceReport;