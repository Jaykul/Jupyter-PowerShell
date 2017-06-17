namespace Jupyter.Server
{
    using NetMQ;
    using NetMQ.Sockets;
    using System.Threading;
    using Microsoft.Extensions.Logging;

    public class Heartbeat
    {
        private ILogger _logger;
        private string _address;
        private ResponseSocket _socket;
        private ManualResetEventSlim _stopEvent;
        private Thread _thread;
        private bool _disposed;

        public Heartbeat(ILogger logger, string address)
        {
            _logger = logger;
            _address = address;

            _socket = new ResponseSocket();
            _stopEvent = new ManualResetEventSlim();
        }

        public void Start()
        {
            _thread = new Thread(StartServerLoop);
            _thread.Start();
            //ThreadPool.QueueUserWorkItem(new WaitCallback(StartServerLoop));
        }

        public void Stop()
        {
            _stopEvent.Set();
        }

        public ManualResetEventSlim GetWaitEvent()
        {
            return _stopEvent;
        }

        private void StartServerLoop(object state)
        {
            _socket.Bind(_address);

            while (!_stopEvent.Wait(0))
            {
                byte[] data = _socket.ReceiveFrameBytes();

                _logger.LogInformation(System.Text.Encoding.Default.GetString(data));
                // Echoing back whatever was received
                _socket.TrySendFrame(data);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected void Dispose(bool dispose)
        {
            if (!_disposed)
            {
                if (dispose)
                {
                    if (_socket != null)
                    {
                        _socket.Dispose();
                    }

                    _disposed = true;
                }
            }
        }
    }
}
