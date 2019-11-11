using Microsoft.Extensions.Logging;
using Jupyter.Messages;
using System;
using System.Linq;
// using Microsoft.Extensions.Configuration;
using System.IO;
using Jupyter.Config;
using System.Reflection;
//using Autofac;
using NLog.Extensions.Logging;
using NLog;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace Jupyter.PowerShell
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args)
                .ConfigureLogging(logging => logging.AddNLog())
                .ConfigureServices(ConfigureServices)
                .Build()
                .RunAsync();
        }
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder().ConfigureAppConfiguration(config =>
            {
                // to suppress the default sources
                // config.Sources.Clear();

                // To support the normal Jupyter use, we absolutely REQUIRE a connection file
                if (args.Length >= 1 && File.Exists(args[0]))
                {
                    config.AddInMemoryCollection(
                        new Dictionary<string, string>
                        {
                            { "Jupyter:ConnectionFile", args[0] }
                        });
                    config.AddCommandLine(args.Skip(1).ToArray());
                }
                config.AddJsonFile("PowerShell-Kernel.Config.json");
                config.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "PowerShell-Kernel.Config.json"), optional: true);
            });
        }


        private static void ConfigureServices(HostBuilderContext builder, IServiceCollection services)
        {
            // The debugger hack, for now
            if (builder.Configuration.GetSection("Debug").GetValue<bool>("BreakOnStart"))
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
            services.Configure<PowerShellOptions>("PowerShell", builder.Configuration);
            services.Configure<LoggerOptions>("Logger", builder.Configuration);



            ConnectionInformation connectionInformation = ConnectionInformation.FromFile(args[0]);

            services.AddHostedService<Kernel>();
        }
    }


    public class Kernel : IHostedService
    {
        private readonly ILogger<Kernel> logger;
        private readonly IHostApplicationLifetime appLifetime;

        public Kernel(ILogger<Kernel> logger, IConfigureOptions<PowerShellOptions> configuration, IHostApplicationLifetime appLifetime)
        {
            this.logger = logger;
            this.appLifetime = appLifetime;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            appLifetime.ApplicationStarted.Register(OnStarted);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void OnStarted()
        {
            logger.LogInformation("OnStarted");

            var options = new PowerShellOptions();
            configuration.GetSection("PowerShell").Bind(options);

            var engine = new PowerShellEngine(options, logger);
            Session connection = new Session(connectionInformation, engine, logger);
            engine.AddReadOnlyVariables(("JupyterSession", connection));
            connection.Wait();

            System.Threading.Thread.Sleep(60000);
        }



    }
}