using Jupyter.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jupyter.PowerShell
{
    public class ExecutionResult : Server.IExecutionResult
    {
        public bool HasOutput => !string.IsNullOrEmpty(OutputString);

        public string OutputString { get; set; }

        public DisplayData GetDisplayData()
        {
            return new DisplayData()
            {
                Data = new Dictionary<string, object>()
                {
                    {"text/plain", OutputString},
                    {"text/html", ToHtml()}
                }
            };
        }
        

        internal string ToHtml()
        {
            return string.Format("<div>{0}</div>", OutputString);
        }
    }
}
