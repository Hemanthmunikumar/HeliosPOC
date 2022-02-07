using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Helios
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
           var _configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true).Build();
            int.TryParse(_configuration.GetSection("HeliosConfig")["JobScheduleInterval"], out int jobInterval);
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                TrainedPills.TrainedPillsProcess(_serviceScopeFactory);
                await Task.Delay(jobInterval, stoppingToken); //TODO change the trigger interval //20000=20sec
            }
        }
    }
}
