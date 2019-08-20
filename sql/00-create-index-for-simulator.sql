use TPCH
go

create nonclustered index ix1 on dbo.CUSTOMER (C_CUSTKEY)
go

create nonclustered index ix1 on dbo.ORDERS (O_ORDERKEY)
go

create nonclustered index ix1 on dbo.LINEITEM (L_ORDERKEY, L_LINENUMBER)
go
