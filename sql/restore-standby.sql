use tempdb
go

alter database [TPCH1GB] set single_user with rollback immediate
go

drop database [TPCH1GB]
go

RESTORE DATABASE [TPCH1GB] 
FROM  
	DISK = N'F:\Backup\TPCH1GB-Migration-Full-0.bak' 
WITH  
	MOVE N'TPCH_sys' TO N'F:\Data\TPCH1GB_sys.mdf',  
	MOVE N'TPCH_data' TO N'F:\Data\TPCH1GB_data.ndf',  
	MOVE N'TPCH_log' TO N'F:\Log\TPCH1GB_log.ldf',  
	STANDBY = N'F:\Backup\ROLLBACK_UNDO_TPCH1GB.bak', 
	STATS = 10,
	REPLACE
GO


RESTORE LOG [TPCH1GB] 
FROM  
	DISK = N'F:\Backup\TPCH1GB-Migration-Log-1.bak' 
WITH  
	STANDBY = N'F:\Backup\ROLLBACK_UNDO_TPCH1GB.BAK',  
	STATS = 10
GO

SELECT TPCH1GB.sys.fn_cdc_get_max_lsn()
go

alter database [TPCH1GB] set single_user with rollback immediate
go

RESTORE LOG [TPCH1GB] 
FROM  
	DISK = N'F:\Backup\TPCH1GB-Migration-Log-2.bak' 
WITH  
	STANDBY = N'F:\Backup\ROLLBACK_UNDO_TPCH1GB.BAK',  
	STATS = 10
GO
