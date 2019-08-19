using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using Bogus;
using System.Linq;

namespace Simulator
{
    class Program
    {
        private static readonly string SQLConnectionString = "Server=localhost;Initial Catalog=TPCH;Integrated Security=SSPI;";

        private static Faker Faker;

        static void Main(string[] args)
        {
            int taskCount = 1;

            var tasks = new List<Task>();
            var simulator = new TPCHSimulator();

            Console.WriteLine($"Creating {taskCount} simulator instances");
            foreach(var n in Enumerable.Range(1, taskCount))
            {   
                tasks.Add(Task.Run(() => simulator.SimulateActivity()));
            }
            Console.WriteLine($"Done. Ctrl+C to terminate.");

            Task.WaitAll(tasks.ToArray());            
        }
    }
}