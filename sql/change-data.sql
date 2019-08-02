use TPC-H

update dbo.NATION set N_COMMENT = 'Another Test' where N_NATIONKEY = 99
update dbo.REGION set R_COMMENT = 'Another Test' where R_REGIONKEY = 99

update dbo.CUSTOMER set C_COMMENT = 'Using Debezium 0.10 Again, No indexes this time' where C_CUSTKEY = 9500001

select top 100 * from dbo.CUSTOMER order by C_CUSTKEY desc
select top 100 * from dbo.NATION 
select top 100 * from dbo.REGION
select top 100 * from dbo.ORDERS order by O_ORDERKEY desc

update dbo.CUSTOMER set C_COMMENT = 'Comment 1' where C_CUSTKEY = 9900001
update dbo.CUSTOMER set C_COMMENT = 'Comment 2' where C_CUSTKEY = 9900001
update dbo.CUSTOMER set C_COMMENT = 'Comment 3' where C_CUSTKEY = 9900001
update dbo.CUSTOMER set C_COMMENT = 'Comment 4' where C_CUSTKEY = 9900001
update dbo.CUSTOMER set C_COMMENT = 'Comment 5' where C_CUSTKEY = 9900001
update dbo.CUSTOMER set C_COMMENT = 'Comment 6' where C_CUSTKEY = 9900001

begin tran
	update dbo.CUSTOMER set C_COMMENT = 'Final Comment' where C_CUSTKEY = 9900001
commit

delete from dbo.NATION where N_NATIONKEY >= 99
delete from dbo.REGION where R_REGIONKEY >= 99
delete from dbo.CUSTOMER where C_CUSTKEY >= 9900001
delete from dbo.ORDERS where O_ORDERKEY >= 90000000
