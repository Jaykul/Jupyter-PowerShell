using Jupyter.Server;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;

namespace Jupyter.PowerShell
{
    public class PowerShellEngine : IReplEngine
    {
        private readonly ILogger _logger;

        /// <summary>
        /// The shared initial session state (we'll preload modules, etc).
        /// </summary>
        public InitialSessionState Iss { get; private set; }

        public Runspace Runspace { get; private set; }

        public PowerShellEngine(ILogger logger)
        {
            _logger = logger;
            Iss = InitialSessionState.CreateDefault();

            // We may want to use a runspace pool? ps.RunspacePool = rsp;
            Runspace = RunspaceFactory.CreateRunspace(Iss);
            Runspace.Open();

            //// We moved ModularInputsModule.dll into a subdirectory
            //// So we need to use GetEntryAssembly to use the path of PowerShell.exe
            //Assembly mps = Assembly.GetEntryAssembly(); // .GetExecutingAssembly();
            //string path = Path.GetDirectoryName(mps.Location);
            //if (!string.IsNullOrEmpty(path))
            //{
            //    // because this must work with PowerShell2, we can't use ImportPSModulesFromPath
            //    path = Path.Combine(path, "Modules");
            //    if (!Directory.Exists(path))
            //    {
            //        var logger = new ConsoleLogger();
            //        logger.WriteLog(LogLevel.Warn, "The Modules Path '{0}' could not be found", path);
            //    }
            //    else
            //    {
            //        iss.ImportPSModule(Directory.GetDirectories(path));
            //    }
            //}
            //return iss.LoadCmdlets(mps);
        }

        /// <summary>
        /// Adds read only variables to the shared initial session state.
        /// </summary>
        /// <param name="values">A collection of string tuples containing the name, value, and description of the variables to be added.</param>
        public void AddReadOnlyVariables(IEnumerable<Tuple<string, string, string>> values)
        {
            foreach (var variable in values)
            {
                Iss.Variables.Add(
                    new SessionStateVariableEntry(
                        variable.Item1,
                        variable.Item2,
                        variable.Item3,
                        ScopedItemOptions.Constant & ScopedItemOptions.ReadOnly));
            }
        }


        public IExecutionResult Execute(string script)
        {
            var result = new ExecutionResult();
            try
            {
                var pipeline = Runspace.CreatePipeline();
                pipeline.Commands.AddScript(script);

                var output = pipeline.Invoke();
                if (output.Count > 0)
                {
                    result.Output.AddRange(output.Select(o => o.BaseObject));
                    pipeline = Runspace.CreatePipeline();
                    var formatter = new Command("Out-String");
                    pipeline.Commands.Add(formatter);

                    result.OutputString = string.Join("\n   \n", pipeline.Invoke(output).Select(line => line.ToString())).Trim();

                    pipeline = Runspace.CreatePipeline();
                    formatter = new Command("ConvertTo-Json");
                    pipeline.Commands.Add(formatter);
                    result.OutputJson = string.Join("\n   \n", pipeline.Invoke(output).Select(line => line.ToString()));

                    // Users need to output their own HTML, ConvertTo-Html is *way* too flawed. 
                    // BUGBUG: need a better way to detect html?
                    if (output.First().BaseObject is string && result.OutputString.StartsWith("<") && result.OutputString.EndsWith(">"))
                    {
                        result.OutputHtml = result.OutputString;
                    }
                }
                // result.OutputJson = JsonConvert.SerializeObject(output);

                //var teeCommand = new Command("Tee-Object");
                //teeCommand.Parameters.Add("-Variable", "Output");
                //pipeline.Commands.Add(teeCommand);

                //var htmlCommand = new Command("ConvertTo-Html");
                //htmlCommand.Parameters.Add("-Fragment");
                //pipeline.Commands.Add(htmlCommand);

                if (pipeline.Error.Count > 0)
                {
                    foreach (ErrorRecord error in pipeline.Error.ReadToEnd())
                    {
                        result.Errors.Add(error);

                        var details = error.ErrorDetails != null ? error.ErrorDetails.Message : error.Exception.Message;

                        _logger.LogError(
                            "SCRIPT=\"{1}\"\nCATEGORY=\"{2}\"\nTargetName=\"{3}\"\nTargetType=\"{4}\"\nActivity=\"{5}\"\nReason=\"{6}\"\nDetails=\"{7}\"\n",
                            script,
                            error.CategoryInfo.Category,
                            error.CategoryInfo.TargetName,
                            error.CategoryInfo.TargetType,
                            error.CategoryInfo.Activity,
                            error.CategoryInfo.Reason,
                            details);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError( "PowerShell Exception in ExecuteRequest {0}:\r\n{1}\r\n{2}", script, ex.Message, ex.StackTrace);
                result.Errors.Add(new ErrorRecord(ex, "UninvokeableScript", ErrorCategory.SyntaxError, script));
            }

            return result;
        }
    }
}
