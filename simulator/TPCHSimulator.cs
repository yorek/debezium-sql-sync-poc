using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using Bogus;
using System.Linq;

namespace Simulator
{    

    class TPCHSimulator
    {
        enum Simulate {
            Order,
            Customer
        }

        private readonly string SQLConnectionString = "";

        private readonly Random Random = new Random();

        private readonly Faker Faker = new Faker();

        private readonly int StartingCustomerId = 0;

        private readonly int StartingOrderId = 0;

        private readonly int StartingPartId = 0;

        private readonly int StartingSupplierId = 0;

        private int CustomerId = 0;

        private int OrderId = 0;

        private int PartId = 0;

        private int SupplierId = 0;

        public TPCHSimulator()
        {
            DotNetEnv.Env.Load();

            Randomizer.Seed = new Random();

            this.SQLConnectionString = Environment.GetEnvironmentVariable("ConnectionString");;
            var builder = new SqlConnectionStringBuilder(this.SQLConnectionString);
            Console.WriteLine($"Connecting to {builder.InitialCatalog}@{builder.DataSource}...");

            using (var conn = new SqlConnection(SQLConnectionString))
            {
                var result = conn.QuerySingle(@"
                    select 
                        MaxUsedCustomerId = (select MAX(C_CUSTKEY) from dbo.CUSTOMER), 
                        MaxUsedOrderId = (select MAX(O_ORDERKEY) from dbo.ORDERS),
                        MaxUsedPartId = (select MAX(P_PARTKEY) from dbo.PART),
                        MaxUsedSupplierId = (select MAX(S_SUPPKEY) from dbo.SUPPLIER)
                    ");
                this.CustomerId = result.MaxUsedCustomerId;
                this.OrderId = result.MaxUsedOrderId;
                this.PartId = result.MaxUsedSupplierId;
                this.SupplierId = result.MaxUsedPartId;

                this.StartingCustomerId = result.MaxUsedCustomerId + 1;
                this.StartingOrderId = result.MaxUsedOrderId + 1;
                this.StartingPartId = result.MaxUsedSupplierId + 1;
                this.StartingSupplierId = result.MaxUsedPartId + 1;                
            }
        }      

        public void SimulateActivity()
        {            
            var simulate = Simulate.Customer;

            while (true)
            {
                var num = (int)(Math.Round(Random.NextDouble() * 1000, 0));                        
                if (num > 000 && num <= 200) simulate = Simulate.Customer;
                //if (num > 200 && num <= 1000) simulate = Simulate.Order;

                if (simulate == Simulate.Customer)
                {
                    num = (int)(Math.Round(Random.NextDouble() * 1000, 0));                

                    if (num > 000 && num <= 600) CreateCustomer();
                    if (num > 600 && num <= 900) UpdateCustomer();
                    if (num > 900 && num <= 1000) DeleteCustomer();
                }

                if (simulate == Simulate.Order) {
                    num = (int)(Math.Round(Random.NextDouble() * 1000, 0));                

                    if (num > 000 && num <= 700) CreateOrder();
                    if (num > 700 && num <= 900) UpdateOrder();
                    if (num > 900 && num <= 1000) DeleteOrder();
                }
                
                /*
                if (num > 700 && num <= 900) CreateSupplier();
                if (num > 700 && num <= 900) UpdateSupplier();
                if (num > 700 && num <= 900) DeleteSupplier();
                if (num > 700 && num <= 900) CreatePart();
                if (num > 700 && num <= 900) UpdatePart();
                if (num > 700 && num <= 900) DeletePart();
                */
                var sleepMs = (int)(Math.Round(Random.NextDouble() * 3000, 0));
                Thread.Sleep(500 + sleepMs);
            }
        }

        private void CreateCustomer()
        {
            var cid = Interlocked.Increment(ref CustomerId);

            Log($"Creating Customer {CustomerId}...");

            using (var conn = new SqlConnection(SQLConnectionString))
            {
                var affectedRows = conn.Execute(
                     "INSERT INTO dbo.CUSTOMER ([C_CUSTKEY], [C_NAME], [C_ADDRESS], [C_NATIONKEY], [C_PHONE], [C_ACCTBAL], [C_MKTSEGMENT], [C_COMMENT]) VALUES (@CUSTKEY, @NAME, @ADDRESS, @NATIONKEY, @PHONE, @ACCTBAL, @MKTSEGMENT, @COMMENT)",
                     new
                     {
                         @CUSTKEY = cid,
                         @NAME = CutToMaxLength(Faker.Company.CompanyName(), 25),
                         @ADDRESS = CutToMaxLength(Faker.Address.FullAddress(), 40),
                         @NATIONKEY = Faker.Random.Int(0, 24),
                         @PHONE = Faker.Random.Int(0, 24),
                         @ACCTBAL = Faker.Random.Decimal(100, 10000),
                         @MKTSEGMENT = Faker.Random.ArrayElement(new string[] { "AUTOMOBILE", "BUILDING", "FURNITURE", "HOUSEHOLD", "MACHINERY" }),
                         @COMMENT = CutToMaxLength(Faker.Lorem.Sentence(5), 100)
                     });
            }
        }

        private void CreateOrder()
        {
            var oid = Interlocked.Increment(ref OrderId);
            var cid = Faker.Random.Int(1, CustomerId);

            using (var conn = new SqlConnection(SQLConnectionString))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {                    
                    Log($"Creating Order {oid}...");

                    var affectedRows = conn.Execute(
                        @"INSERT INTO dbo.ORDERS ([O_ORDERKEY], [O_CUSTKEY], [O_ORDERSTATUS], [O_TOTALPRICE], [O_ORDERDATE], [O_ORDERPRIORITY], [O_CLERK], [O_SHIPPRIORITY], [O_COMMENT])
                        VALUES (@ORDERKEY, @CUSTKEY, @ORDERSTATUS, @TOTALPRICE, @ORDERDATE, @ORDERPRIORITY, @CLERK, @SHIPPRIORITY, @COMMENT)",
                        new
                        {
                            @ORDERKEY = oid,
                            @CUSTKEY = cid,
                            @ORDERSTATUS = Faker.Random.ArrayElement(new string[] { "F", "O", "P" }),
                            @TOTALPRICE = Faker.Random.Decimal(100, 10000),
                            @ORDERDATE = Faker.Date.RecentOffset(15),
                            @ORDERPRIORITY = Faker.Random.ArrayElement(new string[] { "1-URGENT", "2-HIGH", "3-MEDIUM", "4-NOT SPECIFIED", "5-LOW" }),
                            @CLERK = String.Format("Clerk#{0:00000000}", Faker.Random.Number(1, 9999)),
                            @SHIPPRIORITY = 0,
                            @COMMENT = CutToMaxLength(Faker.Lorem.Sentence(5), 100)
                        },
                        tran
                        );

                    var num = 1 + (int)(Math.Round(Random.NextDouble() * 20, 0));                

                    Log($"Creating {num} LineItems for Order {oid}...");
                    
                    for(int lin=0; lin<num; lin++)
                    {
                        affectedRows = conn.Execute(
                            @"INSERT INTO dbo.LINEITEM ([L_ORDERKEY], [L_PARTKEY], [L_SUPPKEY], [L_LINENUMBER], [L_QUANTITY], [L_EXTENDEDPRICE], [L_DISCOUNT], [L_TAX], [L_RETURNFLAG], [L_LINESTATUS], [L_SHIPDATE], [L_COMMITDATE], [L_RECEIPTDATE], [L_SHIPINSTRUCT], [L_SHIPMODE], [L_COMMENT])
                            VALUES (@ORDERKEY, @PARTKEY, @SUPPKEY, @LINENUMBER, @QUANTITY, @EXTENDEDPRICE, @DISCOUNT, @TAX, @RETURNFLAG, @LINESTATUS, @SHIPDATE, @COMMITDATE, @RECEIPTDATE, @SHIPINSTRUCT, @SHIPMODE, @COMMENT)",
                            new
                            {
                                @ORDERKEY = oid,
                                @PARTKEY = Faker.Random.Int(1, PartId),
                                @SUPPKEY = Faker.Random.Int(1, SupplierId),
                                @LINENUMBER = lin + 1,
                                @QUANTITY = Faker.Random.Int(1, 50),
                                @EXTENDEDPRICE = Faker.Random.Decimal(900, 150000),
                                @DISCOUNT = Faker.Random.Decimal(0, 0.10m),
                                @TAX = Faker.Random.Decimal(0, 0.08m),
                                @RETURNFLAG = Faker.Random.ArrayElement(new string[] { "A", "N", "R" }),
                                @LINESTATUS = Faker.Random.ArrayElement(new string[] { "F", "O" }), 
                                @SHIPDATE = Faker.Date.RecentOffset(15), 
                                @COMMITDATE = Faker.Date.RecentOffset(15), 
                                @RECEIPTDATE = Faker.Date.RecentOffset(15), 
                                @SHIPINSTRUCT = Faker.Random.ArrayElement(new string[] { "COLLECT COD", "DELIVER IN PERSON", "TAKE BACK RETURN" }), 
                                @SHIPMODE = Faker.Random.ArrayElement(new string[] { "AIR", "FOB", "MAIL", "RAIL", "REG AIR", "SHIP", "TRUCK" }), 
                                @COMMENT = CutToMaxLength(Faker.Lorem.Sentence(5), 44)
                            },
                            transaction: tran
                        );
                    }

                    tran.Commit();                    
                }
            }
        }

