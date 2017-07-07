namespace Jupyter.Server
{
    using Microsoft.Extensions.Logging;
    using Jupyter.Messages;
    using NetMQ.Sockets;
    using Newtonsoft.Json;

    public class ShutdownHandler : IMessageHandler
    {
        private readonly ILogger _logger;
        private readonly Heartbeat _heartbeat;

        private readonly Shell _shell;

        public ShutdownHandler(ILogger logger, Heartbeat heartbeat, Shell shell)
        {
            _logger = logger;
            _heartbeat = heartbeat;
            _shell = shell;
        }

        public void HandleMessage(Message message, RouterSocket serverSocket, PublisherSocket ioPub)
        {
            var shutdownRequest = message.Content as ShutdownContent;

            _logger.LogInformation("Stopping heartbeat");
            _heartbeat.Stop();

            Message replyMessage = new Message(MessageType.ShutDownReply, shutdownRequest, message.Header);

            _logger.LogInformation("Sending shutdown_response");
            serverSocket.SendMessage(replyMessage);

            _logger.LogInformation("Stopping shell");
            _shell.Stop();
        }
    }
}