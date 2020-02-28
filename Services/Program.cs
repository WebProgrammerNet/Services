using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Debug;
using Services.Models;
using Services.Services;
using Services.Services.TaskQueue;
using Services.Workers;
using System;
using System.Threading.Tasks;

namespace Services
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration(configBuilder =>
                {
                    configBuilder.AddJsonFile("config.json");//configfile
                    configBuilder.AddCommandLine(args);//C pomosu cmd davat parametri
                    //intelesens rabotaet posle dobavlenie sootvetstvuessiy NudGet Packetov.
                })
                .ConfigureLogging((confiLoginig) => {
                    confiLoginig.AddConsole();
                    confiLoginig.AddDebug();
                })
                .ConfigureServices(services => {
                    services.AddHostedService<TaskSchedulerService>();
                    services.AddHostedService<WorkerService>();
                    services.AddSingleton<Settings>();
                    services.AddSingleton<TaskProcessor>();
                    services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
                });
            await builder.RunConsoleAsync();
        }  
    }
}
