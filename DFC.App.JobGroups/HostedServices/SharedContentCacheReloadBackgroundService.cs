using DFC.App.JobGroups.Data.Contracts;
using DFC.Compui.Telemetry.HostedService;
using DFC.Content.Pkg.Netcore.Data.Models.ClientOptions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace DFC.App.JobGroups.HostedServices
{
    [ExcludeFromCodeCoverage]
    public class SharedContentCacheReloadBackgroundService : BackgroundService
    {
        private readonly ILogger<SharedContentCacheReloadBackgroundService> logger;
        private readonly CmsApiClientOptions cmsApiClientOptions;
        private readonly ISharedContentCacheReloadService cacheReloadService;
        private readonly IHostedServiceTelemetryWrapper hostedServiceTelemetryWrapper;

        public SharedContentCacheReloadBackgroundService(ILogger<SharedContentCacheReloadBackgroundService> logger, CmsApiClientOptions cmsApiClientOptions, ISharedContentCacheReloadService sharedContentCacheReloadService, IHostedServiceTelemetryWrapper hostedServiceTelemetryWrapper)
        {
            this.logger = logger;
            this.cmsApiClientOptions = cmsApiClientOptions;
            this.cacheReloadService = sharedContentCacheReloadService;
            this.hostedServiceTelemetryWrapper = hostedServiceTelemetryWrapper;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Cache reload started");

            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Cache reload stopped");

            return base.StopAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Cache reload executing");

            if (cmsApiClientOptions.BaseAddress != null)
            {

                var task = hostedServiceTelemetryWrapper.Execute(() => cacheReloadService.ReloadAsync(stoppingToken), nameof(SharedContentCacheReloadBackgroundService));

                if (!task.IsCompletedSuccessfully)
                {
                    logger.LogInformation("Cache reload didn't complete successfully");
                    if (task.Exception != null)
                    {
                        logger.LogError(task.Exception.ToString());
                        throw task.Exception;
                    }
                }

                return task;
            }

            logger.LogInformation("Base Address is null, aborting cache reload");

            return Task.CompletedTask;
        }
    }
}