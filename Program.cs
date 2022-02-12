using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Serilog.Events;
using Helios.Interfaces;
using Helios.BO;

namespace Helios
{
    public class Program
    {
        static int counter;
        static string connectionString;
        public static IConfiguration _configuration;
        static bool OnFileListener = true;
        public static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            BuildConfig(builder);

            // Specifying the configuration for serilog
            Log.Logger = new LoggerConfiguration() // initiate the logger configuration
                            .ReadFrom.Configuration(builder.Build()) // connect serilog to our configuration folder
                            .Enrich.FromLogContext() //Adds more information to our logs from built in Serilog 
                           // .WriteTo.Console() // decide where the logs are going to be shown
                           // .WriteTo.File("logs\\log.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 100, fileSizeLimitBytes: 100000)
                            .CreateLogger(); //initialise the logger
            //Note:Only the most recent 31 files are retained by default, you can override it by using retainedFileCountLimit
            //Only the most recent 31 files are retained by default, you can override it by using retainedFileCountLimit

            Log.Logger.Information("Application Starting");
            CreateHostBuilder(args).UseSerilog() // Add Serilog
                                   .Build().Run();

        }
        /// <summary>
        /// Handles cleanup operations when app is cancelled or unloads
        /// </summary>
        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
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
            //var logger = new LoggerConfiguration().WriteTo.RollingFile(
            //       outputTemplate: outputTemplate,
            //       restrictedToMinimumLevel: LogEventLevel.Information,
            //       pathFormat: Path.Combine(loggingDirectory, "systemlog-{Date}.text")
            //   .CreateLogger();
            var host = Host.CreateDefaultBuilder(args)
            //    .ConfigureLogging((hostingContext, builder) =>
            //{
            //    builder.AddFile("Logs/myapp-{Date}.txt");
            //})
            //.ConfigureLogging((hostContext, loggingBuilder) =>
            //loggingBuilder.AddSerilog(
            //    loggingBuilder
            //        .Services.BuildServiceProvider().GetRequiredService<ILogger>(),
            //    dispose: true))
            //.ConfigureLogging((context, builder) =>
            //{
            //    builder.AddConsole();
            //    builder.AddSerilog(logger);
            //    //....<- some other option here
            //})
            //.ConfigureLogging((hostContext, builder) =>
            //{
            //    Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(hostContext.Configuration).CreateLogger();
            //    builder.AddConfiguration(hostContext.Configuration.GetSection("Logging"));
            //    builder.AddSerilog(dispose: true);
            //})
               //.ConfigureAppConfiguration((hostContext, builder) =>
               //{
               //    builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
               //    builder.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true);
               // })
                .ConfigureServices((hostContext, services) =>
                {
                    //var settings = hostContext.Configuration.GetSection("Configuration").Get<Settings>();
                    //services.AddSingleton(settings);
                    services.AddTransient<IPouchTrainedPills, PouhTrainedPills>();
                    services.AddTransient<ICSVHelper, CSVHelper>();
                    services.AddTransient<IBlobHandler, BlobHandler>();
                    // services.AddLogging();
                    //services.AddOptions();
                    //var optionsBuilder = new DbContextOptionsBuilder<PostgreDbContext>();
                    //optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Username=postgres;Password=007;Database=poc");
                    //services.AddScoped<PostgreDbContext>(s => new PostgreDbContext(optionsBuilder.Options));
                    // services.AddSingleton<ILogger>(BuildLogger);
                    services.AddHostedService<Worker>();
           //         services.AddLogging(builder =>
           //    builder
           //        .AddDebug()
           //        .AddConsole()
           //        .AddConfiguration(configuration.GetSection("Logging"))
           //        .SetMinimumLevel(LogLevel.Information)
           //);
                });

            return host;
        }
    }
    
}
