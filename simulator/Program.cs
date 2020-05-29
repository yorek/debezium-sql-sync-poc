using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DotNetEnv;

namespace Simulator
{
    class Program
    {
        static void Main(string[] args)
        {
            DotNetEnv.Env.Load();

            Console.WriteLine($"Creating simulator...");
            var simulator = new TPCHSimulator();
            Console.WriteLine($"Simulator created.");

            int taskCount = int.Parse(Environment.GetEnvironmentVariable("SimulatorCount") ?? "1");
            Console.WriteLine($"Starting {taskCount} simulator instances...");
            Console.WriteLine($"Press 'c' to terminate simulators.");
            Console.WriteLine($"Simulation will start in 5 seconds...");
            Thread.Sleep(5000);
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

            Console.WriteLine("Finished.");
        }
    }
}