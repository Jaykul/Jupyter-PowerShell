namespace Jupyter.Messages
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class Status
    {
        [JsonProperty("execution_state")]
        public ExecutionState ExecutionState { get; set; }
    }
}
