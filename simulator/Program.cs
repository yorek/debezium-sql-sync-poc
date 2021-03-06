﻿using System;
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
            int taskCount = 1;

            var tasks = new List<Task>();

            Console.WriteLine($"Creating {taskCount} simulator instances");
            foreach(var n in Enumerable.Range(1, taskCount))
            {   
                var simulator = new TPCHSimulator(n, "Server=localhost;Initial Catalog=TPCH1GB;Integrated Security=SSPI;");
                tasks.Add(Task.Run(() => simulator.SimulateActivity()));
            }
            Console.WriteLine($"Done. Ctrl+C to terminate.");

            Task.WaitAll(tasks.ToArray());            
        }
    }
}