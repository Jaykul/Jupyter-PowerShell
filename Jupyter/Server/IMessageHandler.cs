namespace Jupyter.Server
{
    using Jupyter.Messages;
    using NetMQ.Sockets;

    public interface IMessageHandler
    {
        void HandleMessage(Message message, RouterSocket serverSocket, PublisherSocket ioPub);
    }
}
