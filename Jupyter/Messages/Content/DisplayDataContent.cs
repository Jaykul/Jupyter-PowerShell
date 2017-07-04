namespace Jupyter.Messages
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class DisplayDataContent : Content
    {
        [JsonProperty("data")]
        public Dictionary<string,object> Data { get; set; } = new Dictionary<string, object>();

        [JsonProperty("metadata")]
        public Dictionary<string,string> MetaData { get; set; } = new Dictionary<string, string>();
    }
}
