﻿using AutoMapper;
using DFC.App.JobGroups.Data.Contracts;
using DFC.App.JobGroups.Data.Enums;
using DFC.App.JobGroups.Data.Models.CmsApiModels;
using DFC.App.JobGroups.Data.Models.ContentModels;
using DFC.Compui.Cosmos.Contracts;
using DFC.Content.Pkg.Netcore.Data.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace DFC.App.JobGroups.Services.CacheContentService.Webhooks
{
    public class WebhooksContentService : IWebhooksContentService
    {
        private readonly ILogger<WebhooksContentService> logger;
        private readonly IMapper mapper;
        private readonly ICmsApiService cmsApiService;
        private readonly IDocumentService<ContentItemModel> contentItemDocumentService;
        private readonly IJobGroupCacheRefreshService jobGroupCacheRefreshService;

        public WebhooksContentService(
            ILogger<WebhooksContentService> logger,
            IMapper mapper,
            ICmsApiService cmsApiService,
            IDocumentService<ContentItemModel> contentItemDocumentService,
            IJobGroupCacheRefreshService jobGroupCacheRefreshService)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.cmsApiService = cmsApiService;
            this.contentItemDocumentService = contentItemDocumentService;
            this.jobGroupCacheRefreshService = jobGroupCacheRefreshService;
        }

        public async Task<HttpStatusCode> ProcessContentAsync(Guid eventId, string? apiEndpoint, MessageContentType messageContentType)
        {
            if (!Uri.TryCreate(apiEndpoint, UriKind.Absolute, out Uri? url))
            {
                throw new InvalidDataException($"Invalid Api url '{apiEndpoint}' received for Event Id: {eventId}");
            }

            switch (messageContentType)
            {
                case MessageContentType.SharedContentItem:
                    logger.LogInformation($"Event Id: {eventId} - processing shared content for: {url}");
                    return await ProcessSharedContentAsync(eventId, url).ConfigureAwait(false);
                case MessageContentType.JobGroup:
                    logger.LogInformation($"Event Id: {eventId} - processing LMI SOC refresh for: {url}");
                    return await jobGroupCacheRefreshService.ReloadAsync(url).ConfigureAwait(false);
                case MessageContentType.JobGroupItem:
                    logger.LogInformation($"Event Id: {eventId} - processing LMI SOC item for: {url}");
                    return await jobGroupCacheRefreshService.ReloadItemAsync(url).ConfigureAwait(false);
            }

            return HttpStatusCode.BadRequest;
        }

        public async Task<HttpStatusCode> ProcessSharedContentAsync(Guid eventId, Uri url)
        {
            var apiDataModel = await cmsApiService.GetItemAsync<CmsApiSharedContentModel>(url).ConfigureAwait(false);
            var contentItemModel = mapper.Map<ContentItemModel>(apiDataModel);

            if (contentItemModel == null)
            {
                logger.LogWarning($"Event Id: {eventId} - no shared content for: {url}");
                return HttpStatusCode.NoContent;
            }

            var contentResult = await contentItemDocumentService.UpsertAsync(contentItemModel).ConfigureAwait(false);

            logger.LogInformation($"Event Id: {eventId} - processed shared content result: {contentResult} - for: {url}");
            return contentResult;
        }
    }
}
