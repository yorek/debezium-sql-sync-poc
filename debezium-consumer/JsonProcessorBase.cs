using System;
using System.Collections.Generic;
using System.Configuration;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;

namespace Debezium.Consumer
{
    interface IJsonProcessor
    {
        void Process();
    }

    class PayloadSource
    {
        public string Schema { get; set; }
        public string Table { get; set; }
        public long CommitLSN { get; set; }
    }

    abstract class JsonProcessorBase: IJsonProcessor
    {
        private JObject Json;
        public JObject After => (JObject)(Json["payload"]["after"]);
        public JObject Before => (JObject)(Json["payload"]["before"]);
        public PayloadSource SourceMetadata;
        private Dictionary<string, string> _schema = new Dictionary<string, string>();

        private long _baseLSN = 0;
        private string _sqlConnectionString = ConfigurationManager.AppSettings["AzureSQLConnectionString"];

        public JsonProcessorBase(JObject json)
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
                    Utils.SaveSetting("LastLSN", "0x" + SourceMetadata.CommitLSN.ToString("X"));                 
                }                
            } else {
                Console.WriteLine($"Transaction Commit LSN {SourceMetadata.CommitLSN:X} is greater than base LSN {_baseLSN:X}. Skipping.");
            }
        }        
    }
}