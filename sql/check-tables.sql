use TPCH
go

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
