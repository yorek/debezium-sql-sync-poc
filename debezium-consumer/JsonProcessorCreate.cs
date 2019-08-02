using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Debezium.Consumer
{    
    class JsonProcessorCreate : JsonProcessorBase
    {
        public JsonProcessorCreate(JObject json): base(json) {}

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
}