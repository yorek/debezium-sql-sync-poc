using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;

namespace Debezium.Consumer
{    
    class DebeziumConsumer
    {
        private EventHubClient client;
        private SqlConnection sqlconn;

        public void Configure()
        {
            Console.WriteLine("Validating settings...");
            foreach (string option in new string[] { "EventHubConnectionString", "AzureSQLConnectionString", "BaseLSN" })
            {
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings?[option]))
                {
                    Console.WriteLine($"Missing '{option}' in App.config.");
                    return;
                }
            }

            Console.WriteLine("Connecting to EventHub...");
            string eventhubConnectionString = ConfigurationManager.AppSettings["EventHubConnectionString"];
            client = EventHubClient.CreateFromConnectionString(eventhubConnectionString);

            Console.WriteLine("Connecting to Azure SQL...");
            string sqlConnectionString = ConfigurationManager.AppSettings["AzureSQLConnectionString"];
            sqlconn = new SqlConnection(sqlConnectionString);
            Console.WriteLine("Testing Azure SQL connection...");
            sqlconn.Open();
            sqlconn.Close();
        }

        public async Task ProcessMessages(CancellationToken cancellationToken)
        {
            Console.WriteLine("The application will now start to listen for incoming messages.");

            var startPosition = EventPosition.FromStart();
            if (ConfigurationManager.AppSettings["LastOffset"] != null)
            {
                string lastOffset = ConfigurationManager.AppSettings["LastOffset"];
                startPosition = EventPosition.FromOffset(lastOffset, inclusive: false);
                Console.WriteLine($"Starting from Offset: {lastOffset}");
            }

            var runtimeInfo = await client.GetRuntimeInformationAsync();            
            
            Console.WriteLine("Creating receiver handlers...");
            var utcNow = DateTime.UtcNow;
            //var startPosition = EventPosition.FromEnqueuedTime(utcNow);
            
            var receivers = runtimeInfo.PartitionIds
                .Select(pid =>
                {
                    var receiver = client.CreateReceiver("$Default", pid, startPosition);
                    Console.WriteLine("Created receiver for partition '{0}'.", pid);
                    receiver.SetReceiveHandler(new PartitionReceiver(pid), invokeWhenNoEvents: true);
                    return receiver;
                })
                .ToList();

            try
            {
                await Task.Delay(-1, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                // This is okay because the task was cancelled. :)
            }
            finally
            {
                // Clean up nicely.
                await Task.WhenAll(
                    receivers.Select(receiver => receiver.CloseAsync())
                );
            }
        }
    }
}