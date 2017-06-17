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

        public MessageSender(Validator validator, Microsoft.Extensions.Logging.ILogger logger)
        {
            _validator = validator;
            _logger = logger;
        }

        public bool Send(Message message, NetMQSocket socket)
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
                Send(message.UUID, socket);
            }

            Send(Constants.DELIMITER, socket);
            Send(hmac, socket);
            Send(JsonConvert.SerializeObject(message.Header), socket);
            Send(JsonConvert.SerializeObject(message.ParentHeader), socket);
            Send(JsonConvert.SerializeObject(message.MetaData), socket);
            Send(message.Content, socket, false);

            return true;
        }

        private void Send(string message, NetMQSocket socket, bool sendMore = true)
        {
            socket.SendFrame(message, sendMore);
        }


    }
}
