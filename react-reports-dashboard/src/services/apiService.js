import axios from 'axios';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:8080';

const apiService = {
  // Sales Report API
  getSalesReport: async (months = 6, delay = 0) => {
    try {
      const response = await axios.get(`${API_BASE_URL}/api/report/sales-report`, {
        params: { months, delay }
      });
      return response.data;
    } catch (error) {
      throw new Error(`Sales Report API Error: ${error.message}`);
    }
  },

  // Performance Report API
  getPerformanceReport: async (duration = 60, delay = 0) => {
    try {
      const response = await axios.get(`${API_BASE_URL}/api/report/performance-report`, {
        params: { duration, delay }
      });
      return response.data;
    } catch (error) {
      throw new Error(`Performance Report API Error: ${error.message}`);
    }
  },

  // Analytics Report API
  getAnalyticsReport: async (records = 1000, delay = 0) => {
    try {
      const response = await axios.get(`${API_BASE_URL}/api/report/detailed-analytics`, {
        params: { records, delay }
      });
      return response.data;
    } catch (error) {
      throw new Error(`Analytics Report API Error: ${error.message}`);
    }
  },

  // Health Check
  healthCheck: async () => {
    try {
      const response = await axios.get(`${API_BASE_URL}/api/health`);
      return response.data;
    } catch (error) {
      return { status: 'unhealthy', error: error.message };
    }
  },

  // Circuit Breaker APIs
  getCircuitBreakerStatus: async () => {
    try {
      const response = await axios.get(`${API_BASE_URL}/api/circuit-breaker/status`);
      return response.data;
    } catch (error) {
      throw new Error(`Circuit Breaker Status API Error: ${error.message}`);
    }
  },

  testDatabaseCircuitBreaker: async (simulateFailure = false) => {
    try {
      const response = await axios.get(`${API_BASE_URL}/api/circuit-breaker/database-test`, {
        params: { simulateFailure }
      });
      return response.data;
    } catch (error) {
      return error.response?.data || { error: error.message };
    }
  },

  testApiCircuitBreaker: async (simulateFailure = false) => {
    try {
      const response = await axios.get(`${API_BASE_URL}/api/circuit-breaker/api-test`, {
        params: { simulateFailure }
      });
      return response.data;
    } catch (error) {
      return error.response?.data || { error: error.message };
    }
  },

  simulateLoad: async (requests = 10, failureRate = 0.5) => {
    try {
      const response = await axios.get(`${API_BASE_URL}/api/circuit-breaker/simulate-load`, {
        params: { requests, failureRate }
      });
      return response.data;
    } catch (error) {
      throw new Error(`Load Simulation API Error: ${error.message}`);
    }
  },

  resetCircuitBreakerCounters: async () => {
    try {
      const response = await axios.post(`${API_BASE_URL}/api/circuit-breaker/reset`);
      return response.data;
    } catch (error) {
      throw new Error(`Reset Counters API Error: ${error.message}`);
    }
  },

  // CORS Test API
  testCors: async () => {
    try {
      const response = await axios.get(`${API_BASE_URL}/api/corstest`);
      return response.data;
    } catch (error) {
      throw new Error(`CORS Test API Error: ${error.message}`);
    }
  },

  testCorsPost: async (data = {}) => {
    try {
      const response = await axios.post(`${API_BASE_URL}/api/corstest`, data);
      return response.data;
    } catch (error) {
      throw new Error(`CORS POST Test API Error: ${error.message}`);
    }
  }
};

export default apiService;