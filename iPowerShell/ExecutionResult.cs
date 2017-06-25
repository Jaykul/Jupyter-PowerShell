using Jupyter.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace Jupyter.PowerShell
{
    public class ExecutionResult : Server.IExecutionResult
    {
        public List<Object> Output { get; } = new List<object>();
        internal List<ErrorRecord> Errors { get; } = new List<ErrorRecord>();
        public List<Exception> Exceptions { get { return Errors.Select(er => er.Exception).ToList(); } }

        public string OutputString { get; set; }

        public string OutputHtml { get; set; }

        public string OutputJson { get; set; }

        public DisplayData GetDisplayData()
        {
            return new DisplayData()
            {
                Data = new Dictionary<string, object>()
                {
                    {"text/plain", OutputString},
                    {"text/html", OutputHtml},
                    //{"application/json", OutputJson}
                }
            };
        }
    }
}
