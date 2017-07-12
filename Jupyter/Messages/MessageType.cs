using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Jupyter.Messages
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MessageType
    {
        [EnumMember(Value = "execute_request")]
        ExecuteRequest,
        [EnumMember(Value = "execute_input")]
        ExecuteInput,
        [EnumMember(Value = "execute_reply")]
        ExecuteReply,
        [EnumMember(Value = "execute_result")]
        ExecuteResult,

        [EnumMember(Value = "display_data")]
        DisplayData,
        [EnumMember(Value = "update_display_data")]
        UpdateDisplayData,

        [EnumMember(Value = "kernel_info_request")]
        KernelInfoRequest,
        [EnumMember(Value = "kernel_info_reply")]
        KernelInfoReply,

        [EnumMember(Value = "comm_open")]
        CommOpen,
        [EnumMember(Value = "comm_msg")]
        CommMessage,
        [EnumMember(Value = "comm_close")]
        CommClose,
        [EnumMember(Value = "comm_info")]
        CommInfo,
        [EnumMember(Value = "comm_info_request")]
        CommInfoRequest,
        [EnumMember(Value = "comm_info_reply")]
        CommInfoReply,

        [EnumMember(Value = "inspect_request")]
        InspectRequest,
        [EnumMember(Value = "inspect_reply")]
        InspectReply,

        [EnumMember(Value = "clear_output")]
        ClearOutput,

        [EnumMember(Value = "complete_request")]
        CompleteRequest,
        [EnumMember(Value = "complete_reply")]
        CompleteReply,

        [EnumMember(Value = "shutdown_request")]
        ShutDownRequest,
        [EnumMember(Value = "shutdown_reply")]
        ShutDownReply,

        [EnumMember(Value = "status")]
        Status,

        [EnumMember(Value = "pyout")]
        Output,

        [EnumMember(Value = "pyin")]
        Input,

        [EnumMember(Value = "error")]
        Error,

        [EnumMember(Value = "stream")]
        Stream,
    }
}
