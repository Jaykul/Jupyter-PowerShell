namespace Jupyter.Messages
{
    using Newtonsoft.Json;

    public class StderrContent : Content
    {
        public StderrContent(string text)
        {
            Text = text;
        }

        [JsonProperty("name")]
        public string Name { get; } = "stderr";

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
