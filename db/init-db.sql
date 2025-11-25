RESTORE DATABASE AdventureWorks
FROM DISK = '/var/opt/mssql/backup/AdventureWorks2022.bak'
WITH MOVE 'AdventureWorks2022_Data' TO '/var/opt/mssql/data/AdventureWorks2022.mdf',
     MOVE 'AdventureWorks2022_Log'  TO '/var/opt/mssql/data/AdventureWorks2022_Log.ldf',
     REPLACE;
GO
