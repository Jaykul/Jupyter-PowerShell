namespace Jupyter.Messages
{
    using Newtonsoft.Json;

    public class ExecuteRequestPublishContent : Content
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("execution_count")]
        public int ExecutionCount { get; set; }
    }
}
