namespace Jupyter.Messages
{
    using System.Linq;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class CommInfoReplyContent : Content
    {

        public CommInfoReplyContent()
        {
            Comms = new Dictionary<string, TargetName>();
        }

        public CommInfoReplyContent(Dictionary<string, TargetName> comms)
        {
            Comms = comms;
        }

        public CommInfoReplyContent(Dictionary<string, string> comms)
        {
            Comms = comms.ToDictionary(kvp => kvp.Key, kvp => new TargetName(kvp.Value));
        }

        [JsonProperty("comms")]
        public Dictionary<string, TargetName> Comms { get; set; } 
    }
}
