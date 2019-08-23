use tempdb
go

restore database [TPCH1GB] with recovery, keep_cdc
go

ALTER AUTHORIZATION ON DATABASE::[TPCH1GB] TO [sa]
go

use [TPCH1GB]
go

exec sys.sp_cdc_add_job 'capture'
GO
exec sys.sp_cdc_add_job 'cleanup'
GO


EXEC sys.sp_cdc_help_change_data_capture
GO

select * from dbo.NATION
go

insert into dbo.NATION values
(106, 'TEST', 0, 'TEST'),
(107, 'TEST', 0, 'TEST'),
(108, 'TEST', 0, 'TEST'),
(109, 'TEST', 0, 'TEST')

SELECT sys.fn_cdc_get_max_lsn()


DELETE FROM dbo.NATION where N_NATIONKEY >= 100