        private void UpdateCustomer()
        {
            var cid = Faker.Random.Int(StartingCustomerId, CustomerId);

            Log($"Updating Customer {cid}...");

            using (var conn = new SqlConnection(SQLConnectionString))
            {
                var affectedRows = conn.Execute("UPDATE dbo.CUSTOMER SET [C_NAME] = @NAME, [C_ADDRESS] = @ADDRESS, [C_NATIONKEY] = @NATIONKEY, [C_PHONE] = @PHONE, [C_ACCTBAL] = @ACCTBAL, [C_MKTSEGMENT] = @MKTSEGMENT, [C_COMMENT] = @COMMENT WHERE C_CUSTKEY = @CUSTKEY",
                new
                {
                    @CUSTKEY = cid,
                    @NAME = CutToMaxLength(Faker.Company.CompanyName(), 25),
                    @ADDRESS = CutToMaxLength(Faker.Address.FullAddress(), 40),
                    @NATIONKEY = Faker.Random.Int(0, 24),
                    @PHONE = Faker.Random.Int(0, 24),
                    @ACCTBAL = Faker.Random.Decimal(100, 10000),
                    @MKTSEGMENT = Faker.Random.ArrayElement(new string[] { "AUTOMOBILE", "BUILDING", "FURNITURE", "HOUSEHOLD", "MACHINERY" }),
                    @COMMENT = CutToMaxLength(Faker.Lorem.Sentence(5), 100)
                });
            }
        }

