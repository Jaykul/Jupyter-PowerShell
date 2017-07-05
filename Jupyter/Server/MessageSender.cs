namespace Jupyter.Server
{
    using Jupyter.Messages;
    using Microsoft.Extensions.Logging;
    using NetMQ;
    using Newtonsoft.Json;

    public class MessageSender
    {
        private readonly Validator _validator;
        private readonly ILogger _logger;
        private readonly JsonSerializerSettings _ignoreLoops = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

        public MessageSender(Validator validator, Microsoft.Extensions.Logging.ILogger logger)
        {
            _validator = validator;
            _logger = logger;
        }

        public bool Send(NetMQSocket socket, Message message)
        {
            _logger.LogTrace("Sending Message: {0}", JsonConvert.SerializeObject(message));
            string hmac = _validator.CreateSignature(message);

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
                Send(socket, message.UUID, true);
            }

            Send(socket, Constants.DELIMITER);
            Send(socket, hmac);
            Send(socket, message.Header);
            Send(socket, message.ParentHeader);
            Send(socket, message.MetaData);
            Send(socket, message.Content, false);

            return true;
        }
        

        private void Send(NetMQSocket socket, object message, bool sendMore = true)
        {
            string frame = JsonConvert.SerializeObject(message, _ignoreLoops);
            socket.SendFrame(frame, sendMore);
        }

        private void Send(NetMQSocket socket, string message, bool sendMore = true)
        {
            socket.SendFrame(message, sendMore);
        }


    }
}
