using Microsoft.Extensions.Logging;
using Jupyter.Messages;
using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.IO;
using Jupyter.Config;

namespace Jupyter.PowerShell
{
    public class Kernel
    {
        private static ILogger logger;
        private static IConfigurationRoot configuration;

        public static void Main(string[] args)
        {
            var cwd = Directory.GetCurrentDirectory();
            var loggerFactory = new LoggerFactory();
            var configBuilder = new ConfigurationBuilder()
                                    .SetBasePath(cwd)
                                    .AddJsonFile("Config.json", true);

            configuration = configBuilder.Build();

            var loggerOptions = new LoggerOptions();
            configuration.GetSection("Logger").Bind(loggerOptions);


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

            var powershellOptions = new PowerShellOptions();
            configuration.GetSection("PowerShell").Bind(powershellOptions);

            var engine = new PowerShellEngine(powershellOptions, logger);
            Session connection = new Session(connectionInformation, engine, logger);

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