using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Debezium.Consumer
{    
    class JsonProcessorUpdate : JsonProcessorBase
    {
        public JsonProcessorUpdate(JObject json): base(json) {}

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
}