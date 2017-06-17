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

        private readonly IReplEngine replEngine;

        private readonly MessageSender messageSender;

        private int executionCount = 1;

        public ExecuteRequestHandler(ILogger logger, IReplEngine replEngine, MessageSender messageSender)
        {
            this.logger = logger;
            this.replEngine = replEngine;
            this.messageSender = messageSender;
        }

        public void HandleMessage(Message message, RouterSocket serverSocket, PublisherSocket ioPub)
        {
            this.logger.LogDebug(string.Format("Message Content {0}", message.Content));
            ExecuteRequest executeRequest = JsonConvert.DeserializeObject<ExecuteRequest>(message.Content);

            this.logger.LogInformation(string.Format("Execute Request received with code {0}", executeRequest.Code));

            // 1: Send Busy status on IOPub
            this.SendMessageToIOPub(message, ioPub, StatusValues.Busy);

            // 2: Send execute input on IOPub
            this.SendInputMessageToIOPub(message, ioPub, executeRequest.Code);

            // 3: Evaluate the C# code
            string code = executeRequest.Code;
            var results = this.replEngine.Execute(code);

            DisplayData displayData = results.GetDisplayData();

            // 4: Send execute reply to shell socket
            this.SendExecuteReplyMessage(message, serverSocket);

            // 5: Send execute result message to IOPub
            if (results.HasOutput)
            {
                this.SendOutputMessageToIOPub(message, ioPub, displayData);
            }

            // 6: Send IDLE status message to IOPub
            this.SendMessageToIOPub(message, ioPub, StatusValues.Idle);

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

        public void SendMessageToIOPub(Message message, PublisherSocket ioPub, string statusValue)
        {
            Dictionary<string, string> content = new Dictionary<string, string>();
            content.Add("execution_state", statusValue);
            Message ioPubMessage = new Message(MessageTypeValues.Status,
                JsonConvert.SerializeObject(content), message.Header);

            this.logger.LogInformation(string.Format("Sending message to IOPub {0}", JsonConvert.SerializeObject(ioPubMessage)));
            this.messageSender.Send(ioPubMessage, ioPub);
            this.logger.LogInformation("Message Sent");
        }

        public void SendOutputMessageToIOPub(Message message, PublisherSocket ioPub, DisplayData data)
        {
            Dictionary<string, object> content = new Dictionary<string, object>();
            content.Add("execution_count", this.executionCount);
            content.Add("data", data.Data);
            content.Add("metadata", data.MetaData);

            Message outputMessage = new Message(MessageTypeValues.ExecuteResult,
                JsonConvert.SerializeObject(content), message.Header);

            this.logger.LogInformation(string.Format("Sending message to IOPub {0}", JsonConvert.SerializeObject(outputMessage)));
            this.messageSender.Send(outputMessage, ioPub);
        }

        public void SendInputMessageToIOPub(Message message, PublisherSocket ioPub, string code)
        {
            Dictionary<string, object> content = new Dictionary<string, object>();
            content.Add("execution_count", 1);
            content.Add("code", code);

            Message executeInputMessage = new Message(MessageTypeValues.Input, JsonConvert.SerializeObject(content),
                message.Header);

            this.logger.LogInformation(string.Format("Sending message to IOPub {0}", JsonConvert.SerializeObject(executeInputMessage)));
            this.messageSender.Send(executeInputMessage, ioPub);
        }

        public void SendExecuteReplyMessage(Message message, RouterSocket shellSocket)
        {
            ExecuteReplyOk executeReply = new ExecuteReplyOk()
            {
                ExecutionCount = this.executionCount,
                Payload = new List<Dictionary<string, string>>(),
                UserExpressions = new Dictionary<string, string>()
            };

            Message executeReplyMessage = new Message(MessageTypeValues.ExecuteReply,
                JsonConvert.SerializeObject(executeReply), message.Header);

            // Stick the original identifiers on the message so they'll be sent first
            // Necessary since the shell socket is a ROUTER socket
            executeReplyMessage.Identifiers = message.Identifiers;

            this.logger.LogInformation(string.Format("Sending message to Shell {0}", JsonConvert.SerializeObject(executeReplyMessage)));
            this.messageSender.Send(executeReplyMessage, shellSocket);
        }
    }
}
