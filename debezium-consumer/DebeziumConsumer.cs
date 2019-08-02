using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using Dapper;

namespace Debezium.Consumer
{
    interface IPayloadProcessor
    {
        void Process();
    }

    class PayloadSource
    {
        public string Schema { get; set; }
        public string Table { get; set; }
        public long CommitLSN { get; set; }
    }

    abstract class PayloadProcessorBase: IPayloadProcessor
    {
        private JObject Json;
        public JObject After => (JObject)(Json["payload"]["after"]);
        public JObject Before => (JObject)(Json["payload"]["before"]);
        public PayloadSource SourceMetadata;
        private Dictionary<string, string> _schema = new Dictionary<string, string>();

        private long _baseLSN = 0;
        private string _sqlConnectionString = ConfigurationManager.AppSettings["AzureSQLConnectionString"];

        public PayloadProcessorBase(JObject json)
        {
            this.Json = json;
            LoadSchema();
            LoadSourceMetadata();

            string baseLSNHex = ConfigurationManager.AppSettings["BaseLSN"];
            _baseLSN = Convert.ToInt64(baseLSNHex.Replace("0x", ""), 16);
        }

        public void LoadSourceMetadata()
        {
            var payload = this.Json["payload"];

            var result = new PayloadSource()
            {
                Schema = payload["source"]["schema"].ToString(),
                Table = payload["source"]["table"].ToString(),
                CommitLSN = Convert.ToInt64(payload["source"]["commit_lsn"].ToString().Replace(":", ""), 16)
            };

            this.SourceMetadata = result;
        }

        public virtual void Process()
        {            
        }

        protected void LoadSchema()
        {
            JArray schema = (JArray)(this.Json["schema"]["fields"][0]["fields"]);

            foreach (var s in schema)
            {
                var so = (JObject)(s);
                string field = so["field"].ToString();
                string type = so["type"].ToString();
                string debeziumType = so["name"]?.ToString();
                _schema.Add(field, debeziumType ?? "");
            }
        }

        protected string GetSQLValue(JProperty property)
        {
            string result = property.Value.ToString();
            string debeziumType = _schema[property.Name];

            if (string.IsNullOrEmpty(_schema[property.Name])) // not a debezium data type
            {
                if (property.Value.Type == JTokenType.String)
                {
                    result = "'" + result + "'";
                }
            }
            else
            {
                switch (_schema[property.Name])
                {
                    case "io.debezium.time.Date":
                        var daysFromEoch = Int32.Parse(result);
                        DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                        DateTime date = epoch.AddDays(daysFromEoch);
                        result = "'" + date.ToString("yyyy-MM-dd") + "'";
                        break;

                    default:
                        throw new ApplicationException($"'{debeziumType}' is unknown");
                }
            }

            return result;
        }

        protected void WriteToAzureSQL(string command)
        {                        
            if (SourceMetadata.CommitLSN > _baseLSN)
            {
                Console.WriteLine($"Transaction Commit LSN {SourceMetadata.CommitLSN:X} is greater than base LSN {_baseLSN:X}. Applying...");
                Console.WriteLine(command);
                using (var conn = new SqlConnection(_sqlConnectionString))
                {
                    //int rows = conn.Execute(command);
                    //Console.WriteLine($"Affected Rows: {rows}");
                }
            } else {
                Console.WriteLine($"Transaction Commit LSN {SourceMetadata.CommitLSN:X} is greater than base LSN {_baseLSN:X}. Skipping.");
            }
        }
    }

    class UpdatePayloadProcessor : PayloadProcessorBase
    {
        public UpdatePayloadProcessor(JObject json): base(json) {}

        public override void Process()
        {
            var source = this.SourceMetadata;            

            var sb = new StringBuilder();
            sb.Append($"UPDATE [{source.Schema}].[{source.Table}] SET ");

            var after = this.After;
            var fieldsSet = new List<string>();
            foreach (var e in after.Properties())
            {
                string name = e.Name;
                string value = e.Value.ToString();
                if (e.Value.Type == JTokenType.String)
                {
                    value = "'" + value + "'";
                }

                fieldsSet.Add($"[{name}] = {value}");
            }
            sb.Append(string.Join(", ", fieldsSet));

            sb.Append(" WHERE ");

            var before = this.Before;
            var fieldsDelete = new List<string>();
            foreach (var e in before.Properties())
            {
                string name = e.Name;
                string value = e.Value.ToString();
                if (e.Value.Type == JTokenType.String)
                {
                    value = "'" + value + "'";
                }

                fieldsDelete.Add($"[{name}] = {value}");
            }
            sb.Append(string.Join(" AND ", fieldsDelete));

            string sqlCommand = sb.ToString();

            WriteToAzureSQL(sqlCommand);
        }
    }

