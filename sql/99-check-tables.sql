use TPCH
go

select 
	t.[name], 
	ps.index_id, 
	COUNT(ps.partition_number) as partition_number, 
	SUM(ps.row_count) as row_count, 
	SUM(ps.used_page_count) as used_page_count,
	SUM(ps.used_page_count) * 8 / 1024. / 1024 as used_page_gb
from 
	sys.dm_db_partition_stats ps 
inner join 
	sys.tables t on ps.object_id = t.object_id
where 
	t.is_ms_shipped = 0
and
	ps.index_id in (0,1)
group by
	t.[name],
	ps.index_id
go