        private void UpdateOrder()
        {
            var oid = Faker.Random.Int(StartingOrderId, OrderId);

            using (var conn = new SqlConnection(SQLConnectionString))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {                    
                    Log($"Updating Order {oid}...");

                    var lineCount = conn.ExecuteScalar<int>("SELECT MAX(L_LINENUMBER) FROM dbo.LINEITEM WHERE L_ORDERKEY = @ORDERKEY",  new { @ORDERKEY = oid }, tran);

                    conn.Execute(@"
                        UPDATE dbo.ORDERS 
                        SET [O_ORDERSTATUS] = @ORDERSTATUS, [O_TOTALPRICE] = @TOTALPRICE, [O_ORDERDATE] = @ORDERDATE, [O_ORDERPRIORITY] = @ORDERPRIORITY, [O_CLERK] = @CLERK, [O_SHIPPRIORITY] = @SHIPPRIORITY, [O_COMMENT] = @COMMENT
                        WHERE [O_ORDERKEY] = @ORDERKEY",
                        new
                        {
                            @ORDERKEY = oid,
                            @ORDERSTATUS = Faker.Random.ArrayElement(new string[] { "F", "O", "P" }),
                            @TOTALPRICE = Faker.Random.Decimal(100, 10000),
                            @ORDERDATE = Faker.Date.RecentOffset(15),
                            @ORDERPRIORITY = Faker.Random.ArrayElement(new string[] { "1-URGENT", "2-HIGH", "3-MEDIUM", "4-NOT SPECIFIED", "5-LOW" }),
                            @CLERK = String.Format("Clerk#{0:00000000}", Faker.Random.Number(1, 9999)),
                            @SHIPPRIORITY = 0,
                            @COMMENT = CutToMaxLength(Faker.Lorem.Sentence(5), 100)
                        },
                        tran
                    );

                    Log($"Updating Order {oid}: Updating LineItems");    
                    var lineItemsUpdate = Faker.Random.ArrayElements(Enumerable.Range(1, lineCount).ToArray(), Faker.Random.Int(0, lineCount));
                    foreach(var lin in lineItemsUpdate)
                    {
                        conn.Execute(@"
                            UPDATE dbo.LINEITEM 
                            SET [L_PARTKEY] = @PARTKEY, [L_SUPPKEY] = @SUPPKEY, [L_QUANTITY] = @QUANTITY, [L_EXTENDEDPRICE] = @EXTENDEDPRICE, [L_DISCOUNT] = @DISCOUNT, [L_TAX] = @TAX, [L_RETURNFLAG] = @RETURNFLAG, [L_LINESTATUS] = @LINESTATUS, [L_SHIPDATE] = @SHIPDATE, [L_COMMITDATE] = @COMMITDATE, [L_RECEIPTDATE] = @RECEIPTDATE, [L_SHIPINSTRUCT] = @SHIPINSTRUCT, [L_SHIPMODE] = @SHIPMODE, [L_COMMENT] = @COMMENT
                            WHERE [L_ORDERKEY] = @ORDERKEY AND [L_LINENUMBER] = @LINENUMBER",
                            new
                            {
                                @ORDERKEY = oid,
                                @LINENUMBER = lin,
                                @PARTKEY = Faker.Random.Int(1, PartId),
                                @SUPPKEY = Faker.Random.Int(1, SupplierId),                                
                                @QUANTITY = Faker.Random.Int(1, 50),
                                @EXTENDEDPRICE = Faker.Random.Decimal(900, 150000),
                                @DISCOUNT = Faker.Random.Decimal(0, 0.10m),
                                @TAX = Faker.Random.Decimal(0, 0.08m),
                                @RETURNFLAG = Faker.Random.ArrayElement(new string[] { "A", "N", "R" }),
                                @LINESTATUS = Faker.Random.ArrayElement(new string[] { "F", "O" }), 
                                @SHIPDATE = Faker.Date.RecentOffset(15), 
                                @COMMITDATE = Faker.Date.RecentOffset(15), 
                                @RECEIPTDATE = Faker.Date.RecentOffset(15), 
                                @SHIPINSTRUCT = Faker.Random.ArrayElement(new string[] { "COLLECT COD", "DELIVER IN PERSON", "TAKE BACK RETURN" }), 
                                @SHIPMODE = Faker.Random.ArrayElement(new string[] { "AIR", "FOB", "MAIL", "RAIL", "REG AIR", "SHIP", "TRUCK" }), 
                                @COMMENT = CutToMaxLength(Faker.Lorem.Sentence(5), 44)
                            },
                            transaction: tran
                        );
                    }

                    Log($"Updating Order {oid}: Deleting LineItems");                        
                    var lineItemsDelete = Faker.Random.ArrayElements(Enumerable.Range(1, lineCount).ToArray(), Faker.Random.Int(0, lineCount));
                    conn.Execute(@"
                        DELETE dbo.LINEITEM                             
                        WHERE [L_ORDERKEY] = @ORDERKEY AND [L_LINENUMBER] IN @LINENUMBER",
                            new
                            {
                                @ORDERKEY = oid,
                                @LINENUMBER = lineItemsDelete
                            },
                            transaction: tran
                        );                    

                    Log($"Updating Order {oid}: Adding LineItems");                        
                    var num = lineCount + (int)(Math.Round(Random.NextDouble() * 20, 0));                
                    for(int lin=0; lin<num; lin++)
                    {
                        conn.Execute(
                            @"INSERT INTO dbo.LINEITEM ([L_ORDERKEY], [L_PARTKEY], [L_SUPPKEY], [L_LINENUMBER], [L_QUANTITY], [L_EXTENDEDPRICE], [L_DISCOUNT], [L_TAX], [L_RETURNFLAG], [L_LINESTATUS], [L_SHIPDATE], [L_COMMITDATE], [L_RECEIPTDATE], [L_SHIPINSTRUCT], [L_SHIPMODE], [L_COMMENT])
                            VALUES (@ORDERKEY, @PARTKEY, @SUPPKEY, @LINENUMBER, @QUANTITY, @EXTENDEDPRICE, @DISCOUNT, @TAX, @RETURNFLAG, @LINESTATUS, @SHIPDATE, @COMMITDATE, @RECEIPTDATE, @SHIPINSTRUCT, @SHIPMODE, @COMMENT)",
                            new
                            {
                                @ORDERKEY = oid,
                                @PARTKEY = Faker.Random.Int(1, PartId),
                                @SUPPKEY = Faker.Random.Int(1, SupplierId),
                                @LINENUMBER = lin + 1 + 20,
                                @QUANTITY = Faker.Random.Int(1, 50),
                                @EXTENDEDPRICE = Faker.Random.Decimal(900, 150000),
                                @DISCOUNT = Faker.Random.Decimal(0, 0.10m),
                                @TAX = Faker.Random.Decimal(0, 0.08m),
                                @RETURNFLAG = Faker.Random.ArrayElement(new string[] { "A", "N", "R" }),
                                @LINESTATUS = Faker.Random.ArrayElement(new string[] { "F", "O" }), 
                                @SHIPDATE = Faker.Date.RecentOffset(15), 
                                @COMMITDATE = Faker.Date.RecentOffset(15), 
                                @RECEIPTDATE = Faker.Date.RecentOffset(15), 
                                @SHIPINSTRUCT = Faker.Random.ArrayElement(new string[] { "COLLECT COD", "DELIVER IN PERSON", "TAKE BACK RETURN" }), 
                                @SHIPMODE = Faker.Random.ArrayElement(new string[] { "AIR", "FOB", "MAIL", "RAIL", "REG AIR", "SHIP", "TRUCK" }), 
                                @COMMENT = CutToMaxLength(Faker.Lorem.Sentence(5), 44)
                            },
                            transaction: tran
                        );
                    }

                    tran.Commit();                    
                }
            }
        }

