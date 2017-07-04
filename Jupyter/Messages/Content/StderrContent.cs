namespace Jupyter.Messages
{
    using Newtonsoft.Json;

    public class StderrMessage
    {
        [JsonProperty("name")]
        public string Name { get; } = "stderr";

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
