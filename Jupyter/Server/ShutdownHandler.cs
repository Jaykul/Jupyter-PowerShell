namespace Jupyter.Server
{
    using Microsoft.Extensions.Logging;
    using Jupyter.Messages;
    using NetMQ.Sockets;
    using Newtonsoft.Json;

    public class ShutdownHandler : IMessageHandler
    {
        private readonly ILogger _logger;

        private readonly MessageSender _sender;

        public ShutdownHandler(ILogger logger, MessageSender messageSender)
        {
            _logger = logger;
            _sender = messageSender;
        }

        public void HandleMessage(Message message, RouterSocket serverSocket, PublisherSocket ioPub)
        {
            Shutdown shutdownRequest = JsonConvert.DeserializeObject<Shutdown>(message.Content);

            // TODO: Restart the PowerShell Engine

            Message replyMessage = new Message()
            {
                UUID = message.Header.Session,
                ParentHeader = message.Header,
                Header = new Header(MessageType.ShutDownReply, message.Header.Session),
                Content = JsonConvert.SerializeObject(new Shutdown { Restart = shutdownRequest.Restart })
            };

            _logger.LogInformation("Sending shutdown_response");
            _sender.Send(replyMessage, serverSocket);
        }
    }
}