namespace Jupyter.Messages
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    /// <summary>
    /// The base class for all Jupyter Messages
    /// </summary>
    public class Message
    {
        public Message(MessageType messageType, Content content, Header parentHeader)
        {
            UUID = parentHeader.Session;
            ParentHeader = parentHeader;
            Content = content;
            Header = new Header(messageType, parentHeader.Session);
        }

        public Message(MessageType messageType, Content content, Header parentHeader, List<byte[]> identifier, Header header, string hmac , Dictionary<string, object> metaData) :
            this(messageType, content, parentHeader)
        {
            Identifiers = identifier;
            Header = header;
            HMac = hmac;
            MetaData = metaData;
        }

        [JsonProperty("identifiers")]
        public List<byte[]> Identifiers { get; set; } = new List<byte[]>();

        [JsonProperty("uuid")]
        public string UUID { get; set; } = string.Empty;

        [JsonProperty("hmac")]
        public string HMac { get; set; } = string.Empty;

        [JsonProperty("header")]
        public Header Header { get; set; }

        [JsonProperty("parent_header")]
        public Header ParentHeader { get; set; }

        [JsonProperty("metadata")]
        public Dictionary<string, object> MetaData { get; set; } = new Dictionary<string, object>();

        [JsonProperty("content")]
        public Content Content { get; set; } = new Content();
    }
}
