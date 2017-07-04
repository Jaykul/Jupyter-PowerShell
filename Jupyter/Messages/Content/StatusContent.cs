namespace Jupyter.Messages
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class StatusContent : Content
    {
        public StatusContent(ExecutionState state)
        {
            ExecutionState = state;
        }

        [JsonProperty("execution_state")]
        public ExecutionState ExecutionState { get; set; }
    }
}
