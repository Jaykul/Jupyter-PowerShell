namespace Jupyter.Server
{
    public interface IReplEngine
    {
        IExecutionResult Execute(string script);
    }
}