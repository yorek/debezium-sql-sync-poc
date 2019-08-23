using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Newtonsoft.Json.Linq;
using System.Configuration;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;

namespace DebeziumInitializer
{
    class DebeziumInfo {
        public string ConnectorName;
        public string DatabaseServerName;
    }

    class Program
    {
        private static string EventHubConnectionString = ConfigurationManager.AppSettings["EventHubConnectionString"];        
        private static string DebeziumAddress = ConfigurationManager.AppSettings["DebeziumAddress"];
        private static string DebeziumConnectorName = ConfigurationManager.AppSettings["DebeziumConnectorName"];

        static async Task Main(string[] args)
        {
            Console.WriteLine("Initializing Debezium Snapshot...");

            Console.WriteLine("Getting connector info...");           
            var keyInfo = await GetDebeziumInfo();

            var topic = "debezium_offsets";

            var firstSemiColon = EventHubConnectionString.IndexOf(';');
            var endpointEnd = EventHubConnectionString.Substring(0, firstSemiColon-1);
            var broker = endpointEnd.Substring(14, firstSemiColon-1-14);

            var config = new Dictionary<string, object> {
                    { "bootstrap.servers", $"{broker}:9093" },
                    { "security.protocol", "SASL_SSL" },
                    { "sasl.mechanism", "PLAIN" },
                    { "sasl.username", "$ConnectionString" },
                    { "sasl.password", EventHubConnectionString },
                    { "ssl.ca.location", "./cacert.pem" },
                    //{ "debug", "security,broker,protocol" }       //Uncomment for librdkafka debugging information
                };

            Console.WriteLine("Sending offset...");

            // create correct key for debezium
            //string key = "[\"tpch\",{\"server\":\"laptop\"}]";            
            string key = $"[\"{keyInfo.ConnectorName}\",{{\"server\":\"{keyInfo.DatabaseServerName}\"}}]";            

            // set offset to a fake initial value
            var value = "{\"event_serial_no\":1,\"commit_lsn\":\"00000001:00000001:0001\",\"change_lsn\":\"00000001:00000001:0001\"}";  

            // send message
            var producer = new Producer<string, string>(config, new StringSerializer(Encoding.ASCII), new StringSerializer(Encoding.ASCII));            
            await producer.ProduceAsync(topic, key, value);
            
            
            Console.WriteLine("Done.");
        }

        private static async Task<DebeziumInfo> GetDebeziumInfo(){
            var client = new HttpClient();            
            
            var url = $"{DebeziumAddress}/connectors/{DebeziumConnectorName}";
            var response = await client.GetAsync(url);
            
            string content = await response.Content.ReadAsStringAsync();
            
            var json = JObject.Parse(content);

            var name = json["name"].ToString();
            var server = json["config"]["database.server.name"].ToString();

            return new DebeziumInfo() { ConnectorName = name, DatabaseServerName = server };
        }
    }
}
