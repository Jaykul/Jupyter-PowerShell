namespace Jupyter.Messages
{
    using Newtonsoft.Json;

    public class ExecuteReplyContent : Content
    {
        [JsonProperty("status")]
        public ExecutionResult Status { get; set; }

        [JsonProperty("execution_count")]
        public int ExecutionCount { get; set; }
    }
}
