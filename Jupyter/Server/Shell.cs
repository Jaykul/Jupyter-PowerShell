namespace Jupyter.Server
{

    using System.Collections.Generic;
    using System.Text;
    using System.Linq;
    using System.Threading;
    using Microsoft.Extensions.Logging;
    using NetMQ;
    using NetMQ.Sockets;
    using Jupyter.Messages;
    using Newtonsoft.Json;

    public class Shell 
    {
        private ILogger logger;
        private string addressShell;
        private string addressIOPub;

        private Validator signatureValidator;
        private RouterSocket server;
        private PublisherSocket ioPubSocket;

        private ManualResetEventSlim stopEvent;

        private Thread thread;
        private bool disposed;

        private Dictionary<MessageType, IMessageHandler> messageHandlers;

        public Shell(ILogger logger,
                     string addressShell,
                     string addressIOPub,
                     Validator signatureValidator,
                     Dictionary<MessageType, IMessageHandler> messageHandlers)
        {
            this.logger = logger;
            this.addressShell = addressShell;
            this.addressIOPub = addressIOPub;
            this.signatureValidator = signatureValidator;
            this.messageHandlers = messageHandlers;

            this.server = new RouterSocket();
            this.ioPubSocket = new PublisherSocket();
            this.stopEvent = new ManualResetEventSlim();
        }

        public void Start()
        {
            this.thread = new Thread(this.StartServerLoop);
            this.thread.Start();

            this.logger.LogInformation("Shell Started");
            //ThreadPool.QueueUserWorkItem(new WaitCallback(StartServerLoop));
        }

        private void StartServerLoop(object state)
        {
            this.server.Bind(this.addressShell);
            this.logger.LogInformation(string.Format("Bound the Shell server to address {0}", this.addressShell));

            this.ioPubSocket.Bind(this.addressIOPub);
            this.logger.LogInformation(string.Format("Bound IOPub to address {0}", this.addressIOPub));

            while (!this.stopEvent.Wait(0))
            {
                Message message = this.GetMessage();

                this.logger.LogInformation(JsonConvert.SerializeObject(message));

                if (this.messageHandlers.TryGetValue(message.Header.MessageType, out IMessageHandler handler))
                {
                    this.logger.LogInformation(string.Format("Sending message to handler {0}", message.Header.MessageType));
                    handler.HandleMessage(message, this.server, this.ioPubSocket);
                    this.logger.LogInformation("Message handling complete");
                }
                else
                {
                    this.logger.LogWarning(string.Format("No message handler found for message type {0}",
                                                    message.Header.MessageType));
                }
            }
        }

        private Message GetMessage()
        {
            Message message = new Message();

            // There may be additional ZMQ identities attached; read until the delimiter <IDS|MSG>"
            // and store them in message.identifiers
            // http://ipython.org/ipython-doc/dev/development/messaging.html#the-wire-protocol
            byte[] delimAsBytes = Encoding.ASCII.GetBytes(Constants.DELIMITER);
            byte[] delim;
            while (true)
            {
                delim = this.server.ReceiveFrameBytes();
                if (delim.SequenceEqual(delimAsBytes)) break;

                message.Identifiers.Add(delim);
            }

            // Getting Hmac
            message.HMac = this.server.ReceiveFrameString();
            this.logger.LogInformation(message.HMac);

            // Getting Header
            string header = this.server.ReceiveFrameString();
            this.logger.LogInformation(header);

            message.Header = JsonConvert.DeserializeObject<Header>(header);

            // Getting parent header
            string parentHeader = this.server.ReceiveFrameString();
            this.logger.LogInformation(parentHeader);

            message.ParentHeader = JsonConvert.DeserializeObject<Header>(parentHeader);

            // Getting metadata
            string metadata = this.server.ReceiveFrameString();
            this.logger.LogInformation(metadata);

            message.MetaData = JsonConvert.DeserializeObject<Dictionary<string, object>>(metadata);

            // Getting content
            string content = this.server.ReceiveFrameString();
            this.logger.LogInformation(content);

            message.Content = content;

            return message;
        }


        public void Stop()
        {
            this.stopEvent.Set();
        }

        public ManualResetEventSlim GetWaitEvent()
        {
            return this.stopEvent;
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected void Dispose(bool dispose)
        {
            if (!this.disposed)
            {
                if (dispose)
                {
                    if (this.server != null)
                    {
                        this.server.Dispose();
                    }

                    if (this.ioPubSocket != null)
                    {
                        this.ioPubSocket.Dispose();
                    }

                    this.disposed = true;
                }
            }
        }
    }
}