    class CreatePayloadProcessor : PayloadProcessorBase
    {
        public CreatePayloadProcessor(JObject json): base(json) {}

        public override void Process()
        {
            var source = this.SourceMetadata;       
            var after = this.After;

            var sb = new StringBuilder();
            sb.Append($"INSERT INTO [{source.Schema}].[{source.Table}] ");
            sb.Append("(");

            var fields = new List<string>();
            foreach (var e in after.Properties())
            {
                fields.Add($"[{e.Name}]");
            }
            sb.Append(string.Join(", ", fields));

            sb.Append(") VALUES (");

            var values = new List<string>();
            foreach (var e in after.Properties())
            {
                var value = GetSQLValue(e);
                values.Add(value);
            }
            sb.Append(string.Join(", ", values));

            sb.Append(")");

            string sqlCommand = sb.ToString();

            WriteToAzureSQL(sqlCommand);
        }
    }

    class DeletePayloadProcessor : PayloadProcessorBase    
    {
        public DeletePayloadProcessor(JObject json): base(json) {}

        public override void Process()
        {
            var source = this.SourceMetadata;       
            var before = this.Before;

            var sb = new StringBuilder();
            sb.Append($"DELETE FROM [{source.Schema}].[{source.Table}] WHERE ");
            
            var fields = new List<string>();
            foreach (var e in before.Properties())
            {
                string name = e.Name;
                string value = GetSQLValue(e);

                fields.Add($"[{name}] = {value}");
            }
            sb.Append(string.Join(" AND ", fields));

            string sqlCommand = sb.ToString();

            WriteToAzureSQL(sqlCommand);
        }
    }

    class PayloadProcessorFactory
    {
        public static PayloadProcessorBase CreatePayloadProcessor(string operation, JObject json)
        {
            switch (operation)
            {
                case "c": return new CreatePayloadProcessor(json);
                case "u": return new UpdatePayloadProcessor(json);
                case "d": return new DeletePayloadProcessor(json);
                default: throw new ArgumentException($"Unknown '{operation}' value for operation");
            }
        }
    }

    class PartitionReceiver : IPartitionReceiveHandler
    {
        int _maxBatchSize = 1;
        public int MaxBatchSize { get => _maxBatchSize; set => _maxBatchSize = value; }

        public PartitionReceiver()
        {
        }

        public Task ProcessErrorAsync(Exception error)
        {
            Console.WriteLine(error.Message);
            Console.ReadLine();
            return Task.FromException(error);
        }

        public async Task ProcessEventsAsync(IEnumerable<EventData> events)
        {
            foreach (var e in events)
            {
                string body = Encoding.UTF8.GetString(e.Body);
                if (string.IsNullOrEmpty(body)) continue;

                Console.WriteLine($"Offset: {e.SystemProperties.Offset}");

                var json = JObject.Parse(body);
                var operation = json["payload"]["op"].ToString();

                var processor = PayloadProcessorFactory.CreatePayloadProcessor(operation, json);
                processor.Process();

                await Task.Yield();
            }
        }
    }

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
            Console.WriteLine("The application will now start to listen for incoming message.");

            var runtimeInfo = await client.GetRuntimeInformationAsync();
            Console.WriteLine("Creating receiver handlers...");
            var utcNow = DateTime.UtcNow;
            //var startPosition = EventPosition.FromEnqueuedTime(utcNow);
            var startPosition = EventPosition.FromStart();
            var receivers = runtimeInfo.PartitionIds
                .Select(pid =>
                {
                    var receiver = client.CreateReceiver("$Default", pid, startPosition);
                    Console.WriteLine("Created receiver for partition '{0}'.", pid);
                    receiver.SetReceiveHandler(new PartitionReceiver());
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
                // Save Offsets (TODO)            
                // receivers[0].RuntimeInfo.LastEnqueuedOffset

                // Clean up nicely.
                await Task.WhenAll(
                    receivers.Select(receiver => receiver.CloseAsync())
                );
            }
        }
    }
}