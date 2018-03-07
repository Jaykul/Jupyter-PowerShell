namespace Jupyter.Messages
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System.Runtime.Serialization;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ExecutionResult
    {
        [EnumMember(Value = "ok")]
        Ok,

        [EnumMember(Value = "error")]
        Error,

        [EnumMember(Value = "abort")]
        Abort,
    }
}
