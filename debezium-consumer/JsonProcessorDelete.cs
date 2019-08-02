using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Debezium.Consumer
{    
    class JsonProcessorDelete : JsonProcessorBase
    {
        public JsonProcessorDelete(JObject json): base(json) {}

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
}