using System;
using System.Threading.Tasks;
using System.Threading;
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

            int taskCount = 15;
            Console.WriteLine($"Creating {taskCount} simulator instances");
            var tasks = new List<Task>();
            var cts = new CancellationTokenSource();
            var ct = cts.Token;            
            foreach(var n in Enumerable.Range(1, taskCount))
            {   
                tasks.Add(Task.Run(() => simulator.SimulateActivity(ct)));
            }

            while(true)
            {
                var c = Console.ReadKey(true);
                if (c.KeyChar == 'c') { 
                    Console.WriteLine("Stopping simulator tasks...");
                    cts.Cancel();
                    break;
                }
            } 

            try 
            {
                Task.WaitAll(tasks.ToArray());            
            } 
            catch (Exception e)
            {
                var le = e;                
                while(le != null)
                {
                    Console.WriteLine(le.Message);
                    le = le.InnerException;
                }
            }
            finally
            {
                cts.Dispose();
            }            
        }
    }
}