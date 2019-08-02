insert into dbo.REGION ([R_REGIONKEY], [R_NAME], [R_COMMENT]) values (99, 'Unknown', 'Unkown');
insert into dbo.NATION ([N_NATIONKEY], [N_NAME], [N_REGIONKEY], [N_COMMENT]) values (99, 'Unkown', 99, 'Unkown')

insert into dbo.REGION ([R_REGIONKEY], [R_NAME], [R_COMMENT]) values (100, 'Another Test', 'Another Test')
insert into dbo.NATION ([N_NATIONKEY], [N_NAME], [N_REGIONKEY], [N_COMMENT]) values (100, 'Another Test', 100, 'Another Test')

insert into dbo.CUSTOMER ([C_CUSTKEY], [C_NAME], [C_ADDRESS], [C_NATIONKEY], [C_PHONE], [C_ACCTBAL], [C_MKTSEGMENT], [C_COMMENT])
values (9900001, 'Davide Mauri', '1234 NE 123th St', '99', '12-234-345-345', 1234.56, 'COMPUTER', 'First Time Customer')
 
insert into dbo.ORDERS ([O_ORDERKEY], [O_CUSTKEY], [O_ORDERSTATUS], [O_TOTALPRICE], [O_ORDERDATE], [O_ORDERPRIORITY], [O_CLERK], [O_SHIPPRIORITY], [O_COMMENT])
values (90000000, 9900001, 'O', 3456755.11, '2019-08-01', '1-URGENT', 'Clerk#000007284', 1, 'First order')

insert into dbo.ORDERS ([O_ORDERKEY], [O_CUSTKEY], [O_ORDERSTATUS], [O_TOTALPRICE], [O_ORDERDATE], [O_ORDERPRIORITY], [O_CLERK], [O_SHIPPRIORITY], [O_COMMENT])
values (90000001, 9900001, 'O', 3456755.11, '2019-08-01', '1-URGENT', 'Clerk#000007284', 1, 'Second order')

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
