namespace Jupyter.Server.Handlers
{
    using Microsoft.Extensions.Logging;
    using Jupyter.Messages;
    using NetMQ.Sockets;
    using Newtonsoft.Json;

    public class CommInfoHandler : IMessageHandler
    {
        private readonly ILogger _logger;


        public CommInfoHandler(ILogger logger)
        {
            _logger = logger;
        }

        public void HandleMessage(Message message, RouterSocket serverSocket, PublisherSocket ioPub)
        {
            TargetName targetName = message.Content as TargetName;
            Message replyMessage = new Message(MessageType.CommInfoReply, CreateCommInfoReply(targetName), message.Header);

            _logger.LogInformation("Sending comm_info_reply");
            serverSocket.SendMessage(replyMessage);
        }

        private CommInfoReplyContent CreateCommInfoReply(TargetName name)
        {
            CommInfoReplyContent CommInfoReply = new CommInfoReplyContent( new System.Collections.Generic.Dictionary<string, TargetName> { })
            {

            };

            return CommInfoReply;
        }
    }
}