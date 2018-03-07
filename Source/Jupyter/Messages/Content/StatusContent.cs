namespace Jupyter.Messages
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class StatusContent : Content
    {
        public StatusContent(KernelState state)
        {
            ExecutionState = state;
        }

        [JsonProperty("execution_state")]
        public KernelState ExecutionState { get; set; }
    }
}
