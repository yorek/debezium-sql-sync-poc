use TPCH
GO

EXEC sys.sp_cdc_enable_db
GO

/*
select 'EXEC sys.sp_cdc_enable_table N''' + schema_name(schema_id) + ''', N''' + [name] + ''', @index_name = ''IX1'', role_name=null, @supports_net_changes=1' 
from sys.tables
where type = 'U'
GO
*/

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
