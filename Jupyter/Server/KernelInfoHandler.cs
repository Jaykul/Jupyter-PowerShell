namespace Jupyter.Server
{
    using Microsoft.Extensions.Logging;
    using Jupyter.Messages;
    using NetMQ.Sockets;
    using Newtonsoft.Json;

    public class KernelInfoHandler : IMessageHandler
    {
        private readonly ILogger _logger;

        private readonly MessageSender _sender;

        public KernelInfoHandler(ILogger logger, MessageSender messageSender)
        {
            _logger = logger;
            _sender = messageSender;
        }

        public void HandleMessage(Message message, RouterSocket serverSocket, PublisherSocket ioPub)
        {
            KernelInfoRequestData kernelInfoRequest = JsonConvert.DeserializeObject<KernelInfoRequestData>(message.Content);

            Message replyMessage = new Message()
            {
                UUID = message.Header.Session,
                ParentHeader = message.Header,
                Header = new Header(MessageType.KernelInfoReply, message.Header.Session),
                Content = JsonConvert.SerializeObject(CreateKernelInfoReply())
            };

            _logger.LogInformation("Sending kernel_info_reply");
            _sender.Send(replyMessage, serverSocket);
        }

        private KernelInfoReplyData CreateKernelInfoReply()
        {
            KernelInfoReplyData kernelInfoReply = new KernelInfoReplyData()
            {
                ProtocolVersion = "5.0",
                Implementation = "iPowerShell",
                ImplementationVersion = "0.0.1",
                LanguageInfo = new LanguageInfoData()
                {
                    Name = "PowerShell",
                    Version = "5.0",
                    MimeType = "text/powershell",
                    FileExtension = ".ps1",
                    PygmentsLexer = "powershell",
                    CodemirrorMode = "powershell"
                }
            };

            return kernelInfoReply;
        }
    }
}