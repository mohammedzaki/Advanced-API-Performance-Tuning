import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Card, Form, Button, Alert, Table, Badge } from 'react-bootstrap';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer, PieChart, Pie, Cell } from 'recharts';
import apiService from '../../services/apiService';

const SalesReport = () => {
  const [reportData, setReportData] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [formData, setFormData] = useState({
    months: 6,
    delay: 0
  });

  useEffect(() => {
    loadReport();
  }, []);

  const loadReport = async () => {
    setLoading(true);
    setError(null);
    
    try {
      const data = await apiService.getSalesReport(formData.months, formData.delay);
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

  const formatCurrency = (value) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(value);
  };

  const getMonthlyData = () => {
    if (!reportData?.salesByMonth) return [];
    
    return Object.entries(reportData.salesByMonth).map(([month, amount]) => ({
      month: new Date(month).toLocaleDateString('en-US', { month: 'short', year: 'numeric' }),
      amount: amount,
      formattedAmount: formatCurrency(amount)
    }));
  };

  const getCategoryData = () => {
    if (!reportData?.salesByCategory) return [];
    
    return Object.entries(reportData.salesByCategory).map(([category, amount]) => ({
      name: category,
      value: amount,
      formattedValue: formatCurrency(amount)
    }));
  };

  const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8884d8', '#82ca9d'];

  return (
    <Container>
      <Row className="mb-4">
        <Col>
          <h1 className="display-5">💰 Sales Report</h1>
          <p className="lead">Comprehensive sales analysis and revenue tracking</p>
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
                      <Form.Label>Number of Months</Form.Label>
                      <Form.Control
                        type="number"
                        name="months"
                        value={formData.months}
                        onChange={handleInputChange}
                        min="1"
                        max="24"
                      />
                      <Form.Text className="text-muted">
                        Report period (1-24 months)
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
                          Generating...
                        </>
                      ) : (
                        <>
                          <i className="bi bi-arrow-clockwise me-2"></i>
                          Generate Report
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
          <Alert.Heading>Report Generation Error</Alert.Heading>
          {error}
        </Alert>
      )}

      {/* Report Summary */}
      {reportData && (
        <>
          <Row className="mb-4">
            <Col md={3}>
              <Card className="metric-card h-100">
                <Card.Body className="text-center">
                  <h6 className="text-light">Total Revenue</h6>
                  <h3>{formatCurrency(reportData.totalRevenue)}</h3>
                  <Badge bg="light" text="dark" className="performance-badge">
                    {reportData.processingTimeMs}ms processing
                  </Badge>
                </Card.Body>
              </Card>
            </Col>
            <Col md={3}>
              <Card className="metric-card h-100">
                <Card.Body className="text-center">
                  <h6 className="text-light">Total Transactions</h6>
                  <h3>{reportData.totalRecords?.toLocaleString()}</h3>
                  <Badge bg="light" text="dark" className="performance-badge">
                    {reportData.months} months
                  </Badge>
                </Card.Body>
              </Card>
            </Col>
            <Col md={3}>
              <Card className="metric-card h-100">
                <Card.Body className="text-center">
                  <h6 className="text-light">Average Monthly</h6>
                  <h3>{formatCurrency(reportData.totalRevenue / reportData.months)}</h3>
                  <Badge bg="light" text="dark" className="performance-badge">
                    per month
                  </Badge>
                </Card.Body>
              </Card>
            </Col>
            <Col md={3}>
              <Card className="metric-card h-100">
                <Card.Body className="text-center">
                  <h6 className="text-light">Avg Transaction</h6>
                  <h3>{formatCurrency(reportData.totalRevenue / reportData.totalRecords)}</h3>
                  <Badge bg="light" text="dark" className="performance-badge">
                    per sale
                  </Badge>
                </Card.Body>
              </Card>
            </Col>
          </Row>

          {/* Charts */}
          <Row className="mb-4">
            <Col lg={8}>
              <Card className="chart-container">
                <Card.Body>
                  <h5 className="mb-3">Monthly Sales Trend</h5>
                  <ResponsiveContainer width="100%" height={300}>
                    <BarChart data={getMonthlyData()}>
                      <CartesianGrid strokeDasharray="3 3" />
                      <XAxis dataKey="month" />
                      <YAxis tickFormatter={(value) => `$${(value / 1000).toFixed(0)}K`} />
                      <Tooltip formatter={(value) => formatCurrency(value)} />
                      <Legend />
                      <Bar dataKey="amount" fill="#8884d8" name="Sales Revenue" />
                    </BarChart>
                  </ResponsiveContainer>
                </Card.Body>
              </Card>
            </Col>
            <Col lg={4}>
              <Card className="chart-container">
                <Card.Body>
                  <h5 className="mb-3">Sales by Category</h5>
                  <ResponsiveContainer width="100%" height={300}>
                    <PieChart>
                      <Pie
                        data={getCategoryData()}
                        cx="50%"
                        cy="50%"
                        labelLine={false}
                        label={({name, percent}) => `${name} ${(percent * 100).toFixed(0)}%`}
                        outerRadius={80}
                        fill="#8884d8"
                        dataKey="value"
                      >
                        {getCategoryData().map((entry, index) => (
                          <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                        ))}
                      </Pie>
                      <Tooltip formatter={(value) => formatCurrency(value)} />
                    </PieChart>
                  </ResponsiveContainer>
                </Card.Body>
              </Card>
            </Col>
          </Row>

          {/* Detailed Data Table */}
          <Row>
            <Col>
              <Card>
                <Card.Body>
                  <h5 className="mb-3">Monthly Breakdown</h5>
                  <Table striped hover responsive>
                    <thead className="table-dark">
                      <tr>
                        <th>Month</th>
                        <th>Revenue</th>
                        <th>Growth</th>
                        <th>Transactions</th>
                        <th>Avg per Transaction</th>
                      </tr>
                    </thead>
                    <tbody>
                      {getMonthlyData().map((month, index) => {
                        const prevAmount = index > 0 ? getMonthlyData()[index - 1].amount : month.amount;
                        const growth = ((month.amount - prevAmount) / prevAmount * 100);
                        const estimatedTransactions = Math.floor(month.amount / (reportData.totalRevenue / reportData.totalRecords));
                        
                        return (
                          <tr key={month.month}>
                            <td>{month.month}</td>
                            <td>{month.formattedAmount}</td>
                            <td>
                              {index > 0 ? (
                                <Badge bg={growth >= 0 ? 'success' : 'danger'}>
                                  {growth >= 0 ? '↗️' : '↘️'} {Math.abs(growth).toFixed(1)}%
                                </Badge>
                              ) : (
                                <Badge bg="secondary">-</Badge>
                              )}
                            </td>
                            <td>{estimatedTransactions.toLocaleString()}</td>
                            <td>{formatCurrency(month.amount / estimatedTransactions)}</td>
                          </tr>
                        );
                      })}
                    </tbody>
                  </Table>
                </Card.Body>
              </Card>
            </Col>
          </Row>
        </>
      )}
    </Container>
  );
};

export default SalesReport;