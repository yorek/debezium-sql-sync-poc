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
    class PartitionReceiver : IPartitionReceiveHandler
    {
        int _maxBatchSize = 1;
        public int MaxBatchSize { get => _maxBatchSize; set => _maxBatchSize = value; }

        public PartitionReceiver(string partitionId)
        {
            Console.WriteLine($"Partition receiver {partitionId} ready and listening...");
        }

        public Task ProcessErrorAsync(Exception error)
        {
            Console.WriteLine(error.Message);
            Console.ReadLine();
            return Task.FromException(error);
        }

        public async Task ProcessEventsAsync(IEnumerable<EventData> events)
        {
            if (events == null) 
            {
                Console.WriteLine("No more events to process.");
                return;
            }

            foreach (var e in events)
            {
                string body = Encoding.UTF8.GetString(e.Body);
                if (string.IsNullOrEmpty(body)) continue;

                Console.WriteLine($"Offset: {e.SystemProperties.Offset}");

                var json = JObject.Parse(body);
                
                if (json?["payload"] == null) {
                    Console.WriteLine("Unknown json format, skipping.");
                    continue;
                }

                var operation = json?["payload"]?["op"]?.ToString();

                if (operation == null) {
                    Console.WriteLine("Unknown operation, skipping.");
                    continue;
                }

                var processor = JsonProcessorFactory.CreatePayloadProcessor(operation, json);
                processor.Process();

                Utils.SaveSetting("LastOffset", e.SystemProperties.Offset);                

                await Task.Yield();
            }
        }
    }
}