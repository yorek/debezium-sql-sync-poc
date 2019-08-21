USE [master]
GO

RESTORE DATABASE [WideWorldImporters] 
FROM DISK = N'F:\Backup\WWI-Migration-full-0.bak' 
WITH
MOVE N'WWI_Primary' TO N'F:\Data\WideWorldImporters.mdf',  
MOVE N'WWI_UserData' TO N'F:\Data\WideWorldImporters_UserData.ndf',  
MOVE N'WWI_Log' TO N'F:\Log\WideWorldImporters.ldf',  
MOVE N'WWI_InMemory_Data_1' TO N'F:\Data\WideWorldImporters_InMemory_Data_1',  
REPLACE,  
STATS = 10,
STANDBY = N'F:\Backup\ROLLBACK_UNDO_WideWorldImporters.BAK'
GO

RESTORE LOG [WideWorldImporters] FROM  DISK = N'F:\Backup\WWI-Migration-log-1.bak' WITH   
STANDBY = N'F:\Backup\ROLLBACK_UNDO_WideWorldImporters.BAK',  
STATS = 10
GO

RESTORE LOG [WideWorldImporters] FROM  DISK = N'F:\Backup\WWI-Migration-log-2.bak' WITH   
STANDBY = N'F:\Backup\ROLLBACK_UNDO_WideWorldImporters.BAK',  
STATS = 10
GO

/*
create database WWI_SS on
(name = 'WWI_Primary', filename = 'F:\Data\WWI_SS_Primary.mdf.ss'),
(name = 'WWI_UserData', filename = 'F:\Data\WWI_SS_UserData.ndf.ss')
as snapshot of WideWorldImporters
*/


