namespace Jupyter.Messages
{
    using Newtonsoft.Json;

    public class StdoutMessage
    {
        [JsonProperty("name")]
        public string Name { get; } = "stdout";

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
