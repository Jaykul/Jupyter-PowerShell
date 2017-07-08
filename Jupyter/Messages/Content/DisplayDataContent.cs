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
        public Dictionary<string, object> MetaData { get; set; } = new Dictionary<string, object>();

        [JsonProperty("transient")]
        public Dictionary<string, object> Transient { get; set; } = new Dictionary<string, object>();
    }
}
