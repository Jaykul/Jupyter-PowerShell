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
        private readonly ILogger _logger;
        private readonly IReplEngine _replEngine;
        
        private int _executionCount = 1;

        public ExecuteRequestHandler(ILogger logger, IReplEngine replEngine)
        {
            _logger = logger;
            _replEngine = replEngine;
        }

        public void HandleMessage(Message request, RouterSocket serverSocket, PublisherSocket ioPub)
        {
            _logger.LogDebug(string.Format("Message Content {0}", request.Content));
            ExecuteRequestContent executeRequest = request.Content as ExecuteRequestContent;

            _logger.LogInformation(string.Format("Execute Request received with code {0}", executeRequest.Code));

            // 1: Send Busy status on IOPub
            PublishStatus(request, ioPub, KernelState.Busy);

            // 2: Send execute input on IOPub
            PublishInput(request, ioPub, executeRequest.Code);

            // 3: Call the engine with the code
            string code = executeRequest.Code;
            var results = _replEngine.Execute(code);

            // 4: Send execute reply to shell socket
            // 5: Send execute result message to IOPub
            if (results.Error != null)
            {
                // SendExecuteErrorMessage(message, serverSocket, results.Error);
                PublishError(request, ioPub, results.Error);
            }
            else
            {
                SendExecuteReplyMessage(request, serverSocket);
                if (results.Output.Any())
                {
                    // 5: Send execute result message to IOPub
                    DisplayDataContent displayData = results.GetDisplayData();
                    PublishOutput(request, ioPub, displayData);
                }
            }

            // 6: Send IDLE status message to IOPub
            this.PublishStatus(request, ioPub, KernelState.Idle);

            // TODO: History
            // The Jupyter Notebook interface does not use history messages
            // However, we're supposed to be storing the history *with output* 
            // So that a HistoryHandler can find it when asked
            if (executeRequest.StoreHistory)
            {
                this._executionCount += 1;
            }
        }

        public void PublishStatus(Message request, PublisherSocket ioPub, KernelState statusValue)
        {
            Message message = new Message(MessageType.Status, new StatusContent(statusValue), request.Header);

            this._logger.LogInformation(string.Format("Sending message to IOPub {0}", JsonConvert.SerializeObject(message)));
            ioPub.SendMessage(message);
            this._logger.LogInformation("Message Sent");
        }

        public void PublishOutput(Message request, PublisherSocket ioPub, DisplayDataContent data)
        {
            var content = new ExecuteResultPublishContent(data, _executionCount);
            Message message = new Message(MessageType.ExecuteResult, content, request.Header);

            this._logger.LogInformation(string.Format("Sending message to IOPub {0}", JsonConvert.SerializeObject(message)));
            ioPub.SendMessage(message);
        }

        public void PublishInput(Message request, PublisherSocket ioPub, string code)
        {
            var content = new ExecuteRequestPublishContent(code, _executionCount);
            Message message = new Message(MessageType.Input, content, request.Header);

            this._logger.LogInformation(string.Format("Sending message to IOPub {0}", JsonConvert.SerializeObject(message)));
            ioPub.SendMessage(message);
        }

        public void SendExecuteReplyMessage(Message request, RouterSocket shellSocket)
        {
            var content = new ExecuteResultReplyContent()
            {
                ExecutionCount = this._executionCount,
                Payload = new List<Dictionary<string, string>>(),
                UserExpressions = new Dictionary<string, string>()
            };

            Message message = new Message(MessageType.ExecuteReply, content, request.Header)
            {
                // Stick the original identifiers on the message so they'll be sent first
                // Necessary since the shell socket is a ROUTER socket
                Identifiers = request.Identifiers
            };

            this._logger.LogInformation(string.Format("Sending message to Shell {0}", JsonConvert.SerializeObject(message)));
            shellSocket.SendMessage(message);
        }

        public void SendExecuteErrorMessage(Message request, RouterSocket shellSocket, IErrorResult error)
        {
            var content = new ExecuteErrorReplyContent()
            {
                ExecutionCount = _executionCount,
                //EName = error.Name,
                //EValue = error.Message,
                //Traceback = error.StackTrace
            };

            Message message = new Message(MessageType.ExecuteReply, content, request.Header)
            {
                // Stick the original identifiers on the message so they'll be sent first
                // Necessary since the shell socket is a ROUTER socket
                Identifiers = request.Identifiers
            };

            this._logger.LogInformation(string.Format("Sending message to Shell {0}", JsonConvert.SerializeObject(message)));
            shellSocket.SendMessage(message);
        }

        private void PublishError(Message request, PublisherSocket ioPub, IErrorResult error)
        {
            // Write to Stderr first -- then write the ExecuteError
            var errorMessage = new StderrContent(error.Message);
            Message message = new Message(MessageType.Stream, errorMessage, request.Header)
            {
                Identifiers = request.Identifiers
            };

            this._logger.LogInformation(string.Format("Sending message to IOPub {0}", JsonConvert.SerializeObject(message)));
            ioPub.SendMessage(message);


            var executeReply = new ExecuteErrorPublishContent()
            {
                ExecutionCount = _executionCount,
                EName = error.Name,
                EValue = error.Message,
                Traceback = error.StackTrace
            };
            message = new Message(MessageType.Error, executeReply, request.Header)
            {
                Identifiers = request.Identifiers
            };
            this._logger.LogInformation(string.Format("Sending message to IOPub {0}", JsonConvert.SerializeObject(message)));
            ioPub.SendMessage(message);

        }
    }
}
