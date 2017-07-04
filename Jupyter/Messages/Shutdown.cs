namespace Jupyter.Messages
{
    using Newtonsoft.Json;

    public class Shutdown
    {
        [JsonProperty("restart")]
        public bool Restart { get; set; }


    }
}
