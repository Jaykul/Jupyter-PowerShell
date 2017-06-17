using Jupyter.Messages;

namespace Jupyter.Server
{
    public interface IExecutionResult
    {
        bool HasOutput { get; }
        string OutputString { get; set; }

        DisplayData GetDisplayData();
    }
}