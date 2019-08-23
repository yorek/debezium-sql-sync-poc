using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Newtonsoft.Json.Linq;
using System.Configuration;

namespace DebeziumInitializer
{
    class Program
    {
        private static string EventHubConnectionString = ConfigurationManager.AppSettings?["EventHubConnectionString"];        
        static async Task Main(string[] args)
        {
            Console.WriteLine("Initializing Debezium Snapshot...");

            if (args.Length > 0) 
            {
                EventHubConnectionString = args[0];
            }

            var connectionStringBuilder = new EventHubsConnectionStringBuilder(EventHubConnectionString);            
            if (string.IsNullOrEmpty(connectionStringBuilder.EntityPath))
                connectionStringBuilder.EntityPath = "debezium_offsets";

            var eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());

            var json = "{\"event_serial_no\":1,\"commit_lsn\":\"0000007b:000074f0:0103\",\"change_lsn\":\"0000007b:000074f0:0102\"}";

            Console.WriteLine("Sending offset...");

            var eventData = new EventData(Encoding.ASCII.GetBytes(json));    
            await eventHubClient.SendAsync(eventData, "[\"tpch\",{\"server\":\"laptop\"}]");
                        
            await eventHubClient.SendAsync(new EventData(Encoding.ASCII.GetBytes(json)));

            //await eventHubClient.SendAsync(eventData, "\n");

            var sender = eventHubClient.CreatePartitionSender("4");            
            await sender.SendAsync(eventData);            

            // Console.WriteLine("Receiving...");
            // var receiver = eventHubClient.CreateReceiver("$Default", "4", EventPosition.FromStart());

            // var messages = await receiver.ReceiveAsync(10);
            
            // foreach(var m in messages) {
            //     Console.WriteLine(m.SystemProperties.SequenceNumber);
            //     Console.WriteLine(Encoding.UTF8.GetString(m.Body));
            // }

            Console.WriteLine("Closing...");
            await eventHubClient.CloseAsync();

            Console.WriteLine("Done.");
        }
    }
}
