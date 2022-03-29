using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tgs.Report.Client.Models.Round.BetHistories.JsonConverters
{
    public class DataConverter : JsonConverter<BaseData>
    {
        private static readonly Dictionary<int, Type> _TypeMapper = new()
        {
            { 1, typeof(UrlData) },
            { 2, typeof(TableData) },
            { 3, typeof(TextData) },
            { 4, typeof(JsonData) },
            { 5, typeof(XmlData) }
        };

        public override BaseData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;
            var dataType = root.GetProperty("type").GetInt32();
            if (!_TypeMapper.TryGetValue(dataType, out var type))
            {
                return null;
            }

            var o = JsonSerializer.Deserialize(root.GetRawText(), type, options);
            return (BaseData)o;
        }

        public override void Write(Utf8JsonWriter writer, BaseData value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case null:
                    JsonSerializer.Serialize(writer, (BaseData)null, options);
                    break;

                default:
                {
                    var type = value.GetType();
                    JsonSerializer.Serialize(writer, value, type, options);
                    break;
                }
            }
        }
    }
}