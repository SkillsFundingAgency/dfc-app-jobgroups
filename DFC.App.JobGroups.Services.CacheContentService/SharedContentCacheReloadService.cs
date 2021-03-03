﻿using AutoMapper;
using DFC.App.JobGroups.Data;
using DFC.App.JobGroups.Data.Contracts;
using DFC.App.JobGroups.Data.Models.CmsApiModels;
using DFC.App.JobGroups.Data.Models.ContentModels;
using DFC.Compui.Cosmos.Contracts;
using DFC.Content.Pkg.Netcore.Data.Contracts;
using DFC.Content.Pkg.Netcore.Data.Models.ClientOptions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DFC.App.JobGroups.Services.CacheContentService
{
    public class SharedContentCacheReloadService : ISharedContentCacheReloadService
    {
        private readonly ILogger<SharedContentCacheReloadService> logger;
        private readonly IMapper mapper;
        private readonly IDocumentService<ContentItemModel> contentItemDocumentService;
        private readonly ICmsApiService cmsApiService;
        private readonly CmsApiClientOptions cmsApiClientOptions;
        private readonly IContentTypeMappingService contentTypeMappingService;

        public SharedContentCacheReloadService(
            ILogger<SharedContentCacheReloadService> logger,
            IMapper mapper,
            IDocumentService<ContentItemModel> contentItemDocumentService,
            ICmsApiService cmsApiService,
            CmsApiClientOptions cmsApiClientOptions,
            IContentTypeMappingService contentTypeMappingService)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.contentItemDocumentService = contentItemDocumentService;
            this.cmsApiService = cmsApiService;
            this.cmsApiClientOptions = cmsApiClientOptions;
            this.contentTypeMappingService = contentTypeMappingService;
        }

        public async Task ReloadAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Reload shared content started");

            contentTypeMappingService.AddMapping(Constants.ContentTypeSharedContent, typeof(CmsApiSharedContentModel));

            if (stoppingToken.IsCancellationRequested)
            {
                logger.LogWarning("Reload shared content cancelled");

                return;
            }

            await ReloadCacheItem(Guid.Parse(Constants.SharedContentAskAdviserItemId), stoppingToken).ConfigureAwait(false);

            logger.LogInformation("Reload All shared content completed");
        }

        private async Task ReloadCacheItem(Guid itemId, CancellationToken stoppingToken)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                logger.LogWarning("Reload shared content cancelled");

                return;
            }

            var url = new Uri($"{cmsApiClientOptions.BaseAddress}/{Constants.ContentTypeSharedContent.ToLowerInvariant()}/{itemId}", UriKind.Absolute);
            var apiDataModel = await cmsApiService.GetItemAsync<CmsApiSharedContentModel>(url).ConfigureAwait(false);

            if (apiDataModel == null)
            {
                logger.LogError($"Shared content: {itemId} not found in API response");
                return;
            }

            var contentItemModel = mapper.Map<ContentItemModel>(apiDataModel);

            await contentItemDocumentService.UpsertAsync(contentItemModel).ConfigureAwait(false);
        }
    }
}
