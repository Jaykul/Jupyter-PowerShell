namespace Jupyter.Messages
{
    using Newtonsoft.Json;

    public class StdoutContent : Content
    {
        public StdoutContent(string text)
        {
            Text = text;
        }

        [JsonProperty("name")]
        public string Name { get; } = "stdout";

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
