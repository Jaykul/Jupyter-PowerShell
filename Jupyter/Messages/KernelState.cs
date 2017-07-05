namespace Jupyter.Messages
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System.Runtime.Serialization;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum KernelState
    {
        [EnumMember(Value = "busy")]
        Busy,

        [EnumMember(Value = "idle")]
        Idle,

        [EnumMember(Value = "starting")]
        Starting
    }
}
