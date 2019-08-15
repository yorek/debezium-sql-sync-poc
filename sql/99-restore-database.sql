alter database TPCH set single_user with rollback immediate
go
restore database TPCH from disk = 'd:\_mssql\MSSQL14.MSSQLSERVER\MSSQL\Backup\TPCH.bak' with replace, recovery, stats = 10