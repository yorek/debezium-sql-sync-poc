# Debezium Azure SQL Sync

POC for keeping an Azure SQL in Sync with an on-prem source using Debezium and EventHubs

## Workflow to keep a SQL Server database in sync to Azure SQL

1. Activate CDC on the database that needs to be moved. There is no need to support "net changes" so even tables without PK will work fine
2. Use Debezium 0.10 on-prem and point it to an already created Event Hub with at least 7 days of retention
3. Wait for 1 to finish (log will tell "Snapshot step 8 - Finalizing") and then create a database snapshot of the database that needs to be moved to HS
4. Generate the database schema, without FK, and create an empty database in Azure SQL
5. Export all data from database snapshot and BULK LOAD the data into Azure SQL
6. Once BULK LOAD is finished, create all FK on HS database
7. Get the last LSN from the database snapshot take at 3 (sys.fn_cdc_get_max_lsn())
8. Execute an application that will consume data from EventHub and generate INSERT/DELETE/UPDATE commands for all transaction with commited lsn (it is a metadata stored in the pyload that Debezium creates) greater then the one found in 7.
9. Leave the application running until the two database are in sync
10. Stop database on-prem. Migration Done!

The step are in this sequence due to the fact that Debezium will monitor changes on from the LSN it finds when it performs its own "snapshot", which is actually skipped for VLDB via the provided json config file.
This means that to be sure not to lose any changes, debezium needs to be started (1) BEFORE the sql server database snapshot is taken (2).
During this amout of time, changes can be done to the database and they will end up in debezium AND in the database snapshot.
The last LSN of the database snapshot can be used int the replay tool to skip the transaction found in debezium that are already in the snapsho and thus in the Azure SQL database.

## Step-by-Step instructions

The followin instruction use EventHub as the Kafka Endpoint and Docker Container to run Debezium: is the simplest configuration possible. For alternative solution see here. (TODO)

### Create an EventHub

### Run Debezium

### Activate CDC on source database

### Create source database snapshot

### Generate source database schema

### Bulk Export data from all tables

### Copy data to an Azure Blob Store

### Bulk Import data into Azure SQL

### Get last LSN from database snapshot

### Run the Sync application

### Wait for the database to be in sync

### Done!
