namespace Jupyter.Server
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Web;
    using Microsoft.Extensions.Logging;
    using Jupyter.Messages;
    using NetMQ.Sockets;
    using Newtonsoft.Json;

    public class ExecuteRequestHandler : IMessageHandler
    {
        private readonly ILogger logger;

        private readonly IReplEngine _replEngine;

        private readonly MessageSender messageSender;

        private int executionCount = 1;

        public ExecuteRequestHandler(ILogger logger, IReplEngine replEngine, MessageSender messageSender)
        {
            this.logger = logger;
            this._replEngine = replEngine;
            this.messageSender = messageSender;
        }

        public void HandleMessage(Message message, RouterSocket serverSocket, PublisherSocket ioPub)
        {
            this.logger.LogDebug(string.Format("Message Content {0}", message.Content));
            ExecuteRequestContent executeRequest = message.Content as ExecuteRequestContent;

            this.logger.LogInformation(string.Format("Execute Request received with code {0}", executeRequest.Code));

            // 1: Send Busy status on IOPub
            this.PublishStatus(message, ioPub, KernelState.Busy);

            // 2: Send execute input on IOPub
            this.PublishInput(message, ioPub, executeRequest.Code);

            // 3: Call the engine with the code
            string code = executeRequest.Code;
            var results = _replEngine.Execute(code);

            // 4: Send execute reply to shell socket
            // 5: Send execute result message to IOPub
            if (results.Error != null)
            {
                // SendExecuteErrorMessage(message, serverSocket, results.Error);
                PublishError(message, ioPub, results.Error);
            }
            else
            {
                SendExecuteReplyMessage(message, serverSocket);
                if (results.Output.Any())
                {
                    // 5: Send execute result message to IOPub
                    DisplayDataContent displayData = results.GetDisplayData();
                    PublishOutput(message, ioPub, displayData);
                }
            }

            // 6: Send IDLE status message to IOPub
            this.PublishStatus(message, ioPub, KernelState.Idle);

            // TODO: History
            // The Jupyter Notebook interface does not use history messages
            // However, we're supposed to be storing the history *with output* 
            // So that a HistoryHandler can find it when asked
            if (executeRequest.StoreHistory)
            {
                this.executionCount += 1;
            }
        }

        public void PublishStatus(Message message, PublisherSocket ioPub, KernelState statusValue)
        {
            Message ioPubMessage = new Message( MessageType.Status,
                                                new StatusContent(statusValue), 
                                                message.Header);

            this.logger.LogInformation(string.Format("Sending message to IOPub {0}", JsonConvert.SerializeObject(ioPubMessage)));
            this.messageSender.Send(ioPubMessage, ioPub);
            this.logger.LogInformation("Message Sent");
        }

        public void PublishOutput(Message message, PublisherSocket ioPub, DisplayDataContent data)
        {
            var content = new ExecuteResultPublishContent(data, executionCount);
            Message outputMessage = new Message(MessageType.ExecuteResult, content, message.Header);

            this.logger.LogInformation(string.Format("Sending message to IOPub {0}", JsonConvert.SerializeObject(outputMessage)));
            this.messageSender.Send(outputMessage, ioPub);
        }

        public void PublishInput(Message message, PublisherSocket ioPub, string code)
        {
            var content = new ExecuteRequestPublishContent(code, executionCount);
            Message executeInputMessage = new Message(MessageType.Input, content, message.Header);

            this.logger.LogInformation(string.Format("Sending message to IOPub {0}", JsonConvert.SerializeObject(executeInputMessage)));
            this.messageSender.Send(executeInputMessage, ioPub);
        }

        public void SendExecuteReplyMessage(Message message, RouterSocket shellSocket)
        {
            var content = new ExecuteResultReplyContent()
            {
                ExecutionCount = this.executionCount,
                Payload = new List<Dictionary<string, string>>(),
                UserExpressions = new Dictionary<string, string>()
            };

            Message executeReplyMessage = new Message(  MessageType.ExecuteReply, content, message.Header)
            {
                // Stick the original identifiers on the message so they'll be sent first
                // Necessary since the shell socket is a ROUTER socket
                Identifiers = message.Identifiers
            };

            this.logger.LogInformation(string.Format("Sending message to Shell {0}", JsonConvert.SerializeObject(executeReplyMessage)));
            this.messageSender.Send(executeReplyMessage, shellSocket);
        }

        public void SendExecuteErrorMessage(Message message, RouterSocket shellSocket, IErrorResult error)
        {
            var content = new ExecuteErrorReplyContent()
            {
                ExecutionCount = executionCount,
                //EName = error.Name,
                //EValue = error.Message,
                //Traceback = error.StackTrace
            };

            Message executeReplyMessage = new Message(MessageType.ExecuteReply, content, message.Header)
            {
                // Stick the original identifiers on the message so they'll be sent first
                // Necessary since the shell socket is a ROUTER socket
                Identifiers = message.Identifiers
            };

            this.logger.LogInformation(string.Format("Sending message to Shell {0}", JsonConvert.SerializeObject(executeReplyMessage)));
            this.messageSender.Send(executeReplyMessage, shellSocket);
        }

        private void PublishError(Message message, PublisherSocket ioPub, IErrorResult error)
        {
            // Write to Stderr first -- then write the ExecuteError
            var errorMessage = new StderrContent(error.Message);
            Message stderrMessage = new Message(MessageType.Stream, errorMessage, message.Header)
            {
                Identifiers = message.Identifiers
            };

            this.logger.LogInformation(string.Format("Sending message to IOPub {0}", JsonConvert.SerializeObject(stderrMessage)));
            this.messageSender.Send(stderrMessage, ioPub);


            var executeReply = new ExecuteErrorPublishContent()
            {
                ExecutionCount = executionCount,
                EName = error.Name,
                EValue = error.Message,
                Traceback = error.StackTrace
            };
            Message executeReplyMessage = new Message(MessageType.Error, executeReply, message.Header)
            {
                Identifiers = message.Identifiers
            };
            this.logger.LogInformation(string.Format("Sending message to IOPub {0}", JsonConvert.SerializeObject(executeReplyMessage)));
            this.messageSender.Send(executeReplyMessage, ioPub);

        }
    }
}
