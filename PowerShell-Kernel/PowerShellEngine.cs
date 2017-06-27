using Jupyter.Server;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections;
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

                if (pipeline.Error.Count > 0)
                {
                    

                    foreach (object error in pipeline.Error.ReadToEnd())
                    {
                        var pso = error as PSObject;
                        if (pso == null)
                        {
                            continue;
                        }
                        ErrorRecord errorRecord = pso.BaseObject as ErrorRecord;
                        Exception ex;

                        if (errorRecord != null)
                        {
                            if (result.Error == null)
                            {
                                result.Error = new ErrorResult()
                                {
                                    Name = errorRecord.FullyQualifiedErrorId,
                                    Message = string.Format(
                                                "{0} : {1}\n{2}\n    + CategoryInfo          : {3}\n    + FullyQualifiedErrorId : {4}",
                                                errorRecord.InvocationInfo.InvocationName,
                                                errorRecord.ToString(),
                                                errorRecord.InvocationInfo.PositionMessage,
                                                errorRecord.CategoryInfo,
                                                errorRecord.FullyQualifiedErrorId),
                                    StackTrace = errorRecord.ScriptStackTrace.Split(new []{ "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList()
                                };
                            }
                            ex = errorRecord.Exception;
                        }
                        else
                        {
                            ex = pso.BaseObject as Exception;
                        }

                        if(ex != null)
                        {
                            result.Exceptions.Add(ex);
                        }
                    }
                }

                if (output.Count > 0)
                {
                    result.Output.AddRange(output.Select(o => o.BaseObject));
                    pipeline = Runspace.CreatePipeline();
                    var formatter = new Command("Out-String");
                    pipeline.Commands.Add(formatter);

                    result.OutputString = string.Join("\n", pipeline.Invoke(output).Select(line => line.ToString())).Trim();

                    //pipeline = Runspace.CreatePipeline();
                    //formatter = new Command("ConvertTo-Json");
                    //pipeline.Commands.Add(formatter);
                    //result.OutputJson = string.Join("\n", pipeline.Invoke(JsonWrapper.Wrap(script, output)).Select(line => line.ToString().Replace("\r\n","\n")));

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


            }
            catch (Exception ex)
            {
                _logger.LogError( "PowerShell Exception in ExecuteRequest {0}:\r\n{1}\r\n{2}", script, ex.Message, ex.StackTrace);
                result.Exceptions.Add(ex);
            }

            return result;
        }
    }
}
