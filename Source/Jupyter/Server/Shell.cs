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
    using Jupyter.Server.Handlers;

    public class Shell : IKernelSocketProvider
    {
        private ILogger logger;
        private string addressShell;
        private string addressIOPub;

        private Validator signatureValidator;
        private RouterSocket serverSocket;
        private PublisherSocket ioPubSocket;

        private ManualResetEventSlim stopEvent;

        private Thread thread;
        private bool disposed;

        private Dictionary<MessageType, IMessageHandler> messageHandlers;

        public RouterSocket ShellSocket { get => serverSocket; }
        public PublisherSocket PublishSocket { get => ioPubSocket; }


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

            this.serverSocket = new RouterSocket();
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
            this.serverSocket.Bind(this.addressShell);
            this.logger.LogInformation(string.Format("Bound the Shell server to address {0}", this.addressShell));

            this.ioPubSocket.Bind(this.addressIOPub);
            this.logger.LogInformation(string.Format("Bound IOPub to address {0}", this.addressIOPub));

            while (!this.stopEvent.Wait(0))
            {
                if (serverSocket.ReceiveMessage() is Message message)
                {
                    this.logger.LogInformation(JsonConvert.SerializeObject(message));

                    if (this.messageHandlers.TryGetValue(message.Header.MessageType, out IMessageHandler handler))
                    {
                        this.logger.LogInformation(string.Format("Sending message to handler {0}", message.Header.MessageType));
                        handler.HandleMessage(message, this.serverSocket, this.ioPubSocket);
                        this.logger.LogInformation("Message handling complete");
                    }
                    else
                    {
                        this.logger.LogWarning(string.Format("No message handler found for message type {0}",
                                                        message.Header.MessageType));
                    }
                }
            }
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
                    if (this.serverSocket != null)
                    {
                        this.serverSocket.Dispose();
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