        private void DeleteCustomer()
        {
            var cid = Faker.Random.Int(StartingCustomerId, CustomerId);

            Log($"Deleting Customer {cid}...");

            using (var conn = new SqlConnection(SQLConnectionString))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {                
                    conn.Execute("DELETE FROM li FROM dbo.LINEITEM AS li INNER JOIN dbo.ORDERS AS o ON o.O_ORDERKEY = li.L_ORDERKEY WHERE o.O_CUSTKEY = @CUSTKEY", new { @CUSTKEY = cid }, tran);
                    conn.Execute("DELETE dbo.ORDERS WHERE O_CUSTKEY = @CUSTKEY", new { @CUSTKEY = cid }, tran);
                    conn.Execute("DELETE dbo.CUSTOMER WHERE C_CUSTKEY = @CUSTKEY", new { @CUSTKEY = cid }, tran);

                    tran.Commit();
                }
            }
        }

        private void DeleteOrder()
        {
            var oid = Faker.Random.Int(StartingOrderId, OrderId);

            Log($"Deleting Order {oid}...");

            using (var conn = new SqlConnection(SQLConnectionString))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {                
                    conn.Execute("DELETE dbo.LINEITEM WHERE L_ORDERKEY = @ORDERKEY", new { @ORDERKEY = oid }, tran);
                    conn.Execute("DELETE dbo.ORDERS WHERE O_ORDERKEY = @ORDERKEY", new { @ORDERKEY = oid }, tran);

                    tran.Commit();
                }
            }
        }

        private string CutToMaxLength(string text, int length)
        {
            if (text.Length > length)
                return text.Substring(0, length);
            else
                return text;
        }

        private void Log(string text)
        {
            Console.WriteLine($"{DateTime.Now:o}|{Task.CurrentId:000}|{text}");
        }
    }
}
