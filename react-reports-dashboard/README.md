# React Reports Dashboard

A comprehensive React-based dashboard for visualizing and analyzing API performance reports. Built with Bootstrap and Recharts for a modern, responsive interface.

## Features

- **Dashboard Overview**: Real-time metrics and system health monitoring
- **Sales Reports**: Revenue analysis with interactive charts and trend visualization
- **Performance Reports**: System resource utilization and response time analysis
- **Analytics Reports**: Deep transaction analysis with statistical insights
- **Responsive Design**: Bootstrap-powered responsive UI
- **Real-time Data**: Live API integration with performance monitoring
- **Interactive Charts**: Rich visualizations using Recharts library

## Tech Stack

- **React 18.2.0**: Modern React with hooks and functional components
- **Bootstrap 5.3.2**: Responsive CSS framework
- **React Bootstrap 2.9.1**: Bootstrap components for React
- **Recharts 2.8.0**: Charts and data visualization
- **Axios 1.6.0**: HTTP client for API calls
- **React Router DOM 6.8.0**: Client-side routing

## API Integration

The dashboard integrates with your .NET API running on `localhost:8080`:

- **Sales Report API**: `/api/report/sales-report`
- **Performance Report API**: `/api/report/performance-report`
- **Analytics Report API**: `/api/report/detailed-analytics`
- **Health Check**: `/health`

## Quick Start

### Prerequisites
- Node.js 16+ installed
- .NET API running on port 8080

### Installation

```bash
# Navigate to the React project directory
cd react-reports-dashboard

# Install dependencies
npm install

# Start the development server
npm start
```

The application will open at `http://localhost:3000`

### Environment Configuration

Create a `.env` file in the root directory:

```env
REACT_APP_API_URL=http://localhost:8080
```

## Project Structure

```
react-reports-dashboard/
├── public/
│   └── index.html
├── src/
│   ├── components/
│   │   ├── NavigationBar.js          # Main navigation
│   │   ├── Dashboard.js              # Dashboard overview
│   │   └── reports/
│   │       ├── SalesReport.js        # Sales analysis
│   │       ├── PerformanceReport.js  # System performance
│   │       └── AnalyticsReport.js    # Advanced analytics
│   ├── services/
│   │   └── apiService.js             # API integration layer
│   ├── App.js                        # Main application component
│   ├── index.js                      # Application entry point
│   └── index.css                     # Global styles
├── package.json
└── README.md
```

## Features Overview

### 1. Dashboard
- System health status
- Quick metrics overview
- Performance indicators
- Navigation to detailed reports

### 2. Sales Reports
- Monthly revenue trends
- Category-based sales analysis
- Interactive bar charts and pie charts
- Configurable report parameters (months, processing delay)

### 3. Performance Reports
- CPU and memory utilization
- Response time analysis
- System health recommendations
- Real-time performance metrics

### 4. Analytics Reports
- Transaction value distribution
- Statistical analysis
- Scatter plot visualization
- Business insights and recommendations

## Development

### Available Scripts

- `npm start`: Run development server
- `npm build`: Build for production
- `npm test`: Run test suite
- `npm eject`: Eject from Create React App

### Key Components

#### Dashboard Component
- Loads quick metrics from all APIs
- Displays system health status
- Provides navigation to detailed reports

#### Report Components
- Configurable parameters
- Real-time API integration
- Interactive charts and visualizations
- Export capabilities

#### API Service
- Centralized API integration
- Error handling and retry logic
- Configurable base URL

## Customization

### Styling
- Bootstrap theme customization in `index.css`
- Custom CSS classes for specific components
- Responsive breakpoints for mobile optimization

### API Configuration
- Modify `apiService.js` for different endpoints
- Update environment variables for different environments
- Add authentication headers if required

## Production Deployment

### Build for Production

```bash
npm run build
```

### Docker Deployment

Create a `Dockerfile`:

```dockerfile
FROM node:18-alpine as builder
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build

FROM nginx:alpine
COPY --from=builder /app/build /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

### Environment Variables

For production, set:
- `REACT_APP_API_URL`: Your production API URL
- Additional environment-specific configurations

## Integration with Docker Compose

Add to your existing `docker-compose.yml`:

```yaml
react-dashboard:
  build:
    context: ./react-reports-dashboard
  container_name: react-dashboard
  ports:
    - "3001:80"
  environment:
    - REACT_APP_API_URL=http://localhost:8080
  depends_on:
    - dotnet-app
  networks:
    - perf-net
```

## Monitoring and Observability

The dashboard includes links to:
- **Jaeger Tracing**: `http://localhost:16686`
- **Grafana Dashboards**: `http://localhost:3000`
- **Direct API Health**: Integration with health check endpoints

## Performance Considerations

- **Lazy Loading**: Components loaded on-demand
- **Memoization**: React.memo for performance optimization
- **Efficient Re-renders**: Optimized state management
- **Chart Performance**: Recharts optimized for large datasets

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is part of the Advanced API Performance Tuning training course.