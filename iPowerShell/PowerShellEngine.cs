using Jupyter.Server;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jupyter.PowerShell
{
    public class PowerShellEngine : IReplEngine
    {
        public IExecutionResult Execute(string script)
        {
            return new ExecutionResult()
            {
                // TODO: Don't just echo ... 
                OutputString = script
            };
        }
    }
}
