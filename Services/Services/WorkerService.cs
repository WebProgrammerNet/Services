using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Services.Models;
using Services.Services.TaskQueue;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Workers
{
    class WorkerService : BackgroundService
    {
        private readonly IBackgroundTaskQueue taskQueue;
        private readonly ILogger<WorkerService> logger;
        private readonly Settings settings;
        //settings prosto soxranyaet kaki te dannie dlya App
        //Mojno i bilo pul zdelat etix servisov i zapuskat ix dinamiceski.
        public WorkerService(IBackgroundTaskQueue taskQueue, ILogger<WorkerService> logger, Settings settings)
        {
            this.taskQueue = taskQueue;
            this.logger = logger;
            this.settings = settings;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var workercount = settings.WorkersCount;
            var workers = Enumerable.Range(0, workercount).Select(num => RunInstance(num, stoppingToken));
            await Task.WhenAll(workers);
        }

        private async Task RunInstance(int num, CancellationToken stoppingToken)
        {
            logger.LogInformation($"#{num} is starting.");
            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem = await taskQueue.DequeueAsync(stoppingToken);
                try
                {
                    logger.LogInformation($"#{num}  Processing Task. Queue Size : {taskQueue.Size}");
                    await workItem(stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogInformation(ex,$"#{num} Error ocurred exeuting task.");
                }
            }
            logger.LogInformation($"#{num} is stopping.");

        }
    }
}
