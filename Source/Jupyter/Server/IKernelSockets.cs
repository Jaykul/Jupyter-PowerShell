using NetMQ.Sockets;

namespace Jupyter.Server
{
    public interface IKernelSocketProvider
    {
        RouterSocket ShellSocket { get; }
        PublisherSocket PublishSocket { get; }
    }
}