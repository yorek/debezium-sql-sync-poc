using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using Bogus;
using System.Linq;

namespace Simulator
{
    class TPCHSimulator
    {
        private readonly string SQLConnectionString = "Server=localhost;Initial Catalog=TPCH;Integrated Security=SSPI;";

        private readonly Random Random = new Random();

        private readonly Faker Faker = new Faker();

        private volatile int CustomerId = 2000000;

        private volatile int OrderId = 70000000;

        public TPCHSimulator()
        {
            Randomizer.Seed = new Random();
        }

        public void SimulateActivity()
        {
            while (true)
            {
                var num = (int)(Math.Round(Random.NextDouble() * 1000, 0));

                if (num <= 500) CreateCustomer();
                if (num > 500 && num <= 600) UpdateCustomer();
                if (num > 600 && num <= 900) CreateOrder();

                Thread.Sleep(5000);
            }
        }

        private void CreateCustomer()
        {
            using (var conn = new SqlConnection(SQLConnectionString))
            {

                conn.Execute(
                     "INSERT INTO dbo.CUSTOMER ([C_CUSTKEY], [C_NAME], [C_ADDRESS], [C_NATIONKEY], [C_PHONE], [C_ACCTBAL], [C_MKTSEGMENT], [C_COMMENT]) VALUES (@CUSTKEY, @NAME, @ADDRESS, @NATIONKEY, @PHONE, @ACCTBAL, @MKTSEGMENT, @COMMENT)",
                     new
                     {
                         @CUSTKEY = CustomerId,
                         @NAME = CutToMaxLength(Faker.Company.CompanyName(), 25),
                         @ADDRESS = CutToMaxLength(Faker.Address.FullAddress(), 40),
                         @NATIONKEY = Faker.Random.Int(0, 24),
                         @PHONE = Faker.Random.Int(0, 24),
                         @ACCTBAL = Faker.Random.Decimal(100, 10000),
                         @MKTSEGMENT = Faker.Random.ArrayElement(new string[] { "AUTOMOBILE", "BUILDING", "FURNITURE", "HOUSEHOLD", "MACHINERY" }),
                         @COMMENT = CutToMaxLength(Faker.Lorem.Sentence(5), 100)
                     });

                CustomerId += 1;
            }
        }

        private void CreateOrder()
        {
            var cid = Faker.Random.Int(1, CustomerId);

            using (var conn = new SqlConnection(SQLConnectionString))
            {

                conn.Execute(@"INSERT INTO dbo.ORDERS ([O_ORDERKEY], [O_CUSTKEY], [O_ORDERSTATUS], [O_TOTALPRICE], [O_ORDERDATE], [O_ORDERPRIORITY], [O_CLERK], [O_SHIPPRIORITY], [O_COMMENT])
                        VALUES (@ORDERKEY, @CUSTKEY, @ORDERSTATUS, @TOTALPRICE, @ORDERDATE, @ORDERPRIORITY, @CLERK, @SHIPPRIORITY, @COMMENT)",
                     new
                     {
                         @ORDERKEY = OrderId,
                         @CUSTKEY = cid,
                         @ORDERSTATUS = Faker.Random.ArrayElement(new string[] { "F", "O", "P" }),
                         @TOTALPRICE = Faker.Random.Decimal(100, 10000),
                         @ORDERDATE = Faker.Date.RecentOffset(15),
                         @ORDERPRIORITY = Faker.Random.ArrayElement(new string[] { "1-URGENT", "2-HIGH", "3-MEDIUM", "4-NOT SPECIFIED", "5-LOW" }),
                         @CLERK = String.Format("Clerk#{0:00000000}", Faker.Random.Number(1, 9999)),
                         @SHIPPRIORITY = 0,
                         @COMMENT = CutToMaxLength(Faker.Lorem.Sentence(5), 100)
                     });

                OrderId += 1;
            }
        }

        private void UpdateCustomer()
        {
            var cid = Faker.Random.Int(1, CustomerId);

            using (var conn = new SqlConnection(SQLConnectionString))
            {
                conn.Execute("UPDATE dbo.CUSTOMER SET [C_NAME] = @NAME, [C_ADDRESS] = @ADDRESS, [C_NATIONKEY] = @NATIONKEY, [C_PHONE] = @PHONE, [C_ACCTBAL] = @ACCTBAL, [C_MKTSEGMENT] = @MKTSEGMENT, [C_COMMENT] = @COMMENT WHERE C_CUSTKEY = @CUSTKEY",
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
            using (var conn = new SqlConnection(SQLConnectionString))
            {
                conn.Execute("");
            }
        }

        private void UpdateLineItem()
        {
            using (var conn = new SqlConnection(SQLConnectionString))
            {
                conn.Execute("");
            }
        }

        private string CutToMaxLength(string text, int length)
        {
            if (text.Length > length)
                return text.Substring(0, length);
            else
                return text;
        }
    }
}
