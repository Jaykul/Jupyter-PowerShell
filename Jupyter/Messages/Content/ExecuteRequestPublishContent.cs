namespace Jupyter.Messages
{
    using Newtonsoft.Json;

    public class ExecuteRequestPublishContent : Content
    {
        public ExecuteRequestPublishContent(string code, int executionCount)
        {
            Code = code;
            ExecutionCount = executionCount;
        }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("execution_count")]
        public int ExecutionCount { get; set; }
    }
}
