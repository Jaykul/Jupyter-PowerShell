using Jupyter.Messages;
using System;
using System.Collections.Generic;

namespace Jupyter.Server
{
    public interface IExecutionResult
    {
        List<Object> Output { get; }

        List<Exception> Exceptions { get; }

        DisplayData GetDisplayData();
    }
}