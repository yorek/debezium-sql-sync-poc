use tempdb
go

alter database [WideWorldImporters] set single_user with rollback immediate
go

drop database [WideWorldImporters]
go

restore database [WideWorldImporters] 
from disk = N'D:\_mssql\MSSQL14.MSSQLSERVER\MSSQL\Backup\WideWorldImporters-Full.bak' 
with
move N'WWI_Primary' to N'D:\_mssql\MSSQL14.MSSQLSERVER\MSSQL\DATA\WideWorldImporters.mdf',  
move N'WWI_UserData' to N'D:\_mssql\MSSQL14.MSSQLSERVER\MSSQL\DATA\WideWorldImporters_UserData.ndf',
move N'WWI_Log' to N'D:\_mssql\MSSQL14.MSSQLSERVER\MSSQL\DATA\WideWorldImporters.ldf',  
move N'WWI_InMemory_Data_1' to N'D:\_mssql\MSSQL14.MSSQLSERVER\MSSQL\DATA\WideWorldImporters_InMemory_Data_1',  
replace,
recovery,
stats = 5
go

select recovery_model_desc from sys.databases where [name] = 'WideWorldImporters'
go

alter database [WideWorldImporters] set recovery full
go

backup database [WideWorldImporters] 
to disk = 'd:\_mssql\MSSQL14.MSSQLSERVER\MSSQL\Backup\WWI-Migration-full-0.bak' 
with init, compression
go

use [WideWorldImporters]
go

update [Warehouse].[StockItems] set MarketingComments = 'Test' where StockItemId = 1
go

backup log [WideWorldImporters] 
to disk = 'd:\_mssql\MSSQL14.MSSQLSERVER\MSSQL\Backup\WWI-Migration-log-1.bak'
go

exec sys.sp_cdc_enable_db
go

exec sys.sp_cdc_enable_table N'Warehouse', N'StockItems', @role_name=null, @supports_net_changes=0
go

update [Warehouse].[StockItems] set MarketingComments = 'Test' where StockItemId = 2
go

backup log [WideWorldImporters] 
to disk = 'd:\_mssql\MSSQL14.MSSQLSERVER\MSSQL\Backup\WWI-Migration-log-2.bak'
go
