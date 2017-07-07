namespace Jupyter.Messages
{
    using Newtonsoft.Json;

    /// <summary>
    /// The Jupyter Message Header
    /// </summary>
    public class Header
    {
        [JsonProperty("msg_id")]
        public string MessageId { get; set; } = System.Guid.NewGuid().ToString("N");

        [JsonProperty("username")]
        public string Username { get; set; } = Constants.USERNAME;

        [JsonProperty("session")]
        public string Session { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; } = System.DateTimeOffset.UtcNow.ToString("o");

        [JsonProperty("msg_type")]
        public MessageType MessageType { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; } = Constants.PROTOCOL_VERSION;

        public Header(MessageType messageType, string session)
        {
            Session = session;
            MessageType = messageType;
        }
    }
}
