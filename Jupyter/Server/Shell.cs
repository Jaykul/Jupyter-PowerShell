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
                Message message = this.ReceiveMessage();

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

        private Message ReceiveMessage()
        {
            // There may be additional ZMQ identities attached; read until the delimiter <IDS|MSG>"
            // and store them in message.identifiers
            // http://ipython.org/ipython-doc/dev/development/messaging.html#the-wire-protocol
            byte[] delimiterBytes = Encoding.ASCII.GetBytes(Constants.DELIMITER);
            byte[] delimiter;
            var frames = new List<byte[]>();
            do
            {
                delimiter = server.ReceiveFrameBytes();
                frames.Add(delimiter);
            } while (!delimiter.SequenceEqual(delimiterBytes));
            // strip delimiter
            frames.RemoveAt(frames.Count - 1);

            // Getting Hmac
            var hmac = ReceiveFrame<string>();

            // Getting Header
            var header = ReceiveFrame<Header>();

            // Getting parent header
            var parent = ReceiveFrame<Header>();

            // Getting metadata
            var metaData = ReceiveFrame<Dictionary<string, object>>();

            // Getting content
            Content content = null;

            switch (header.MessageType)
            {
                case MessageType.ExecuteRequest:
                    content = ReceiveFrame<ExecuteRequestContent>();
                    break;
                case MessageType.CompleteRequest:
                    content = ReceiveFrame<CompleteRequestContent>();
                    break;
                case MessageType.ShutDownRequest:
                    content = ReceiveFrame<ShutdownContent>();
                    break;
                case MessageType.KernelInfoRequest:
                    content = ReceiveFrame<Content>();
                    break;
                //case MessageType.ExecuteInput:
                //case MessageType.ExecuteReply:
                //case MessageType.ExecuteResult:
                //case MessageType.CompleteReply:
                //case MessageType.ShutDownReply:
                //case MessageType.KernelInfoReply:

                //case MessageType.Status:
                //case MessageType.Output:
                //case MessageType.Input:
                //case MessageType.Error:
                //case MessageType.Stream:
                default:
                    logger.LogInformation(header.MessageType + " message not handled.");
                    content = ReceiveFrame<Content>();
                    break;
            }


            return new Message(header.MessageType, content, parent, frames, header, hmac, metaData);
        }

        private T ReceiveFrame<T>()
        {
            var frame = server.ReceiveFrameString();
            logger.LogInformation(frame);
            if(typeof(T) == typeof(string))
            {
                return new[] { frame }.Cast<T>().First();
            }
            return JsonConvert.DeserializeObject<T>(frame);
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
