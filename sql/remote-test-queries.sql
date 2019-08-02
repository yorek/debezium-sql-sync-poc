select 
	t.name,
	ps.row_count
from
	sys.dm_db_partition_stats ps 
inner join
	sys.tables t on t.object_id = ps.object_id
go

select top 100 * from dbo.CUSTOMER order by C_CUSTKEY desc
select top 100 * from dbo.NATION 
select top 100 * from dbo.REGION
select top 100 * from dbo.ORDERS order by O_ORDERKEY desc


delete from dbo.NATION where N_NATIONKEY >= 99
delete from dbo.REGION where R_REGIONKEY >= 99
delete from dbo.CUSTOMER where C_CUSTKEY >= 9900001
delete from dbo.ORDERS where O_ORDERKEY >= 9900000

select count(*) from dbo.ORDERS


--BULK INSERT dbo.ORDERS FROM 'tpch/10GB/orders.tbl' WITH (TABLOCK, DATA_SOURCE = 'Azure-Storage', FIELDTERMINATOR = '|', BATCHSIZE=100000)
