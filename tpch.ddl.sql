/****** Object:  User [cdc]    Script Date: 8/19/2019 2:37:42 PM ******/
CREATE USER [cdc] WITHOUT LOGIN WITH DEFAULT_SCHEMA=[cdc]
GO
/****** Object:  User [debezium]    Script Date: 8/19/2019 2:37:42 PM ******/
CREATE USER [debezium] FOR LOGIN [debezium] WITH DEFAULT_SCHEMA=[dbo]
GO
/****** Object:  User [debezium-tpch]    Script Date: 8/19/2019 2:37:42 PM ******/
CREATE USER [debezium-tpch] FOR LOGIN [debezium-tpch] WITH DEFAULT_SCHEMA=[dbo]
GO
/****** Object:  User [striim-tpch]    Script Date: 8/19/2019 2:37:42 PM ******/
CREATE USER [striim-tpch] FOR LOGIN [striim-tpch] WITH DEFAULT_SCHEMA=[dbo]
GO
sys.sp_addrolemember @rolename = N'db_owner', @membername = N'cdc'
GO
sys.sp_addrolemember @rolename = N'db_owner', @membername = N'debezium'
GO
sys.sp_addrolemember @rolename = N'db_owner', @membername = N'debezium-tpch'
GO
sys.sp_addrolemember @rolename = N'db_owner', @membername = N'striim-tpch'
GO
/****** Object:  Schema [cdc]    Script Date: 8/19/2019 2:37:42 PM ******/
CREATE SCHEMA [cdc]
GO
/****** Object:  Table [dbo].[CUSTOMER]    Script Date: 8/19/2019 2:37:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CUSTOMER](
	[C_CUSTKEY] [int] NOT NULL,
	[C_NAME] [varchar](25) NOT NULL,
	[C_ADDRESS] [varchar](40) NOT NULL,
	[C_NATIONKEY] [int] NOT NULL,
	[C_PHONE] [char](15) NOT NULL,
	[C_ACCTBAL] [decimal](15, 2) NOT NULL,
	[C_MKTSEGMENT] [char](10) NOT NULL,
	[C_COMMENT] [varchar](117) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LINEITEM]    Script Date: 8/19/2019 2:37:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LINEITEM](
	[L_ORDERKEY] [int] NOT NULL,
	[L_PARTKEY] [int] NOT NULL,
	[L_SUPPKEY] [int] NOT NULL,
	[L_LINENUMBER] [int] NOT NULL,
	[L_QUANTITY] [decimal](15, 2) NOT NULL,
	[L_EXTENDEDPRICE] [decimal](15, 2) NOT NULL,
	[L_DISCOUNT] [decimal](15, 2) NOT NULL,
	[L_TAX] [decimal](15, 2) NOT NULL,
	[L_RETURNFLAG] [char](1) NOT NULL,
	[L_LINESTATUS] [char](1) NOT NULL,
	[L_SHIPDATE] [date] NOT NULL,
	[L_COMMITDATE] [date] NOT NULL,
	[L_RECEIPTDATE] [date] NOT NULL,
	[L_SHIPINSTRUCT] [char](25) NOT NULL,
	[L_SHIPMODE] [char](10) NOT NULL,
	[L_COMMENT] [varchar](44) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NATION]    Script Date: 8/19/2019 2:37:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NATION](
	[N_NATIONKEY] [int] NOT NULL,
	[N_NAME] [char](25) NOT NULL,
	[N_REGIONKEY] [int] NOT NULL,
	[N_COMMENT] [varchar](152) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ORDERS]    Script Date: 8/19/2019 2:37:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ORDERS](
	[O_ORDERKEY] [int] NOT NULL,
	[O_CUSTKEY] [int] NOT NULL,
	[O_ORDERSTATUS] [char](1) NOT NULL,
	[O_TOTALPRICE] [decimal](15, 2) NOT NULL,
	[O_ORDERDATE] [date] NOT NULL,
	[O_ORDERPRIORITY] [char](15) NOT NULL,
	[O_CLERK] [char](15) NOT NULL,
	[O_SHIPPRIORITY] [int] NOT NULL,
	[O_COMMENT] [varchar](79) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PART]    Script Date: 8/19/2019 2:37:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PART](
	[P_PARTKEY] [int] NOT NULL,
	[P_NAME] [varchar](55) NOT NULL,
	[P_MFGR] [char](25) NOT NULL,
	[P_BRAND] [char](10) NOT NULL,
	[P_TYPE] [varchar](25) NOT NULL,
	[P_SIZE] [int] NOT NULL,
	[P_CONTAINER] [char](10) NOT NULL,
	[P_RETAILPRICE] [decimal](15, 2) NOT NULL,
	[P_COMMENT] [varchar](23) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PARTSUPP]    Script Date: 8/19/2019 2:37:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PARTSUPP](
	[PS_PARTKEY] [int] NOT NULL,
	[PS_SUPPKEY] [int] NOT NULL,
	[PS_AVAILQTY] [int] NOT NULL,
	[PS_SUPPLYCOST] [decimal](15, 2) NOT NULL,
	[PS_COMMENT] [varchar](199) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[REGION]    Script Date: 8/19/2019 2:37:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[REGION](
	[R_REGIONKEY] [int] NOT NULL,
	[R_NAME] [char](25) NOT NULL,
	[R_COMMENT] [varchar](152) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SUPPLIER]    Script Date: 8/19/2019 2:37:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SUPPLIER](
	[S_SUPPKEY] [int] NOT NULL,
	[S_NAME] [char](25) NOT NULL,
	[S_ADDRESS] [varchar](40) NOT NULL,
	[S_NATIONKEY] [int] NOT NULL,
	[S_PHONE] [char](15) NOT NULL,
	[S_ACCTBAL] [decimal](15, 2) NOT NULL,
	[S_COMMENT] [varchar](101) NOT NULL
) ON [PRIMARY]
GO
