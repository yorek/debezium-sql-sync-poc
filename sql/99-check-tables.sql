use TPCH
go

select 
	t.name, 
	ps.index_id, 
	ps.partition_number, 
	ps.row_count, 
	ps.used_page_count,
	ps.used_page_count * 8 / 1024. / 1024 as used_page_gb
from 
	sys.dm_db_partition_stats ps 
inner join 
	sys.tables t on ps.object_id = t.object_id
where 
	t.is_ms_shipped = 0
go