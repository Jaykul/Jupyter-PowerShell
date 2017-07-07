namespace Jupyter.Messages
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class DisplayDataContent : Content
    {
        public DisplayDataContent(Dictionary<string, object> data)
        {
            Data = data;
        }

        [JsonProperty("data")]
        public Dictionary<string,object> Data { get; set; }

        [JsonProperty("metadata")]
        public Dictionary<string,string> MetaData { get; set; } = new Dictionary<string, string>();

        [JsonProperty("transient")]
        public Dictionary<string, string> Transient { get; set; } = new Dictionary<string, string>();
    }
}
