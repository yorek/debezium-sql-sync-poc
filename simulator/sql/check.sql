create nonclustered index ix1 on dbo.LINEITEM (L_ORDERKEY)

create nonclustered index ix1 on dbo.ORDERS (O_CUSTKEY)

create nonclustered index ix1 on dbo.CUSTOMER (C_CUSTKEY)

/*
delete from dbo.CUSTOMER where C_CUSTKEY > 1500000 
delete from dbo.ORDERS where O_ORDERKEY > 60000000
delete from dbo.LINEITEM where L_ORDERKEY > 60000000 
*/



select top 100 * from dbo.CUSTOMER where C_CUSTKEY > 1500000 order by C_CUSTKEY desc
select top 100 * from dbo.ORDERS where O_ORDERKEY > 60000000 order by O_ORDERKEY desc
select top 100 * from dbo.LINEITEM where L_ORDERKEY > 60000000 order by L_ORDERKEY desc

select count(*) from dbo.CUSTOMER where C_CUSTKEY > 1500000
select count(*) from dbo.ORDERS where O_ORDERKEY > 60000000
select count(*) from dbo.LINEITEM where L_ORDERKEY > 60000000