namespace Jupyter.Server
{
    using Jupyter.Messages;
    using Microsoft.Extensions.Logging;
    using NetMQ;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;

    public static class MessageSender
    {
        private static readonly JsonSerializerSettings _ignoreLoops = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

        public static Validator Validator { get; set; }
        public static ILogger Logger { get; set; }

        private static string SessionId;
        private static Header lastParentHeader;

        public static bool SendMessage(this NetMQSocket socket, Message message)
        {
            Logger?.LogTrace("Sending Message: {0}", JsonConvert.SerializeObject(message));
            if(message.Header.MessageType == MessageType.Input)
            {
                lastParentHeader = message.ParentHeader;
            }
            if (string.IsNullOrEmpty(message.UUID))
            {
                message.UUID = SessionId;
            }

            if (string.IsNullOrEmpty(message.Header.Session))
            {
                message.Header.Session = SessionId;
                message.ParentHeader = lastParentHeader;
            }

            var messageFrames = new[] {
                JsonConvert.SerializeObject(message.Header),
                JsonConvert.SerializeObject(message.ParentHeader),
                JsonConvert.SerializeObject(message.MetaData),
                JsonConvert.SerializeObject(message.Content)
            };
            string hmac = Validator.CreateSignature(messageFrames);

            if (message.Identifiers != null && message.Identifiers.Count > 0)
            {
                // Send ZMQ identifiers from the message we're responding to.
                // This is important when we're dealing with ROUTER sockets, like the shell socket,
                // because the message won't be sent unless we manually include these.
                foreach (var ident in message.Identifiers)
                {
                    socket.TrySendFrame(ident, true);
                }
            }
            else
            {
                // This is just a normal message so send the UUID
                socket.SendFrame(message.UUID, true);
            }

            socket.SendFrame(Constants.DELIMITER, true);
            socket.SendFrame(hmac, true);
            socket.SendFrame(messageFrames[0], true);
            socket.SendFrame(messageFrames[1], true);
            socket.SendFrame(messageFrames[2], true);
            socket.SendFrame(messageFrames[3], false);

            return true;
        }

        public static Message ReceiveMessage(this NetMQSocket socket)
        {
            // There may be additional ZMQ identities attached; read until the delimiter <IDS|MSG>"
            // and store them in message.identifiers
            // http://ipython.org/ipython-doc/dev/development/messaging.html#the-wire-protocol
            byte[] delimiterBytes = Encoding.ASCII.GetBytes(Constants.DELIMITER);
            byte[] delimiter;
            var identifier = new List<byte[]>();
            do
            {
                delimiter = socket.ReceiveFrameBytes();
                identifier.Add(delimiter);
            } while (!delimiter.SequenceEqual(delimiterBytes));
            // strip delimiter
            identifier.RemoveAt(identifier.Count - 1);

            var hmac = socket.ReceiveFrameString();
            var headerFrame = socket.ReceiveFrameString();
            var parentFrame = socket.ReceiveFrameString();
            var metadataFrame = socket.ReceiveFrameString();
            var contentFrame = socket.ReceiveFrameString();

            if (!Validator.IsValidSignature(hmac, headerFrame, parentFrame, metadataFrame, contentFrame))
            {
                return null;
            }

            var header = JsonConvert.DeserializeObject<Header>(headerFrame);
            Content content;

            switch (header.MessageType)
            {
                case MessageType.ExecuteRequest:
                    content = JsonConvert.DeserializeObject<ExecuteRequestContent>(contentFrame);
                    break;
                case MessageType.CompleteRequest:
                    content = JsonConvert.DeserializeObject<CompleteRequestContent>(contentFrame);
                    break;
                case MessageType.ShutDownRequest:
                    content = JsonConvert.DeserializeObject<ShutdownContent>(contentFrame);
                    break;
                case MessageType.KernelInfoRequest:
                    content = JsonConvert.DeserializeObject<Content>(contentFrame);
                    break;
                //case MessageType.ExecuteInput:
                //case MessageType.ExecuteReply:
                //case MessageType.ExecuteResult:
                //case MessageType.CompleteReply:
                //case MessageType.ShutDownReply:
                //case MessageType.KernelInfoReply:

                //case MessageType.Status:
                //case MessageType.Output:
                //case MessageType.Input:
                //case MessageType.Error:
                //case MessageType.Stream:
                default:
                    Logger?.LogInformation(header.MessageType + " message not handled.");
                    content = new Content();
                    break;
            }

            // Update the static session
            SessionId = header.Session;

            return new Message(header.MessageType, content, JsonConvert.DeserializeObject<Header>(parentFrame),
                        identifier, header, hmac, JsonConvert.DeserializeObject<Dictionary<string, object>>(metadataFrame));
        }

    }
}
