namespace Jupyter.Messages
{
    using Newtonsoft.Json;

    public class TargetName : Content
    {
        public TargetName(string name)
        {
            Name = name;
        }

        [JsonProperty("target_name")]
        public string Name { get; set; } = string.Empty;
    }
}