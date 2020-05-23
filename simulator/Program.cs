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
        static void Main(string[] args)
        {
            Console.WriteLine($"Creating simulator...");
            var simulator = new TPCHSimulator();
            Console.WriteLine($"Done.");

            int taskCount = 5;
            Console.WriteLine($"Creating {taskCount} simulator instances");
            var tasks = new List<Task>();
            foreach(var n in Enumerable.Range(1, taskCount))
            {   
                tasks.Add(Task.Run(() => simulator.SimulateActivity()));
            }

            Console.WriteLine($"Done. Ctrl+C to terminate.");

            Task.WaitAll(tasks.ToArray());            
        }
    }
}