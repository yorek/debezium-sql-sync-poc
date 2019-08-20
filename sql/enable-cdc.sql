
/* 
	STEP 1
*/
-- start debezium
-- register sql server connector
-- wait for debezium snapshot to complete

/*
	POST-STEP 1 
*/
insert into dbo.REGION ([R_REGIONKEY], [R_NAME], [R_COMMENT]) values (99, 'Unknown', 'Unkown');
insert into dbo.NATION ([N_NATIONKEY], [N_NAME], [N_REGIONKEY], [N_COMMENT]) values (99, 'Unkown', 99, 'Unkown')

insert into dbo.REGION ([R_REGIONKEY], [R_NAME], [R_COMMENT]) values (100, 'Another Test', 'Another Test')
insert into dbo.NATION ([N_NATIONKEY], [N_NAME], [N_REGIONKEY], [N_COMMENT]) values (100, 'Another Test', 100, 'Another Test')

insert into dbo.CUSTOMER ([C_CUSTKEY], [C_NAME], [C_ADDRESS], [C_NATIONKEY], [C_PHONE], [C_ACCTBAL], [C_MKTSEGMENT], [C_COMMENT])
values (9900001, 'Davide Mauri', '1234 NE 123th St', '99', '12-234-345-345', 1234.56, 'COMPUTER', 'First Time Customer')

select * from cdc.dbo_REGION_CT
select * from cdc.dbo_NATION_CT
select * from cdc.dbo_CUSTOMER_CT


/*
	STEP 3 TO 5
*/
-- Skipped as not related to this file

/*
	STEP 6
*/
use TPCH_SS1
go

SELECT sys.fn_cdc_get_max_lsn()
GO

/*
	POST-STEP 6
*/
-- do some changes
use TPCH
go

insert into dbo.ORDERS ([O_ORDERKEY], [O_CUSTKEY], [O_ORDERSTATUS], [O_TOTALPRICE], [O_ORDERDATE], [O_ORDERPRIORITY], [O_CLERK], [O_SHIPPRIORITY], [O_COMMENT])
values (90000000, 9900001, 'O', 3456755.11, '2019-08-01', '1-URGENT', 'Clerk#000007284', 1, 'First order')

insert into dbo.ORDERS ([O_ORDERKEY], [O_CUSTKEY], [O_ORDERSTATUS], [O_TOTALPRICE], [O_ORDERDATE], [O_ORDERPRIORITY], [O_CLERK], [O_SHIPPRIORITY], [O_COMMENT])
values (90000001, 9900001, 'O', 3456755.11, '2019-08-01', '1-URGENT', 'Clerk#000007284', 1, 'Second order')

delete from dbo.REGION where R_REGIONKEY = 100
delete from dbo.NATION where N_NATIONKEY = 100

select * from cdc.dbo_ORDERS_CT
select * from cdc.dbo_REGION_CT
select * from cdc.dbo_NATION_CT

--select count(*) from dbo.ORDERS

/*
	Disable CDC
*/

-- Do this if only want to disable CDC on a specific table
--EXEC sys.sp_cdc_disable_table 'dbo', 'CUSTOMER', 'dbo_CUSTOMER'
--GO

EXEC sys.sp_cdc_disable_db 

/*
use the following to restore the database to snapshot state
with CDC enabled and before and change done
*/
use tempdb
alter database TPCH  set single_user with rollback immediate
restore database TPCH from database_snapshot = 'TPCH_SS1' with keep_cdc
use TPCH
exec sys.sp_cdc_add_job 'capture'
exec sys.sp_cdc_add_job 'cleanup'
GO