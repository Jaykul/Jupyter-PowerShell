// namespace Jupyter.Messages
// {
//     using System.Linq;
//     using System.Collections.Generic;
//     using Newtonsoft.Json;

//     public class CommMessage : Message
//     {
//         public CommMessage()
//         {
//             Data = new Dictionary<string, object>();
//         }

//         [JsonProperty("data")]
//         public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

//         [JsonProperty("comm_id")]
//         public string CommId { get; set; } = string.Empty;
//     }
// }
