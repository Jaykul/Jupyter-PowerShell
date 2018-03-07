namespace Jupyter.Messages
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class ExecuteErrorPublishContent : Content
    {
        [JsonProperty("execution_count")]
        public int ExecutionCount { get; set; }

        [JsonProperty("ename")]
        public string EName { get; set; }

        [JsonProperty("evalue")]
        public string EValue { get; set; }

        [JsonProperty("traceback")]
        public List<string> Traceback { get; set; }
    }
}
