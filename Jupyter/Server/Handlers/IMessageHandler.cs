namespace Jupyter.Server.Handlers
{
    using Jupyter.Messages;
    using NetMQ.Sockets;

    public interface IMessageHandler
    {
        void HandleMessage(Message message, RouterSocket serverSocket, PublisherSocket ioPub);
    }
}
