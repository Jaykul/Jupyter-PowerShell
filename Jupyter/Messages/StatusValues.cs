namespace Jupyter.Messages
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System.Runtime.Serialization;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ExecutionState
    {

        [EnumMember(Value = "ok")]
        Ok,

        [EnumMember(Value = "error")]
        Error,

        [EnumMember(Value = "abort")]
        Abort,

        [EnumMember(Value = "busy")]
        Busy,

        [EnumMember(Value = "idle")]
        Idle,

        [EnumMember(Value = "starting")]
        Starting
    }
}
