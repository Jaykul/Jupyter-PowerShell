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
            ExecuteRequestData executeRequest = JsonConvert.DeserializeObject<ExecuteRequestData>(message.Content);

            this.logger.LogInformation(string.Format("Execute Request received with code {0}", executeRequest.Code));

            // 1: Send Busy status on IOPub
            this.SendMessageToIOPub(message, ioPub, ExecutionState.Busy);

            // 2: Send execute input on IOPub
            this.SendInputMessageToIOPub(message, ioPub, executeRequest.Code);

            // 3: Call the engine with the code
            string code = executeRequest.Code;
            var results = _replEngine.Execute(code);

            // 4: Send execute reply to shell socket
            // 5: Send execute result message to IOPub
            if (results.Error != null)
            {
                SendExecuteErrorMessage(message, serverSocket, results.Error);
                SendErrorToIOPub(message, ioPub, results.Error);
            }
            else
            {
                SendExecuteReplyMessage(message, serverSocket);
                if (results.Output.Any())
                {
                    // 5: Send execute result message to IOPub
                    DisplayData displayData = results.GetDisplayData();
                    SendOutputMessageToIOPub(message, ioPub, displayData);
                }
            }


            // 6: Send IDLE status message to IOPub
            this.SendMessageToIOPub(message, ioPub, ExecutionState.Idle);

            // TODO: History
            // The Jupyter Notebook interface does not use history messages
            // However, we're supposed to be storing the history *with output* 
            // So that a HistoryHandler can find it when asked
            if (executeRequest.StoreHistory)
            {
                this.executionCount += 1;
            }

        }


        //private string GetCodeOutput(ExecutionResult executionResult)
        //{
        //    StringBuilder sb = new StringBuilder();

        //    foreach (string result in executionResult.OutputResults)
        //    {
        //        sb.Append(result);
        //    }

        //    return sb.ToString();
        //}

        //private string GetCodeHtmlOutput(ExecutionResult executionResult)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    foreach (Tuple<string, ConsoleColor> tuple in executionResult.OutputResultWithColorInformation)
        //    {
        //        string encoded = HttpUtility.HtmlEncode(tuple.Item1);
        //        sb.Append(string.Format("<font style=\"color:{0}\">{1}</font>", tuple.Item2.ToString(), encoded));
        //    }

        //    return sb.ToString();
        //}

        public void SendMessageToIOPub(Message message, PublisherSocket ioPub, ExecutionState statusValue)
        {
            var content = new Dictionary<string, ExecutionState>
            {
                { "execution_state", statusValue }
            };

            Message ioPubMessage = new Message( MessageType.Status,
                                                JsonConvert.SerializeObject(content), 
                                                message.Header);

            this.logger.LogInformation(string.Format("Sending message to IOPub {0}", JsonConvert.SerializeObject(ioPubMessage)));
            this.messageSender.Send(ioPubMessage, ioPub);
            this.logger.LogInformation("Message Sent");
        }

        public void SendOutputMessageToIOPub(Message message, PublisherSocket ioPub, DisplayData data)
        {
            Dictionary<string, object> content = new Dictionary<string, object>
            {
                { "execution_count", this.executionCount },
                { "data", data.Data },
                { "metadata", data.MetaData }
            };

            Message outputMessage = new Message(MessageType.ExecuteResult, JsonConvert.SerializeObject(content, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), message.Header);

            this.logger.LogInformation(string.Format("Sending message to IOPub {0}", JsonConvert.SerializeObject(outputMessage)));
            this.messageSender.Send(outputMessage, ioPub);
        }

        public void SendInputMessageToIOPub(Message message, PublisherSocket ioPub, string code)
        {
            Dictionary<string, object> content = new Dictionary<string, object>
            {
                { "execution_count", 1 },
                { "code", code }
            };

            Message executeInputMessage = new Message(MessageType.Input, JsonConvert.SerializeObject(content),
                message.Header);

            this.logger.LogInformation(string.Format("Sending message to IOPub {0}", JsonConvert.SerializeObject(executeInputMessage)));
            this.messageSender.Send(executeInputMessage, ioPub);
        }

        public void SendExecuteReplyMessage(Message message, RouterSocket shellSocket)
        {
            var executeReply = new ExecuteReplyOk()
            {
                ExecutionCount = this.executionCount,
                Payload = new List<Dictionary<string, string>>(),
                UserExpressions = new Dictionary<string, string>()
            };

            Message executeReplyMessage = new Message(  MessageType.ExecuteReply,
                                                        JsonConvert.SerializeObject(executeReply), 
                                                        message.Header)
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
            var executeReply = new ExecuteReplyError()
            {
                ExecutionCount = executionCount,
                EName = error.Name,
                EValue = error.Message,
                Traceback = error.StackTrace
            };

            Message executeReplyMessage = new Message(  MessageType.ExecuteReply, 
                                                        JsonConvert.SerializeObject(executeReply), 
                                                        message.Header)
            {
                // Stick the original identifiers on the message so they'll be sent first
                // Necessary since the shell socket is a ROUTER socket
                Identifiers = message.Identifiers
            };

            this.logger.LogInformation(string.Format("Sending message to Shell {0}", JsonConvert.SerializeObject(executeReplyMessage)));
            this.messageSender.Send(executeReplyMessage, shellSocket);
        }

        private void SendErrorToIOPub(Message message, PublisherSocket ioPub, IErrorResult error)
        {
            var executeReply = new ExecuteReplyError()
            {
                ExecutionCount = executionCount,
                EName = error.Name,
                EValue = error.Message,
                Traceback = error.StackTrace
            };
            Message executeReplyMessage = new Message(  MessageType.Error, 
                                                        JsonConvert.SerializeObject(executeReply), 
                                                        message.Header)
            {
                Identifiers = message.Identifiers
            };
            this.messageSender.Send(executeReplyMessage, ioPub);
            this.logger.LogInformation(string.Format("Sending message to IOPub {0}", JsonConvert.SerializeObject(executeReplyMessage)));

            var errorMessage = new StderrMessage()
            {
                Text = error.Message
            };

            Message stderrMessage = new Message(MessageType.Stream, 
                                                JsonConvert.SerializeObject(errorMessage), 
                                                message.Header)
            {
                Identifiers = message.Identifiers
            };
            
            this.messageSender.Send(stderrMessage, ioPub);
            this.logger.LogInformation(string.Format("Sending message to IOPub {0}", JsonConvert.SerializeObject(stderrMessage)));
        }
    }
}
