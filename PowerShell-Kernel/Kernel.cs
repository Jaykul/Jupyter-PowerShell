using Microsoft.Extensions.Logging;
using Jupyter.Messages;
using System;
using System.Linq;

namespace Jupyter.PowerShell
{
    public class Kernel
    {
        private static ILogger logger;

        public static void Main(string[] args)
        {
            var loggerFactory = new LoggerFactory();

            if (args.Contains("-c"))
            {
                loggerFactory.AddConsole();
            }

            if(args.Contains("-d"))
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
            var engine = new PowerShellEngine(logger);
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