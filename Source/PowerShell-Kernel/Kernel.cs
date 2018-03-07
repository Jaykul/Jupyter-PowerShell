using Microsoft.Extensions.Logging;
using Jupyter.Messages;
using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.IO;
using Jupyter.Config;
using System.Reflection;

namespace Jupyter.PowerShell
{
    public class Kernel
    {
        private static ILogger logger;
        private static IConfigurationRoot configuration;

        public static void Main(string[] args)
        {
            var loggerFactory = new LoggerFactory();

            var installPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var baseConfig = Path.Combine(installPath, "PowerShell-Kernel.Config.json");
            var cwdConfig = Path.Combine(Directory.GetCurrentDirectory(), "PowerShell-Kernel.Config.json");

            var configBuilder = new ConfigurationBuilder()
                                    .AddJsonFile(baseConfig, true)
                                    .AddJsonFile(cwdConfig, true);

            configuration = configBuilder.Build();

            var loggerOptions = new LoggerOptions();
            configuration.GetSection("Logger").Bind(loggerOptions);

            if (configuration.GetSection("Debug").GetValue<bool>("BreakOnStart"))
            {
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    System.Diagnostics.Debugger.Break();
                }
                else
                {
                    System.Diagnostics.Debugger.Launch();
                }
            }

            if (loggerOptions.ConsoleOutput)
            {
                loggerFactory.AddConsole();
            }

            if(loggerOptions.DebuggerOutput)
            {
                loggerFactory.AddDebug();
            }
            
            logger = loggerFactory.CreateLogger<PowerShellEngine>();

            PrintAllArgs(args);
            if (args.Length <= 0)
            {
                Console.WriteLine("Requires path to Connection file.");
                return;
            }

            ConnectionInformation connectionInformation = ConnectionInformation.FromFile(args[0]);

            var options = new PowerShellOptions();
            configuration.GetSection("PowerShell").Bind(options);

            var engine = new PowerShellEngine(options, logger);
            Session connection = new Session(connectionInformation, engine, logger);
            engine.AddReadOnlyVariables(("JupyterSession", connection));
            connection.Wait();

            System.Threading.Thread.Sleep(60000);
        }

        private static void PrintAllArgs(string[] args)
        {            
            logger.LogDebug("PowerShell Jupyter Kernel Args: ");
            foreach (string s in args)
            {
                logger.LogDebug(s);
            }
        }
    }
}