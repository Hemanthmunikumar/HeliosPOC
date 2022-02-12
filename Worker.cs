using Helios.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;

namespace Helios
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _config;
        private readonly IPouchTrainedPills _pouchTrainedPills;
        public Worker(ILogger<Worker> logger,  IConfiguration config, IPouchTrainedPills pouchTrainedPills)
        {
            _logger = logger;
            _config = config;
            _pouchTrainedPills = pouchTrainedPills;
        }
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int.TryParse(_config.GetSection("HeliosConfig")["JobScheduleInterval"], out int jobInterval);
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Pouch Pills process started at: {time}", DateTimeOffset.Now);
                await _pouchTrainedPills.PillsProcess();
                _logger.LogInformation("Pouch Pills process ended at: {time}", DateTimeOffset.Now);
                _logger.LogInformation("Next process after {interval}", jobInterval);
                // TrainedPills.TrainedPillsProcess(_serviceScopeFactory);
                await Task.Delay(jobInterval, stoppingToken); //TODO change the trigger interval //20000=20sec
            }
        }
    }
}
