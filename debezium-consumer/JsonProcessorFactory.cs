using System;
using Newtonsoft.Json.Linq;

namespace Debezium.Consumer
{    
    class JsonProcessorFactory
    {
        public static JsonProcessorBase CreatePayloadProcessor(string operation, JObject json)
        {
            switch (operation)
            {
                case "c": return new JsonProcessorCreate(json);
                case "u": return new JsonProcessorUpdate(json);
                case "d": return new JsonProcessorDelete(json);
                default: throw new ArgumentException($"Unknown '{operation}' value for operation");
            }
        }
    }
}