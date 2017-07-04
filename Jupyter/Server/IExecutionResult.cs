using Jupyter.Messages;
using System;
using System.Collections.Generic;

namespace Jupyter.Server
{

    public interface IErrorResult
    {
        string Name { get; set; }
        string Message { get; set; }
        List<string> StackTrace { get; set; }

    }


    public interface IExecutionResult
    {
        List<Object> Output { get; }

        List<Exception> Exceptions { get; }

        IErrorResult Error { get; }

        DisplayDataContent GetDisplayData();
    }
}