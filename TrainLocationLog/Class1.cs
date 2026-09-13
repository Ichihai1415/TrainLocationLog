using System.Text.Json;
using System.Text.Json.Serialization;

namespace TrainLocationLog
{

    public class HF_car_type
    {
        [JsonPropertyName("dt")]
        public string Dt { get; set; }

        [JsonPropertyName("type")]
        public CType[] Type { get; set; }

        public class CType
        {
            [JsonPropertyName("style_line")]
            public string StyleLine { get; set; }

            [JsonPropertyName("code")]
            public string Code { get; set; }

            [JsonPropertyName("style_hrm")]
            public string StyleHrm { get; set; }

            [JsonPropertyName("flg")]
            public string Flg { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }
        }
    }


    public class HF_station
    {
        [JsonPropertyName("ikisaki")]
        public CIkisaki[] Ikisaki { get; set; }

        [JsonPropertyName("dt")]
        public string Dt { get; set; }

        public class CIkisaki
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("code")]
            public string Code { get; set; }

            [JsonPropertyName("yomi")]
            public string Yomi { get; set; }

            [JsonPropertyName("eiji")]
            public string Eiji { get; set; }
        }
    }


    public class OPDT
    {
        [JsonPropertyName("context")]
        public string Context { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("dc:date")]
        public DateTime DcDate { get; set; }

        [JsonPropertyName("dct:valid")]
        public DateTime DctValid { get; set; }

        [JsonPropertyName("odpt:frequency")]
        public int OdptFrequency { get; set; }

        [JsonPropertyName("odpt:railway")]
        public string OdptRailway { get; set; }

        [JsonPropertyName("owl:sameAs")]
        public string OwlSameAs { get; set; }

        [JsonPropertyName("odpt:trainNumber")]
        public string OdptTrainNumber { get; set; }

        [JsonPropertyName("odpt:trainType")]
        public string OdptTrainType { get; set; }

        [JsonPropertyName("odpt:delay")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string OdptDelay { get; set; }

        [JsonPropertyName("odpt:startingStation")]
        public string OdptStartingStation { get; set; }

        [JsonPropertyName("odpt:terminalStation")]
        public string OdptTerminalStation { get; set; }

        [JsonPropertyName("odpt:fromStation")]
        public string OdptFromStation { get; set; }

        [JsonPropertyName("odpt:toStation")]
        public string OdptToStation { get; set; }

        [JsonPropertyName("odpt:railDirection")]
        public string OdptRailDirection { get; set; }

        [JsonPropertyName("odpt:trainOwner")]
        public string OdptTrainOwner { get; set; }


    }


    public class AK_station
    {
        [JsonPropertyName("result")]
        public CResult Result { get; set; }

        public class CResult
        {
            [JsonPropertyName("status")]
            public string Status { get; set; }

            [JsonPropertyName("data")]
            public List<CData> Data { get; set; }

            public class CData
            {
                [JsonPropertyName("id")]
                public int Id { get; set; }

                [JsonPropertyName("created")]
                public DateTime? Created { get; set; }

                [JsonPropertyName("modified")]
                public DateTime? Modified { get; set; }

                [JsonPropertyName("deleted")]
                public object Deleted { get; set; }

                [JsonPropertyName("sort")]
                public int Sort { get; set; }

                [JsonPropertyName("station_name_ja")]
                public string StationNameJa { get; set; }

                [JsonPropertyName("station_name_kana")]
                public string StationNameKana { get; set; }

                [JsonPropertyName("station_name_en")]
                public string StationNameEn { get; set; }

                [JsonPropertyName("json_name")]
                public string JsonName { get; set; }

                [JsonPropertyName("ainokaze_flag")]
                public bool AinokazeFlag { get; set; }

                [JsonPropertyName("liner_flag")]
                public bool LinerFlag { get; set; }

                [JsonPropertyName("color_flag")]
                public bool ColorFlag { get; set; }

                [JsonPropertyName("train_display_flag")]
                public bool TrainDisplayFlag { get; set; }

                [JsonPropertyName("up_stop_station_ja")]
                public string UpStopStationJa { get; set; }

                [JsonPropertyName("up_stop_station_en")]
                public string UpStopStationEn { get; set; }

                [JsonPropertyName("down_stop_station_ja")]
                public string DownStopStationJa { get; set; }

                [JsonPropertyName("down_stop_station_en")]
                public string DownStopStationEn { get; set; }
            }

        }

    }


    public class AK_data
    {
        [JsonPropertyName("result")]
        public CResult Result { get; set; }
    }


    public class CResult
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("data")]
        public string Data { get; set; }
    }

    public class FlexibleStringConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.TokenType switch
            {
                JsonTokenType.String => reader.GetString(),
                JsonTokenType.Number => reader.GetInt32().ToString(),
                JsonTokenType.Null => null,
                _ => throw new JsonException($"Unexpected token {reader.TokenType}")
            };
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }


}
