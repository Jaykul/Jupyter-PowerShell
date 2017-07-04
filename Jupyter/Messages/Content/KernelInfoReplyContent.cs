namespace Jupyter.Messages
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public class KernelInfoReplyContent : Content
    {
        [JsonProperty("protocol_version")]
        public string ProtocolVersion { get; set; }

        [JsonProperty("implementation")]
        public string Implementation { get; set; }

        [JsonProperty("implementation_version")]
        public string ImplementationVersion { get; set; }

        [JsonProperty("language_info")]
        public LanguageInfoContent LanguageInfo { get; set; }

        [JsonProperty("banner")]
        public string Banner { get; set; }

        [JsonProperty("help_links")]
        public List<Dictionary<string, string>> HelpLinks { get; set; }
    }
}
