namespace Jupyter.Messages
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class ExecuteReplyOk : ExecuteReplyData
    {
        public ExecuteReplyOk()
        {
            this.Status = ExecutionState.Ok;
        }

        [JsonProperty("payload")]
        public List<Dictionary<string,string>> Payload { get; set; }

        [JsonProperty("user_expressions")]
        public Dictionary<string,string> UserExpressions { get; set; }
    }
}
