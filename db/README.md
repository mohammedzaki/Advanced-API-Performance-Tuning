# Database Setup

This directory contains the database initialization scripts for the SQL Server container used in the Advanced API Performance Tuning demo.

## Files

- **`init-db.sh`** - Automated database initialization script that runs on first container startup
- **`init-db.sql`** - SQL restore command (backup approach)
- **`AdventureWorks2022.bak`** - SQL Server backup file (if available)

## Automatic Initialization

When you start the SQL Server container for the first time, the `init-db.sh` script will:

1. **Wait for SQL Server to be ready** - Waits up to 60 seconds for SQL Server to fully start
2. **Check if database exists** - Verifies if AdventureWorks2022 database is already present
3. **Restore from backup** (if available):
   - Looks for `AdventureWorks2022.bak` in this directory
   - Attempts to restore with standard logical names
   - Falls back to alternative logical names if needed
   - Shows file list if restore fails for debugging
4. **Create sample database** (if backup not available):
   - Creates AdventureWorks2022 database
   - Creates Products table with sample data
   - Inserts 5 sample products for testing

## Database Configuration

The database is configured via environment variables in `.env`:

```env
SQL_SERVER_HOST=mssql
SQL_SERVER_PORT=1433
SQL_SERVER_DATABASE=AdventureWorks2022
SQL_SERVER_USER=sa
SQL_SERVER_PASSWORD=YourStrong!Passw0rd
```

## Using a Custom Backup File

If you have an AdventureWorks2022 backup file:

1. Place your `.bak` file in this `db/` directory
2. Rename it to `AdventureWorks2022.bak`
3. Run `docker-compose down -v` to remove existing volumes
4. Run `docker-compose up -d` to recreate containers with your backup

## Manual Database Restoration

If you need to manually restore the database:

```bash
# Connect to SQL Server container
docker exec -it mssql /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong!Passw0rd

# List logical file names in backup
RESTORE FILELISTONLY FROM DISK = '/var/opt/mssql/backup/AdventureWorks2022.bak';
GO

# Restore database with correct logical names
RESTORE DATABASE AdventureWorks2022
FROM DISK = '/var/opt/mssql/backup/AdventureWorks2022.bak'
WITH MOVE 'LogicalDataFileName' TO '/var/opt/mssql/data/AdventureWorks2022.mdf',
     MOVE 'LogicalLogFileName' TO '/var/opt/mssql/data/AdventureWorks2022_Log.ldf',
     REPLACE;
GO
```

## Troubleshooting

### Database initialization fails
```bash
# Check SQL Server logs
docker logs mssql

# Connect to container and check manually
docker exec -it mssql /bin/bash

# Run initialization script manually
docker exec -it mssql /bin/bash -c "chmod +x /init-db.sh && /init-db.sh"
```

### Cannot connect to database
```bash
# Verify SQL Server is running
docker ps | grep mssql

# Check health status
docker inspect mssql | grep Health

# Test connection
docker exec -it mssql /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong!Passw0rd -Q "SELECT @@VERSION"
```

### Reset database
```bash
# Stop and remove containers and volumes
docker-compose down -v

# Start fresh
docker-compose up -d mssql
```

## Database Schema

The sample database includes:

### Products Table
```sql
CREATE TABLE Products (
    ProductID INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    Price DECIMAL(10,2) NOT NULL,
    Category NVARCHAR(50),
    CreatedDate DATETIME DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1
);
```

### Sample Data
- 5 products across different categories (Electronics, Books, Clothing, Home)
- Price range: $29.99 - $199.99
- All marked as active

## Persistence

Database data is persisted in a Docker volume named `mssql-data`:
- Data survives container restarts
- To reset: `docker-compose down -v`
- To backup: `docker volume inspect mssql-data`

## Connection String Format

```
Server=mssql,1433;Database=AdventureWorks2022;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;
```

For localhost (outside Docker):
```
Server=localhost,1433;Database=AdventureWorks2022;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;
```

## Health Check

The SQL Server container includes a health check that:
- Runs every 10 seconds
- Executes `SELECT 1` to verify connectivity
- Has a 60-second startup period
- Marks container unhealthy after 5 failed retries

The .NET application waits for `service_healthy` status before starting.