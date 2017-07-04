namespace Jupyter.Messages
{
    using Newtonsoft.Json;

    /// <summary>
    /// The Jupyter Message Header
    /// </summary>
    public class Header
    {
        [JsonProperty("msg_id")]
        public string MessageId { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("session")]
        public string Session { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("msg_type")]
        public MessageType MessageType { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        public Header()
        {
        }

        public Header(MessageType messageType, string session)
        {
            Username = Constants.USERNAME;
            Session = session;
            MessageId = System.Guid.NewGuid().ToString();
            MessageType = messageType;
            Version = Constants.PROTOCOL_VERSION;
        }
    }
}
