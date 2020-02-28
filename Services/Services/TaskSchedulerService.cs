using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Services.Models;
using Microsoft.Extensions.Logging;
using Services.Workers;
using Services.Services.TaskQueue;

namespace Services.Services
{
    public class TaskSchedulerService : IHostedService, IDisposable
    {
        private Timer _timer;
        public IServiceProvider services { get; }
        private readonly Settings settings;
        private readonly ILogger _logger;
        private readonly object syncRoot = new object();
        private readonly Random random = new Random();
        public TaskSchedulerService(IServiceProvider serviceProvider)
        {
            this.services = serviceProvider;
            this.settings = services.GetRequiredService<Settings>();
            this._logger = serviceProvider.GetRequiredService<ILogger<TaskSchedulerService>>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //var settings = services.GetRequiredService<Settings>();
            //var interval = settings.GetValue<int>("RunInterval");

            var interval = settings?.RunInterval ?? 0;
            if (interval == 0)
            {
                _logger.LogWarning("The Interval don't can be zero!");
                interval = 60;
            }
            _timer = new Timer(
             (e) => ProccessTask(),//kakoy libo method
             null, //sostayanie v cmisle parametr
             TimeSpan.Zero,//begin and end interval
             TimeSpan.FromSeconds(interval)
         //Eto vremya kotoriy proydyot vremya s momenta sozdanie taymera do evo zapuska
         //period kotoriy on budet vizivat etot metod
         );
            return Task.CompletedTask;
        }

        private void ProccessTask()
        {
            if (Monitor.TryEnter(syncRoot))
            {
                _logger.LogInformation($"Process Task Started");
                for (int i = 0; i < 20; i++) DoWork();

                _logger.LogInformation($" Process Task End");
                Monitor.Exit(syncRoot);
            }
            else
            {
                _logger.LogInformation("The process is currently worked. Skipped");

            }
        }
        private void DoWork()
        {
            var number = random.Next(20);
            var processor = services.GetRequiredService<TaskProcessor>();
            var queue = services.GetRequiredService<IBackgroundTaskQueue>();
            queue.QueueBackgroundWorkItem(token =>
            {
                return processor.RunAsync(number, token); 
            });

        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }
        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
