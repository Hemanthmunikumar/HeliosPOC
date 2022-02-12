using Helios.BO;
using Helios.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.IO;

namespace Helios
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            BuildConfig(builder);

            // Specifying the configuration for serilog
            Log.Logger = new LoggerConfiguration() // initiate the logger configuration
                            .ReadFrom.Configuration(builder.Build()) // connect serilog to our configuration folder
                            .Enrich.FromLogContext() //Adds more information to our logs from built in Serilog 
                            .CreateLogger(); //initialise the logger

            Log.Logger.Information("Application Starting");
            CreateHostBuilder(args).UseSerilog() // Add Serilog
                                   .Build().Run();

        }

        static void BuildConfig(IConfigurationBuilder builder)
        {
            // Check the current directory that the application is running on 
            // Then once the file 'appsetting.json' is found, we are adding it.
            // We add env variables, which can override the configs in appsettings.json
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
        }
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddTransient<IPouchTrainedPills, PouhTrainedPills>();
                    services.AddTransient<ICSVHelper, CSVHelper>();
                    services.AddTransient<IBlobHandler, BlobHandler>();
                    services.AddHostedService<Worker>();
                });

            return host;
        }
    }

}
