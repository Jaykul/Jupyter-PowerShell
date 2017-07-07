namespace Jupyter
{
    using Jupyter.Messages;
    using Jupyter.Server;
    using Microsoft.Extensions.Logging;
    using NetMQ.Sockets;
    using System.Collections.Generic;

    public class Session : IKernelSocketProvider
    {
        private ILogger _logger;
        private ConnectionInformation _connection;
        private IReplEngine _engine;
        private Validator _validator;
        private Shell _shell;
        private Heartbeat _heartbeat;
        private Dictionary<MessageType, IMessageHandler> _messageHandlers;

        public Session(ConnectionInformation connection, IReplEngine engine, ILogger logger)
        {
            _connection = connection;
            _logger     = logger;
            _validator  = new Validator(_logger, connection.Key, connection.SignatureScheme);
            MessageSender.Validator = _validator;
            MessageSender.Logger = _logger;

            _engine = engine;

            InitializeMessageHandlers();

            _heartbeat  = new Heartbeat(_logger, GetAddress(connection.HBPort));
            _shell      = new Shell(_logger, GetAddress(connection.ShellPort), GetAddress(connection.IOPubPort), _validator, MessageHandlers);

            _heartbeat.Start();
            _shell.Start();
        }

        public RouterSocket ShellSocket { get => _shell.ShellSocket; }
        public PublisherSocket PublishSocket { get => _shell.PublishSocket; }
        
        public void Wait()
        {
            _shell.GetWaitEvent().Wait();
            _heartbeat.GetWaitEvent().Wait();
        }


        private Dictionary<MessageType, IMessageHandler> MessageHandlers => this._messageHandlers;

        private void InitializeMessageHandlers()
        {
            this._messageHandlers = new Dictionary<MessageType, IMessageHandler>
            {
                { MessageType.KernelInfoRequest, new KernelInfoHandler(_logger) },
                { MessageType.ExecuteRequest, new ExecuteRequestHandler(_logger, _engine) },
                { MessageType.ShutDownRequest, new ShutdownHandler(_logger, _heartbeat, _shell) }
            };
            // this._messageHandlers.Add(MessageTypeValues.CompleteRequest, new CompleteRequestHandler());
        }

        private string GetAddress(int port)
        {
            string address = string.Format("{0}://{1}:{2}", _connection.Transport, _connection.IP, port);
            _logger.LogDebug(address);
            return address;
        }
    }
}
