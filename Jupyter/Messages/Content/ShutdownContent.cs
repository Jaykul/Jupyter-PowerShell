namespace Jupyter.Messages
{
    using Newtonsoft.Json;

    public class ShutdownContent : Content
    {
        [JsonProperty("restart")]
        public bool Restart { get; set; }


    }
}
