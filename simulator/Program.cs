using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using Bogus;
using System.Linq;

namespace simulator
{
    class Customer
    {
        public int Id;
        public string CompanyName;
        public string Address;
    }

    class Program
    {
        private static readonly string SQLConnectionString = "Server=localhost;Initial Catalog=TPCH;Integrated Security=SSPI;";

        private static Faker Faker;

        static void Main(string[] args)
        {
            int taskCount = 1;

            Randomizer.Seed = new Random();            

            var tasks = new List<Task>();

            Console.WriteLine($"Creating {taskCount} simulator instances");
            foreach(var n in Enumerable.Range(1, taskCount))
            {   
                tasks.Add(Task.Run(() => SimulateActivity()));
            }
            Console.WriteLine($"Done. Ctrl+C to terminate.");

            Task.WaitAll(tasks.ToArray());            
        }

        static void SimulateActivity()
        {            
            var random = new Random();            

            while (true) {
                var num = (int)(Math.Round(random.NextDouble() * 1000, 0));

                CreateCustomer();

                Task.Delay(5000);
            }
        }

        static void CreateCustomer()
        {
            //Console.WriteLine("Creating new customer");
            using (var conn = new SqlConnection(SQLConnectionString)) {

                var faker = new Faker();              
                
                conn.Execute(
                     "INSERT INTO dbo.CUSTOMER ([C_CUSTKEY], [C_NAME], [C_ADDRESS], [C_NATIONKEY], [C_PHONE], [C_ACCTBAL], [C_MKTSEGMENT], [C_COMMENT]) VALUES (@CUSTKEY, @NAME, @ADDRESS, @NATIONKEY, @PHONE, @ACCTBAL, @MKTSEGMENT, @COMMENT)",
                     new {
                        @CUSTKEY = 9999999, 
                        @NAME = CutToMaxLength(faker.Company.CompanyName(), 25),
                        @ADDRESS = CutToMaxLength(faker.Address.FullAddress(), 40),
                        @NATIONKEY = faker.Random.Int(0, 24),
                        @PHONE = faker.Random.Int(0, 24),
                        @ACCTBAL = faker.Random.Decimal(100, 10000),
                        @MKTSEGMENT = faker.Random.ArrayElement(new string[] {"AUTOMOBILE", "BUILDING", "FURNITURE", "HOUSEHOLD", "MACHINERY"}),
                        @COMMENT = CutToMaxLength(faker.Lorem.Sentence(5), 100)
                     });

            }
        }

        static void CreateOrder() {
            using (var conn = new SqlConnection(SQLConnectionString)) {
                conn.Execute("");
            }
        }

        static void UpdateCustomer() {
            using (var conn = new SqlConnection(SQLConnectionString)) {
                conn.Execute("");
            }
        }

        static void UpdateOrder() {
            using (var conn = new SqlConnection(SQLConnectionString)) {
                conn.Execute("");
            }
        }

        static void UpdateLineItem() {
            using (var conn = new SqlConnection(SQLConnectionString)) {
                conn.Execute("");
            }
        }

        static string CutToMaxLength(string text, int length)
        {            
            if (text.Length > length) 
                return text.Substring(0, length); 
            else 
                return text;
        }
    }
}
