# dotnet8-sample

Simple .NET 8 Web API sample that exposes `/api/products` and `/api/products/{id}`.

## Run locally
```bash
dotnet restore
dotnet run
```

The API listens on the default Kestrel port (usually 5000). To run on port 8080:
```bash
dotnet run --urls http://localhost:8080
```

## Docker
```bash
docker build -t dotnet8-sample .
docker run -p 8080:8080 dotnet8-sample
```

## Notes
- Sample data seeded in memory (200 items).
- Small Thread.Sleep(5) in service to make baseline latency measurable.

## Reference course file
The uploaded course file is available at:
/mnt/data/Advanced_API_Performance_Tuning_20251105_123619_0000.docx

