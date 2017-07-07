namespace Jupyter.Messages
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class ExecuteResultPublishContent : DisplayDataContent
    {
        public ExecuteResultPublishContent(DisplayDataContent content, int executionCount = 0) : base(content.Data)
        {
            MetaData = content.MetaData;
            ExecutionCount = executionCount;
        }

        [JsonProperty("execution_count")]
        public int ExecutionCount { get; set; }
    }
}
