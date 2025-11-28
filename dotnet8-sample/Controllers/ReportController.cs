using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace dotnet_sample.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly ILogger<ReportController> _logger;

        private readonly dotnet_sample.Services.RabbitMqPublisher _publisher;

        public ReportController(ILogger<ReportController> logger, dotnet_sample.Services.RabbitMqPublisher publisher)
        {
            _logger = logger;
            _publisher = publisher;
        }

        [HttpGet("sales-report")]
        public async Task<IActionResult> GenerateSalesReport([FromQuery] int months = 12, [FromQuery] int delay = 0)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            _logger.LogInformation("Starting sales report generation for {Months} months", months);

            try
            {
                var report = await GenerateLongSalesReportAsync(months, delay);
                stopwatch.Stop();
                
                _logger.LogInformation("Sales report generated in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                // Publish summary to RabbitMQ
                var summaryMessage = $"{{\"type\":\"sales-summary\",\"generatedAt\":\"{DateTime.UtcNow:o}\",\"months\":{months},\"totalRecords\":{report.Records.Count},\"totalRevenue\":{report.TotalRevenue}}}";
                _publisher.Publish(summaryMessage);
                
                return Ok(new
                {
                    reportType = "sales-summary",
                    generatedAt = DateTime.UtcNow,
                    processingTimeMs = stopwatch.ElapsedMilliseconds,
                    months = months,
                    totalRecords = report.Records.Count,
                    totalRevenue = report.TotalRevenue,
                    summary = report
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate sales report");
                return StatusCode(500, new { error = "Failed to generate report", message = ex.Message });
            }
        }

        [HttpGet("performance-report")]
        public async Task<IActionResult> GeneratePerformanceReport([FromQuery] int complexity = 5, [FromQuery] int delay = 1000)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            _logger.LogInformation("Starting performance report generation with complexity {Complexity}", complexity);

            try
            {
                var report = await GenerateComplexPerformanceReportAsync(complexity, delay);
                stopwatch.Stop();
                
                _logger.LogInformation("Performance report generated in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                
                return Ok(new
                {
                    reportType = "performance-analysis",
                    generatedAt = DateTime.UtcNow,
                    processingTimeMs = stopwatch.ElapsedMilliseconds,
                    complexity = complexity,
                    dataPoints = report.DataPoints.Count,
                    summary = report
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate performance report");
                return StatusCode(500, new { error = "Failed to generate report", message = ex.Message });
            }
        }

        [HttpGet("detailed-analytics")]
        public async Task<IActionResult> GenerateDetailedAnalytics([FromQuery] int records = 1000, [FromQuery] bool includeCharts = true)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            _logger.LogInformation("Starting detailed analytics generation for {Records} records", records);

            try
            {
                var analytics = await GenerateDetailedAnalyticsAsync(records, includeCharts);
                stopwatch.Stop();
                
                _logger.LogInformation("Detailed analytics generated in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                
                return Ok(new
                {
                    reportType = "detailed-analytics",
                    generatedAt = DateTime.UtcNow,
                    processingTimeMs = stopwatch.ElapsedMilliseconds,
                    recordsProcessed = records,
                    includeCharts = includeCharts,
                    analytics = analytics
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate detailed analytics");
                return StatusCode(500, new { error = "Failed to generate analytics", message = ex.Message });
            }
        }

        private async Task<SalesReport> GenerateLongSalesReportAsync(int months, int delayMs)
        {
            var random = new Random();
            var report = new SalesReport();
            var startDate = DateTime.UtcNow.AddMonths(-months);

            for (int month = 0; month < months; month++)
            {
                var monthDate = startDate.AddMonths(month);
                var daysInMonth = DateTime.DaysInMonth(monthDate.Year, monthDate.Month);

                for (int day = 1; day <= daysInMonth; day++)
                {
                    var currentDate = new DateTime(monthDate.Year, monthDate.Month, day);
                    var dailySales = random.Next(50, 200);
                    var avgOrderValue = Math.Round(random.NextDouble() * 100 + 20, 2);
                    var dailyRevenue = Math.Round(dailySales * avgOrderValue, 2);

                    report.Records.Add(new SalesRecord
                    {
                        Date = currentDate,
                        SalesCount = dailySales,
                        Revenue = dailyRevenue,
                        AverageOrderValue = avgOrderValue,
                        TopProducts = GenerateTopProducts(random, 5),
                        CustomerSegments = GenerateCustomerSegments(random),
                        RegionalBreakdown = GenerateRegionalBreakdown(random)
                    });

                    report.TotalRevenue += dailyRevenue;
                    
                    // Add small delay to simulate processing time
                    if (delayMs > 0 && day % 10 == 0)
                    {
                        await Task.Delay(delayMs / 10);
                    }
                }
            }

            // Calculate additional metrics
            report.AverageMonthlyRevenue = Math.Round(report.TotalRevenue / months, 2);
            report.TotalTransactions = report.Records.Sum(r => r.SalesCount);
            report.OverallAverageOrderValue = Math.Round(report.TotalRevenue / report.TotalTransactions, 2);

            return report;
        }

        private async Task<PerformanceReport> GenerateComplexPerformanceReportAsync(int complexity, int delayMs)
        {
            var report = new PerformanceReport();
            var random = new Random();

            // Simulate complex calculations
            for (int i = 0; i < complexity * 100; i++)
            {
                var dataPoint = new PerformanceDataPoint
                {
                    Timestamp = DateTime.UtcNow.AddMinutes(-i),
                    CpuUsage = Math.Round(random.NextDouble() * 100, 2),
                    MemoryUsage = Math.Round(random.NextDouble() * 100, 2),
                    RequestsPerSecond = random.Next(10, 1000),
                    ResponseTime = Math.Round(random.NextDouble() * 2000 + 50, 2),
                    ErrorRate = Math.Round(random.NextDouble() * 5, 3),
                    ThroughputMbps = Math.Round(random.NextDouble() * 100 + 10, 2)
                };

                // Simulate heavy computation
                dataPoint.ComputedMetric = await SimulateHeavyComputation(dataPoint, complexity);
                report.DataPoints.Add(dataPoint);

                if (i % 50 == 0 && delayMs > 0)
                {
                    await Task.Delay(delayMs / complexity);
                }
            }

            // Calculate summary statistics
            report.Summary = new PerformanceSummary
            {
                AvgCpuUsage = Math.Round(report.DataPoints.Average(d => d.CpuUsage), 2),
                AvgMemoryUsage = Math.Round(report.DataPoints.Average(d => d.MemoryUsage), 2),
                AvgResponseTime = Math.Round(report.DataPoints.Average(d => d.ResponseTime), 2),
                MaxRequestsPerSecond = report.DataPoints.Max(d => d.RequestsPerSecond),
                TotalErrors = report.DataPoints.Sum(d => d.ErrorRate)
            };

            return report;
        }

        private async Task<DetailedAnalytics> GenerateDetailedAnalyticsAsync(int records, bool includeCharts)
        {
            var analytics = new DetailedAnalytics();
            var random = new Random();

            // Generate transaction data
            for (int i = 0; i < records; i++)
            {
                analytics.Transactions.Add(new TransactionAnalytics
                {
                    TransactionId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow.AddHours(-random.Next(0, 720)), // Last 30 days
                    Amount = Math.Round(random.NextDouble() * 1000 + 10, 2),
                    Category = GetRandomCategory(random),
                    PaymentMethod = GetRandomPaymentMethod(random),
                    CustomerSegment = GetRandomCustomerSegment(random),
                    ProcessingTime = random.Next(100, 5000),
                    Success = random.NextDouble() > 0.05 // 95% success rate
                });

                // Add processing delay for large datasets
                if (i % 100 == 0 && records > 500)
                {
                    await Task.Delay(10);
                }
            }

            // Generate analytics summaries
            analytics.Summary = GenerateAnalyticsSummary(analytics.Transactions);
            
            if (includeCharts)
            {
                analytics.Charts = await GenerateChartDataAsync(analytics.Transactions);
            }

            return analytics;
        }

        private async Task<double> SimulateHeavyComputation(PerformanceDataPoint dataPoint, int complexity)
        {
            await Task.Delay(1); // Simulate async operation
            
            // Simulate CPU-intensive calculation
            double result = 0;
            for (int i = 0; i < complexity * 10; i++)
            {
                var logValue = Math.Log(i + 2); // Avoid log(1) = 0 division
                if (!double.IsInfinity(logValue) && logValue > 0)
                {
                    var sqrtValue = Math.Sqrt(dataPoint.CpuUsage * dataPoint.MemoryUsage);
                    if (!double.IsNaN(sqrtValue) && !double.IsInfinity(sqrtValue))
                    {
                        result += sqrtValue / logValue;
                    }
                }
            }
            
            // Ensure result is finite
            if (double.IsNaN(result) || double.IsInfinity(result))
            {
                result = 0;
            }
            
            return Math.Round(result, 4);
        }

        private List<TopProduct> GenerateTopProducts(Random random, int count)
        {
            var products = new[] { "Laptop", "Mouse", "Keyboard", "Monitor", "Headphones", "Webcam", "Tablet", "Phone" };
            return products.Take(count).Select(p => new TopProduct
            {
                Name = p,
                Sales = random.Next(10, 100),
                Revenue = Math.Round(random.NextDouble() * 1000 + 100, 2)
            }).ToList();
        }

        private Dictionary<string, double> GenerateCustomerSegments(Random random)
        {
            return new Dictionary<string, double>
            {
                ["Enterprise"] = Math.Round(random.NextDouble() * 40 + 20, 1),
                ["SMB"] = Math.Round(random.NextDouble() * 30 + 25, 1),
                ["Individual"] = Math.Round(random.NextDouble() * 25 + 15, 1),
                ["Government"] = Math.Round(random.NextDouble() * 15 + 5, 1)
            };
        }

        private Dictionary<string, double> GenerateRegionalBreakdown(Random random)
        {
            return new Dictionary<string, double>
            {
                ["North America"] = Math.Round(random.NextDouble() * 40 + 30, 1),
                ["Europe"] = Math.Round(random.NextDouble() * 30 + 25, 1),
                ["Asia Pacific"] = Math.Round(random.NextDouble() * 25 + 20, 1),
                ["Latin America"] = Math.Round(random.NextDouble() * 15 + 10, 1),
                ["Other"] = Math.Round(random.NextDouble() * 10 + 5, 1)
            };
        }

        private string GetRandomCategory(Random random)
        {
            var categories = new[] { "Electronics", "Clothing", "Books", "Home", "Sports", "Beauty", "Automotive", "Food" };
            return categories[random.Next(categories.Length)];
        }

        private string GetRandomPaymentMethod(Random random)
        {
            var methods = new[] { "Credit Card", "Debit Card", "PayPal", "Bank Transfer", "Digital Wallet", "Cash" };
            return methods[random.Next(methods.Length)];
        }

        private string GetRandomCustomerSegment(Random random)
        {
            var segments = new[] { "Premium", "Standard", "Basic", "VIP", "Enterprise" };
            return segments[random.Next(segments.Length)];
        }

        private AnalyticsSummary GenerateAnalyticsSummary(List<TransactionAnalytics> transactions)
        {
            return new AnalyticsSummary
            {
                TotalTransactions = transactions.Count,
                TotalRevenue = Math.Round(transactions.Sum(t => t.Amount), 2),
                AverageTransactionValue = Math.Round(transactions.Average(t => t.Amount), 2),
                SuccessRate = Math.Round((double)transactions.Count(t => t.Success) / transactions.Count * 100, 2),
                AverageProcessingTime = Math.Round(transactions.Average(t => t.ProcessingTime), 2),
                TopCategories = transactions.GroupBy(t => t.Category)
                    .OrderByDescending(g => g.Sum(t => t.Amount))
                    .Take(5)
                    .ToDictionary(g => g.Key, g => Math.Round(g.Sum(t => t.Amount), 2))
            };
        }

        private async Task<ChartData> GenerateChartDataAsync(List<TransactionAnalytics> transactions)
        {
            await Task.Delay(50); // Simulate chart generation time
            
            return new ChartData
            {
                DailyRevenue = transactions
                    .GroupBy(t => t.Timestamp.Date)
                    .OrderBy(g => g.Key)
                    .ToDictionary(g => g.Key.ToString("yyyy-MM-dd"), g => Math.Round(g.Sum(t => t.Amount), 2)),
                CategoryBreakdown = transactions
                    .GroupBy(t => t.Category)
                    .ToDictionary(g => g.Key, g => g.Count()),
                PaymentMethodDistribution = transactions
                    .GroupBy(t => t.PaymentMethod)
                    .ToDictionary(g => g.Key, g => Math.Round((double)g.Count() / transactions.Count * 100, 1))
            };
        }
    }

    // Data models
    public class SalesReport
    {
        public List<SalesRecord> Records { get; set; } = new();
        public double TotalRevenue { get; set; }
        public double AverageMonthlyRevenue { get; set; }
        public int TotalTransactions { get; set; }
        public double OverallAverageOrderValue { get; set; }
    }

    public class SalesRecord
    {
        public DateTime Date { get; set; }
        public int SalesCount { get; set; }
        public double Revenue { get; set; }
        public double AverageOrderValue { get; set; }
        public List<TopProduct> TopProducts { get; set; } = new();
        public Dictionary<string, double> CustomerSegments { get; set; } = new();
        public Dictionary<string, double> RegionalBreakdown { get; set; } = new();
    }

    public class TopProduct
    {
        public string Name { get; set; } = "";
        public int Sales { get; set; }
        public double Revenue { get; set; }
    }

    public class PerformanceReport
    {
        public List<PerformanceDataPoint> DataPoints { get; set; } = new();
        public PerformanceSummary Summary { get; set; } = new();
    }

    public class PerformanceDataPoint
    {
        public DateTime Timestamp { get; set; }
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public int RequestsPerSecond { get; set; }
        public double ResponseTime { get; set; }
        public double ErrorRate { get; set; }
        public double ThroughputMbps { get; set; }
        public double ComputedMetric { get; set; }
    }

    public class PerformanceSummary
    {
        public double AvgCpuUsage { get; set; }
        public double AvgMemoryUsage { get; set; }
        public double AvgResponseTime { get; set; }
        public int MaxRequestsPerSecond { get; set; }
        public double TotalErrors { get; set; }
    }

    public class DetailedAnalytics
    {
        public List<TransactionAnalytics> Transactions { get; set; } = new();
        public AnalyticsSummary Summary { get; set; } = new();
        public ChartData? Charts { get; set; }
    }

    public class TransactionAnalytics
    {
        public string TransactionId { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public double Amount { get; set; }
        public string Category { get; set; } = "";
        public string PaymentMethod { get; set; } = "";
        public string CustomerSegment { get; set; } = "";
        public double ProcessingTime { get; set; }
        public bool Success { get; set; }
    }

    public class AnalyticsSummary
    {
        public int TotalTransactions { get; set; }
        public double TotalRevenue { get; set; }
        public double AverageTransactionValue { get; set; }
        public double SuccessRate { get; set; }
        public double AverageProcessingTime { get; set; }
        public Dictionary<string, double> TopCategories { get; set; } = new();
    }

    public class ChartData
    {
        public Dictionary<string, double> DailyRevenue { get; set; } = new();
        public Dictionary<string, int> CategoryBreakdown { get; set; } = new();
        public Dictionary<string, double> PaymentMethodDistribution { get; set; } = new();
    }
}