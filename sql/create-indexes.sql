/*
alter table dbo.CUSTOMER add constraint pk__CUSTOMER primary key nonclustered (C_CUSTKEY) 
alter table dbo.NATION add constraint pk__NATION primary key nonclustered (N_NATIONKEY) 
alter table dbo.REGION add constraint pk__REGION primary key nonclustered (R_REGIONKEY) 
*/

/*
create unique nonclustered index IX1 on dbo.CUSTOMER (C_CUSTKEY) 
create unique nonclustered index IX1 on dbo.LINEITEM (L_ORDERKEY, L_LINENUMBER) 
create unique nonclustered index IX1 on dbo.NATION (N_NATIONKEY) 
create unique nonclustered index IX1 on dbo.ORDERS (O_ORDERKEY) 
create unique nonclustered index IX1 on dbo.PART (P_PARTKEY) 
create unique nonclustered index IX1 on dbo.PARTSUPP (PS_PARTKEY, PS_SUPPKEY) 
create unique nonclustered index IX1 on dbo.REGION (R_REGIONKEY) 
create unique nonclustered index IX1 on dbo.SUPPLIER (S_SUPPKEY) 
*/