use TPCH
GO

select 
	t.name,
	ps.index_id,
	ps.partition_number,
	ps.row_count
from
	sys.dm_db_partition_stats ps 
inner join
	sys.tables t on t.object_id = ps.object_id
where 
	ps.index_id = 0
go


EXEC sys.sp_cdc_enable_db
GO

select 'EXEC sys.sp_cdc_enable_table N''' + schema_name(schema_id) + ''', N''' + [name] + ''', @index_name = ''IX1'', role_name=null, @supports_net_changes=1' 
from sys.tables
where type = 'U'
GO

EXEC sys.sp_cdc_enable_table N'dbo', N'NATION', @role_name=null, @supports_net_changes=0
EXEC sys.sp_cdc_enable_table N'dbo', N'REGION', @role_name=null, @supports_net_changes=0
EXEC sys.sp_cdc_enable_table N'dbo', N'PART', @role_name=null, @supports_net_changes=0
EXEC sys.sp_cdc_enable_table N'dbo', N'SUPPLIER', @role_name=null, @supports_net_changes=0
EXEC sys.sp_cdc_enable_table N'dbo', N'PARTSUPP', @role_name=null, @supports_net_changes=0
EXEC sys.sp_cdc_enable_table N'dbo', N'CUSTOMER', @role_name=null, @supports_net_changes=0
EXEC sys.sp_cdc_enable_table N'dbo', N'ORDERS', @role_name=null, @supports_net_changes=0
EXEC sys.sp_cdc_enable_table N'dbo', N'LINEITEM', @role_name=null, @supports_net_changes=0

EXEC sys.sp_cdc_help_change_data_capture
GO

CREATE DATABASE TPCH_SS1 ON
(NAME = 'TPCH_sys', FILENAME = 'D:\_mssql\MSSQL14.MSSQLSERVER\MSSQL\DATA\TPCH_sys.mdf.ss1'),
(NAME = 'TPCH_data', FILENAME = 'D:\_mssql\MSSQL14.MSSQLSERVER\MSSQL\DATA\TPCH_data.ndf.ss1')
AS SNAPSHOT OF TPCH
GO

SELECT sys.fn_cdc_get_max_lsn()
GO

exec sp_whoisactive @get_plans = 1
go

select * from cdc.dbo_NATION_CT
select * from cdc.dbo_REGION_CT
select * from cdc.dbo_CUSTOMER_CT
select * from cdc.dbo_ORDERS_CT

--select count(*) from dbo.ORDERS

--EXEC sys.sp_cdc_disable_table 'dbo', 'CUSTOMER', 'dbo_CUSTOMER'
--EXEC sys.sp_cdc_disable_table 'dbo', 'LINEITEM', 'dbo_LINEITEM'
--EXEC sys.sp_cdc_disable_table 'dbo', 'ORDERS', 'dbo_ORDERS'
--GO

--EXEC sys.sp_cdc_disable_db 

--use tempdb
--alter database TPCH  set single_user with rollback immediate
--restore database TPCH from database_snapshot = 'TPCH_SS1' with keep_cdc
--use TPCH
--exec sys.sp_cdc_add_job 'capture'
--exec sys.sp_cdc_add_job 'cleanup'
GO