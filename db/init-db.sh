#!/bin/bash

# Wait for SQL Server to be ready
echo "Waiting for SQL Server to start..."
sleep 10s

# Function to check if SQL Server is ready
check_sql_ready() {
    /opt/mssql-tools18/bin/sqlcmd -S mssql -U sa -P "$SA_PASSWORD" -C -Q "SELECT 1" > /dev/null 2>&1
    return $?
}

# Wait until SQL Server is ready (max 60 seconds)
for i in {1..12}; do
    if check_sql_ready; then
        echo "SQL Server is ready!"
        break
    fi
    echo "Waiting for SQL Server to be ready... ($i/12)"
    sleep 5
done

# Check if the database already exists
DB_EXISTS=$(/opt/mssql-tools18/bin/sqlcmd -S mssql -U sa -P "$SA_PASSWORD" -C -Q "SELECT name FROM sys.databases WHERE name = 'AdventureWorks2022'" -h -1 -W | grep -c "AdventureWorks2022")

if [ "$DB_EXISTS" -eq "0" ]; then
    echo "AdventureWorks2022 database does not exist. Checking for backup..."
    
    # Check if backup file exists
    if [ -f "/scripts/AdventureWorks2022.bak" ]; then
        echo "Backup file found. Starting restore..."
        
        # First, get the logical file names from the backup
        echo "Getting logical file names from backup..."
        /opt/mssql-tools18/bin/sqlcmd -S mssql -U sa -P "$SA_PASSWORD" -C -Q "RESTORE FILELISTONLY FROM DISK = '/scripts/AdventureWorks2022.bak';" -o /tmp/filelist.txt
        
        # Try to extract logical names (this is a simplified approach)
        DATA_FILE=$(grep -i "\.mdf" /tmp/filelist.txt | awk '{print $1}' | head -1)
        LOG_FILE=$(grep -i "\.ldf" /tmp/filelist.txt | awk '{print $1}' | head -1)
        
        if [ -z "$DATA_FILE" ]; then
            DATA_FILE="AdventureWorks2022"
        fi
        if [ -z "$LOG_FILE" ]; then
            LOG_FILE="AdventureWorks2022_log"
        fi
        
        echo "Attempting restore with data file: $DATA_FILE and log file: $LOG_FILE"
        
        # Restore the database
        /opt/mssql-tools18/bin/sqlcmd -S mssql -U sa -P "$SA_PASSWORD" -C -Q "
        RESTORE DATABASE AdventureWorks2022
        FROM DISK = '/scripts/AdventureWorks2022.bak'
        WITH MOVE '$DATA_FILE' TO '/var/opt/mssql/data/AdventureWorks2022.mdf',
             MOVE '$LOG_FILE' TO '/var/opt/mssql/data/AdventureWorks2022_Log.ldf',
             REPLACE;
        "
        
        if [ $? -eq 0 ]; then
            echo "Database restored successfully!"
        else
            echo "Error: Database restore failed. Creating sample database instead..."
            # Create sample database (fallback)
            /opt/mssql-tools18/bin/sqlcmd -S mssql -U sa -P "$SA_PASSWORD" -C -Q "CREATE DATABASE AdventureWorks2022;"
            
            /opt/mssql-tools18/bin/sqlcmd -S mssql -U sa -P "$SA_PASSWORD" -C -d AdventureWorks2022 -Q "
            CREATE TABLE Products (
                ProductID INT PRIMARY KEY IDENTITY(1,1),
                Name NVARCHAR(100) NOT NULL,
                Description NVARCHAR(500),
                Price DECIMAL(10,2) NOT NULL,
                Category NVARCHAR(50),
                CreatedDate DATETIME DEFAULT GETDATE(),
                IsActive BIT DEFAULT 1
            );
            
            INSERT INTO Products (Name, Description, Price, Category, IsActive)
            VALUES 
                ('Product 1', 'Sample product 1', 99.99, 'Electronics', 1),
                ('Product 2', 'Sample product 2', 149.99, 'Electronics', 1),
                ('Product 3', 'Sample product 3', 29.99, 'Books', 1),
                ('Product 4', 'Sample product 4', 79.99, 'Clothing', 1),
                ('Product 5', 'Sample product 5', 199.99, 'Home', 1);
            "
            echo "Sample database created!"
        fi
    else
        echo "Backup file not found. Creating a sample database..."
        
        # Create a simple database with sample data if backup doesn't exist
        /opt/mssql-tools18/bin/sqlcmd -S mssql -U sa -P "$SA_PASSWORD" -C -Q "CREATE DATABASE AdventureWorks2022;"
        
        /opt/mssql-tools18/bin/sqlcmd -S mssql -U sa -P "$SA_PASSWORD" -C -d AdventureWorks2022 -Q "
        CREATE TABLE Products (
            ProductID INT PRIMARY KEY IDENTITY(1,1),
            Name NVARCHAR(100) NOT NULL,
            Description NVARCHAR(500),
            Price DECIMAL(10,2) NOT NULL,
            Category NVARCHAR(50),
            CreatedDate DATETIME DEFAULT GETDATE(),
            IsActive BIT DEFAULT 1
        );
        
        INSERT INTO Products (Name, Description, Price, Category, IsActive)
        VALUES 
            ('Product 1', 'Sample product 1', 99.99, 'Electronics', 1),
            ('Product 2', 'Sample product 2', 149.99, 'Electronics', 1),
            ('Product 3', 'Sample product 3', 29.99, 'Books', 1),
            ('Product 4', 'Sample product 4', 79.99, 'Clothing', 1),
            ('Product 5', 'Sample product 5', 199.99, 'Home', 1);
        "
        
        echo "Sample database created successfully!"
    fi
else
    echo "AdventureWorks2022 database already exists. Skipping initialization."
fi

echo "Database initialization completed!"
