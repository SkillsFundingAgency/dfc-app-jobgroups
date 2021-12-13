using AutoMapper;
using DFC.App.JobGroups.Data.Contracts;
using DFC.App.JobGroups.Data.Enums;
using DFC.App.JobGroups.Data.Models;
using DFC.App.JobGroups.Data.Models.ClientOptions;
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
        private const string EventTypePublished = "published";

        private readonly ILogger<WebhooksContentService> logger;
        private readonly IMapper mapper;
        private readonly ICmsApiService cmsApiService;
        private readonly IDocumentService<ContentItemModel> contentItemDocumentService;
        private readonly IJobGroupCacheRefreshService jobGroupCacheRefreshService;
        private readonly IJobGroupPublishedRefreshService jobGroupPublishedRefreshService;
        private readonly IEventGridService eventGridService;
        private readonly EventGridClientOptions eventGridClientOptions;

        public WebhooksContentService(
            ILogger<WebhooksContentService> logger,
            IMapper mapper,
            ICmsApiService cmsApiService,
            IDocumentService<ContentItemModel> contentItemDocumentService,
            IJobGroupCacheRefreshService jobGroupCacheRefreshService,
            IJobGroupPublishedRefreshService jobGroupPublishedRefreshService,
            IEventGridService eventGridService,
            EventGridClientOptions eventGridClientOptions)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.cmsApiService = cmsApiService;
            this.contentItemDocumentService = contentItemDocumentService;
            this.jobGroupCacheRefreshService = jobGroupCacheRefreshService;
            this.jobGroupPublishedRefreshService = jobGroupPublishedRefreshService;
            this.eventGridService = eventGridService;
            this.eventGridClientOptions = eventGridClientOptions;
        }

        public async Task<HttpStatusCode> ProcessContentAsync(bool isDraft, Guid eventId, Guid? contentId, string? apiEndpoint, MessageContentType messageContentType)
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
                    if (isDraft)
                    {
                        logger.LogInformation($"Event Id: {eventId} - processing draft LMI SOC refresh for: {url}");
                        var result = await jobGroupCacheRefreshService.ReloadAsync(url).ConfigureAwait(false);
                        if (result == HttpStatusCode.OK || result == HttpStatusCode.Created)
                        {
                            await PostPublishedEventAsync($"Publish all SOCs to delta-report API", eventGridClientOptions.ApiEndpoint, Guid.NewGuid()).ConfigureAwait(false);
                        }

                        return result;
                    }
                    else
                    {
                        logger.LogInformation($"Event Id: {eventId} - processing published LMI SOC refresh from draft for: {url}");
                        return await jobGroupPublishedRefreshService.ReloadAsync(url).ConfigureAwait(false);
                    }

                case MessageContentType.JobGroupItem:
                    if (isDraft)
                    {
                        logger.LogInformation($"Event Id: {eventId} - processing draft LMI SOC item for: {url}");
                        var result = await jobGroupCacheRefreshService.ReloadItemAsync(url).ConfigureAwait(false);
                        if (result == HttpStatusCode.OK || result == HttpStatusCode.Created)
                        {
                            var eventGridEndpoint = new Uri($"{eventGridClientOptions.ApiEndpoint}/{contentId}", UriKind.Absolute);
                            await PostPublishedEventAsync($"Publish individual SOC to delta-report API", eventGridEndpoint, contentId).ConfigureAwait(false);
                        }

                        return result;
                    }
                    else
                    {
                        logger.LogInformation($"Event Id: {eventId} - processing published LMI SOC item from draft for: {url}");
                        return await jobGroupPublishedRefreshService.ReloadItemAsync(url).ConfigureAwait(false);
                    }
            }

            return HttpStatusCode.BadRequest;
        }

        public async Task<HttpStatusCode> ProcessSharedContentAsync(Guid eventId, Uri? url)
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

        public Task PostPublishedEventAsync(string displayText, Uri? apiEndpoint, Guid? contentId)
        {
            logger.LogInformation($"Posting to event grid for: {displayText}");

            var eventGridEventData = new EventGridEventData
            {
                ItemId = contentId.ToString(),
                Api = apiEndpoint?.ToString(),
                DisplayText = displayText,
                VersionId = Guid.NewGuid().ToString(),
                Author = eventGridClientOptions.SubjectPrefix,
            };

            return eventGridService.SendEventAsync(eventGridEventData, eventGridClientOptions.SubjectPrefix, EventTypePublished);
        }
    }
}
