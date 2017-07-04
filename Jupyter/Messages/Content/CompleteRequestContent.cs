namespace Jupyter.Messages
{
    using Newtonsoft.Json;

    public class CompleteRequestContent : Content
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("cursor_pos")]
        public int CursorPosition { get; set; }
    }
}
