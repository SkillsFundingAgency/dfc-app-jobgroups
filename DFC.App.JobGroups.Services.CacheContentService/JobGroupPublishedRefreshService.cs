﻿using DFC.App.JobGroups.Data.Contracts;
using DFC.App.JobGroups.Data.Models.ClientOptions;
using DFC.App.JobGroups.Data.Models.JobGroupModels;
using DFC.Compui.Cosmos.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DFC.App.JobGroups.Services.CacheContentService
{
    public class JobGroupPublishedRefreshService : IJobGroupPublishedRefreshService
    {
        private readonly ILogger<JobGroupPublishedRefreshService> logger;
        private readonly IDocumentService<JobGroupModel> jobGroupDocumentService;
        private readonly IJobGroupApiConnector jobGroupApiConnector;
        private readonly JobGroupDraftApiClientOptions jobGroupDraftApiClientOptions;

        public JobGroupPublishedRefreshService(
            ILogger<JobGroupPublishedRefreshService> logger,
            IDocumentService<JobGroupModel> jobGroupDocumentService,
            IJobGroupApiConnector jobGroupApiConnector,
            JobGroupDraftApiClientOptions jobGroupDraftApiClientOptions)
        {
            this.logger = logger;
            this.jobGroupDocumentService = jobGroupDocumentService;
            this.jobGroupApiConnector = jobGroupApiConnector;
            this.jobGroupDraftApiClientOptions = jobGroupDraftApiClientOptions;
        }

        public async Task<HttpStatusCode> ReloadAsync(Uri url)
        {
            var fullUrl = new Uri($"{url}/{jobGroupDraftApiClientOptions.SummaryEndpoint}", UriKind.Absolute);

            logger.LogInformation($"Refreshing all Job Groups from {fullUrl}");
            var summaries = await jobGroupApiConnector.GetSummaryAsync(fullUrl).ConfigureAwait(false);

            if (summaries != null && summaries.Any())
            {
                await PurgeAsync().ConfigureAwait(false);

                foreach (var item in summaries)
                {
                    await ReloadItemAsync(new Uri($"{url}/{jobGroupDraftApiClientOptions.DetailEndpoint}/{item.Id}", UriKind.Absolute)).ConfigureAwait(false);
                }

                logger.LogInformation($"Refreshed all Job Groups from {fullUrl}");

                return HttpStatusCode.OK;
            }

            return HttpStatusCode.NoContent;
        }

        public async Task<HttpStatusCode> ReloadItemAsync(Uri url)
        {
            logger.LogInformation($"Getting Job Group item: {url}");

            var jobGroupModel = await jobGroupApiConnector.GetDetailsAsync(url).ConfigureAwait(false);

            if (jobGroupModel != null)
            {
                var existingJobGroup = await jobGroupDocumentService.GetAsync(w => w.Soc == jobGroupModel.Soc, jobGroupModel.PartitionKey!).ConfigureAwait(false);
                if (existingJobGroup != null)
                {
                    jobGroupModel.Etag = existingJobGroup.Etag;
                }

                logger.LogInformation($"Upserting Job Groups item: {jobGroupModel.Soc} / {url}");
                return await jobGroupDocumentService.UpsertAsync(jobGroupModel).ConfigureAwait(false);
            }

            return HttpStatusCode.BadRequest;
        }

        public async Task<bool> PurgeAsync()
        {
            logger.LogInformation("Purging all Job Groups");
            return await jobGroupDocumentService.PurgeAsync().ConfigureAwait(false);
        }
    }
}
