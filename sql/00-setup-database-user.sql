USE [master]
GO
CREATE LOGIN [debezium-tpch] WITH PASSWORD = 'debezium-TPCH-P@ssw0rd!'
GO
USE [TPCH]
GO
CREATE USER [debezium-tpch] FROM LOGIN [debezium-tpch]
GO
ALTER ROLE [db_owner] ADD MEMBER [debezium-tpch]
GO