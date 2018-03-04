// namespace Jupyter.Server.Handlers
// {
//     using Microsoft.Extensions.Logging;
//     using Jupyter.Messages;
//     using NetMQ.Sockets;
//     using Newtonsoft.Json;

//     public class CommOpenHandler : IMessageHandler
//     {
//         private readonly ILogger _logger;


//         public CommOpenHandler(ILogger logger)
//         {
//             _logger = logger;
//         }

//         public void HandleMessage(Message message, RouterSocket serverSocket, PublisherSocket ioPub)
//         {
//             TargetName targetName = message.Content as TargetName;
//             Message replyMessage = new Message(MessageType.CommClose, message.Header);

//             _logger.LogInformation("Sending comm_info_reply");
//             serverSocket.SendMessage(replyMessage);
//         }

//     }
// }