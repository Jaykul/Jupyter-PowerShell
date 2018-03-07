using Jupyter.Server;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;

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
            Iss = InitialSessionState.CreateDefault2();

            // Preload all cmdlets from this assembly
            Assembly core = Assembly.GetExecutingAssembly();
            Iss.LoadCmdlets(core);

            // FOR CORE:
            // Fix the PSModulePath, because now we're a full-blown host and ship with our own modules
            // TODO: What should this default be?
            var oldPath = Environment.GetEnvironmentVariable("PSModulePath", EnvironmentVariableTarget.Process) ?? "";
            var localModules = Path.Combine(Path.GetDirectoryName(core.Location), "Modules");
            var newPath = string.Join(Path.PathSeparator, oldPath.Split(Path.PathSeparator).Append(localModules).Distinct());
            Environment.SetEnvironmentVariable("PSModulePath", newPath, EnvironmentVariableTarget.Process);

            // We may want to use a runspace pool? ps.RunspacePool = rsp;
            Runspace = RunspaceFactory.CreateRunspace(Iss);
            Runspace.Open();
        }


        /// <summary>
        /// Adds read only variables to the shared initial session state.
        /// </summary>
        /// <param name="values">A collection of string tuples containing the name, value, and description of the variables to be added.</param>
        public void AddReadOnlyVariables(params (string Name, object Value)[] values)
        {
            var readOnly = ScopedItemOptions.Constant & ScopedItemOptions.ReadOnly;
            foreach (var v in values)
            {
                var variable = new PSVariable(v.Name, v.Value, readOnly);
                Runspace.SessionStateProxy.PSVariable.Set(variable);
            }
        }


        public IExecutionResult Execute(string script)
        {
            var result = new ExecutionResult(_options);
            Pipeline pipeline = null;
            IEnumerable<PSObject> output = null;
            try
            {
                pipeline = Runspace.CreatePipeline();
                pipeline.Commands.AddScript(script);
                output = pipeline.Invoke().Where(o => o != null);

                LogErrors(result, pipeline.Error);
            }
            catch (RuntimeException err)
            {
                if (result.Error == null)
                {
                    var errorRecord = err.ErrorRecord;
                    if (errorRecord != null)
                    {
                        result.Error = new ErrorResult()
                        {
                            Name = errorRecord.FullyQualifiedErrorId,
                            Message = string.Format(
                                        "{0} : {1}\r\n",
                                        errorRecord.InvocationInfo?.InvocationName,
                                        errorRecord.ToString()),
                            StackTrace = new List<string>(new[] {
                            errorRecord.InvocationInfo?.PositionMessage,
#if NETCORE
                            errorRecord.ScriptStackTrace,
#endif
                            "CategoryInfo          : " + errorRecord.CategoryInfo,
                            "FullyQualifiedErrorId : " + errorRecord.FullyQualifiedErrorId })
                        };
                    }

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
                                    "{0} : {1}",
                                    ex.Source,
                                    ex.Message),
                        StackTrace = new List<string>(ex.StackTrace.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
                    };
                }
                _logger.LogError("Unhandled PowerShell Exception in ExecuteRequest {0}:\r\n{1}\r\n{2}", script, ex.Message, ex.StackTrace);
                result.Exceptions.Add(ex);
            }

            CollectOutput(result, output);

            return result;
        }

        private void CollectOutput(ExecutionResult result, IEnumerable<PSObject> output)
        {
            if (output != null && output.Any())
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
                                    "{0} : {1}",
                                    errorRecord.InvocationInfo.InvocationName,
                                    errorRecord.ToString()).TrimEnd('\r','\n') + "\r\n",
                                StackTrace = new List<string>(new[] {
                                    errorRecord.InvocationInfo.PositionMessage,
#if NETCORE
                                    errorRecord.ScriptStackTrace,
#endif
                                    "CategoryInfo          : " + errorRecord.CategoryInfo,
                                    "FullyQualifiedErrorId : " + errorRecord.FullyQualifiedErrorId })
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
