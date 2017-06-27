using Jupyter.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace Jupyter.PowerShell
{
    public class ErrorResult : Server.IErrorResult
    {
        public string Name { get; set; }
        public string Message { get; set; }
        public List<string> StackTrace { get; set; }
    }

    public class ExecutionResult : Server.IExecutionResult
    {
        public List<Object> Output { get; } = new List<object>();

        public List<Exception> Exceptions { get; } = new List<Exception>();

        public Server.IErrorResult Error { get; set; }

        public string OutputString { get; set; }

        public string OutputHtml { get; set; }

        public DisplayData GetDisplayData()
        {
            var data = new Dictionary<string, object>()
            {
                {"text/plain", OutputString}
            };

            if(!string.IsNullOrEmpty(OutputHtml))
            {
                data.Add("text/html", OutputHtml);
            }
            if (Output.Count > 0)
            {
                data.Add("application/json", new { output = Output });
            }

            return new DisplayData()
            {
                Data = data
            };
        }
    }
}
