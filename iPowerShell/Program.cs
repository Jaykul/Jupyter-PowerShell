using Microsoft.Extensions.Logging;
using Jupyter.Messages;
using System;

namespace Jupyter.PowerShell
{
    class Program
    {
        private static ILogger logger;

        static void Main(string[] args)
        {
            var loggerFactory = new LoggerFactory()
                                     .AddConsole()
                                     .AddDebug();
            
            logger = loggerFactory.CreateLogger<PowerShellEngine>();

            PrintAllArgs(args);
            if (args.Length <= 0)
            {
                Console.WriteLine("Requires path to Connection file.");
                return;
            }

            ConnectionInformation connectionInformation = ConnectionInformation.FromFile(args[0]);
            var engine = new PowerShellEngine();
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