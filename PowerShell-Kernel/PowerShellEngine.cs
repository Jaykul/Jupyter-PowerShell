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
        private readonly PowerShellOptions _options;

        /// <summary>
        /// The shared initial session state (we'll preload modules, etc).
        /// </summary>
        public InitialSessionState Iss { get; private set; }

        public Runspace Runspace { get; private set; }

        public PowerShellEngine(PowerShellOptions options, ILogger logger)
        {
            _options = options;
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
            var result = new ExecutionResult(_options);
            Pipeline pipeline = null;
            System.Collections.ObjectModel.Collection<PSObject> output = null;
            try
            {
                pipeline = Runspace.CreatePipeline();
                pipeline.Commands.AddScript(script);
                output = pipeline.Invoke();

                LogErrors(result, pipeline.Error);
            }
            catch (RuntimeException err)
            {
                if (result.Error == null)
                {
                    var errorRecord = err.ErrorRecord;
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
                        StackTrace = new List<string>(new[] { errorRecord.InvocationInfo.PositionMessage })
                        // PS Core Only?                                   StackTrace = errorRecord.ScriptStackTrace.Split(new []{ "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList()
                    };
                }
                _logger.LogError("PowerShell Exception in ExecuteRequest {0}:\r\n{1}\r\n{2}", script, err.Message, err.StackTrace);
                result.Exceptions.Add(err);
            }
            catch (Exception ex)
            {
                if (result.Error == null)
                {
                    result.Error = new ErrorResult()
                    {
                        Name = ex.GetType().FullName,
                        Message = string.Format(
                                    "{0} : {1}\n{2}",
                                    ex.Source,
                                    ex.Message,
                                    ex.TargetSite),
                        StackTrace = new List<string>(ex.StackTrace.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
                    };
                }
                _logger.LogError("Unhandled PowerShell Exception in ExecuteRequest {0}:\r\n{1}\r\n{2}", script, ex.Message, ex.StackTrace);
                result.Exceptions.Add(ex);
            }

            CollectOutput(result, output);

            return result;
        }

        private void CollectOutput(ExecutionResult result, System.Collections.ObjectModel.Collection<PSObject> output)
        {
            if (output?.Count > 0)
            {
                result.Output.AddRange(output.Select(o => o.BaseObject));
                try
                {
                    Pipeline pipeline = Runspace.CreatePipeline();
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
                catch (Exception ex)
                {
                    _logger.LogError("Unhandled PowerShell Exception in ExecuteRequest {0}:\r\n{1}\r\n{2}", "Out-String", ex.Message, ex.StackTrace);
                }
            }
        }

        private static void LogErrors(ExecutionResult result, PipelineReader<object> errorStream)
        {
            if (errorStream?.Count > 0)
            {
                foreach (object error in errorStream.ReadToEnd())
                {
                    var pso = error as PSObject;
                    if (pso == null)
                    {
                        continue;
                    }
                    ErrorRecord errorRecord = pso.BaseObject as ErrorRecord;
                    Exception exception;

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
                                StackTrace = new List<string>(new[] { errorRecord.InvocationInfo.PositionMessage })
                                // PS Core Only?                                   StackTrace = errorRecord.ScriptStackTrace.Split(new []{ "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList()
                            };
                        }
                        exception = errorRecord.Exception;
                    }
                    else
                    {
                        exception = pso.BaseObject as Exception;
                    }

                    if (exception != null)
                    {
                        result.Exceptions.Add(exception);
                    }
                }
            }
        }
    }
}
