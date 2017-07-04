namespace Jupyter.Messages
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class ExecuteErrorPublishContent : ExecuteReplyContent
    {
        public ExecuteErrorPublishContent()
        {
            this.Status = ExecutionState.Error;
        }

        [JsonProperty("ename")]
        public string EName { get; set; }

        [JsonProperty("evalue")]
        public string EValue { get; set; }

        [JsonProperty("traceback")]
        public List<string> Traceback { get; set; }
    }
}
