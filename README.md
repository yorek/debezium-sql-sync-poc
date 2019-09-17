# Debezium Azure SQL Sync

POC for keeping an Azure SQL in Sync with an on-prem source using Debezium and EventHubs

## Option 1: SQL Server to Azure SQL DB

1. Activate CDC on the database that needs to be moved. There is no need to support "net changes" so even tables without PK will work fine.
2. Generate the database schema without FK and indexes, and create an empty database in HS. Schema can be generated used DMA or mssql-tool.
3. Start Debezium 0.10 on-prem or on the cloud and point it to an already created Event Hub with at least 7 days of retention and with Kafka endpoint enabled. Wait for Debezium to be fully started and its snapshot finished.
4. Create a database snapshot of the database that needs to be moved to HS
5. Export all data from database snapshot and BULK LOAD the data into HS. Use the Smart Bulk Copy tool for the best performance with HS.
6. Once BULK LOAD is finished, create all FK and indexes on HS database
7. Get the last LSN from the database snapshot take at 2 (sys.fn_cdc_get_max_lsn())
8. Create/Execute an application that will consume data from EventHub and generate INSERT/DELETE/UPDATE commands for all transaction with committed lsn (it is a metadata stored in the pyload that Debezium creates) greater than the one found in 6.
9. Stop database on-prem. Leave the application running until the two databases are in sync
10. Migration Done!

## Option 2: SQL Server to Azure SQL VM to Azure SQL DB

1. Activate CDC on the database that needs to be moved. There is no need to support "net changes" so even tables without PK will work fine.
2. Take a FULL backup
3. Restore the FULL backup to an Azure SQL Server 2017 running in a VM with STANDBY options.
4. Generate the database schema without FK and indexes, and create an empty database in HS. Schema can be generated used DMA or mssql-tool.
5. Export all data from database snapshot and BULK LOAD the data into HS. Use the Smart Bulk Copy tool for the best performance with HS.
6. Once BULK LOAD is finished, create all FK on HS database
7. Start Debezium 0.10 on Azure, configured to use the Azure SQL VM as the source, and point it to an already created Event Hub with at least 7 days of retention.
8. Manually initialize Debezium using the provided application (debezium_initializer)
9. Get the last LSN from the standby database (sys.fn_cdc_get_max_lsn())
10. Take a Differential/Log Backup on premises and restore it on Azure VM
11. Create/Execute an application that will consume data from EventHub and generate INSERT/DELETE/UPDATE commands for all transaction with committed LSN (it is a metadata stored in the payload that Debezium creates) greater than the one found in 8.
12. Go back to point 10 and repeat until the database are in sync.
