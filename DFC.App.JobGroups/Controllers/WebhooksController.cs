﻿using DFC.App.JobGroups.Data.Contracts;
using DFC.App.JobGroups.Data.Enums;
using DFC.App.JobGroups.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DFC.App.JobGroups.Controllers
{
    [Route("api/webhook")]
    public class WebhooksController : Controller
    {
        private readonly Dictionary<string, WebhookCacheOperation> acceptedEventTypes = new Dictionary<string, WebhookCacheOperation>
        {
            { "draft", WebhookCacheOperation.CreateOrUpdate },
            { "published", WebhookCacheOperation.CreateOrUpdate },
            { "draft-discarded", WebhookCacheOperation.Delete },
            { "unpublished", WebhookCacheOperation.Delete },
            { "deleted", WebhookCacheOperation.Delete },
        };

        private readonly ILogger<WebhooksController> logger;
        private readonly IWebhooksService webhookService;

        public WebhooksController(
            ILogger<WebhooksController> logger,
            IWebhooksService webhookService)
        {
            this.logger = logger;
            this.webhookService = webhookService;
        }

        [HttpPost]
        [Route("ReceiveEvents")]
        public async Task<IActionResult> ReceiveEvents()
        {
            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            string requestContent = await reader.ReadToEndAsync().ConfigureAwait(false);
            logger.LogInformation($"Received events: {requestContent}");

            var eventGridSubscriber = new EventGridSubscriber();
            foreach (var key in acceptedEventTypes.Keys)
            {
                eventGridSubscriber.AddOrUpdateCustomEventMapping(key, typeof(EventGridEventData));
            }

            var eventGridEvents = eventGridSubscriber.DeserializeEventGridEvents(requestContent);

            foreach (var eventGridEvent in eventGridEvents)
            {
                if (!Guid.TryParse(eventGridEvent.Id, out Guid eventId))
                {
                    throw new InvalidDataException($"Invalid Guid for EventGridEvent.Id '{eventGridEvent.Id}'");
                }

                if (eventGridEvent.Data is SubscriptionValidationEventData subscriptionValidationEventData)
                {
                    logger.LogInformation($"Got SubscriptionValidation event data, validationCode: {subscriptionValidationEventData!.ValidationCode},  validationUrl: {subscriptionValidationEventData.ValidationUrl}, topic: {eventGridEvent.Topic}");

                    var responseData = new SubscriptionValidationResponse()
                    {
                        ValidationResponse = subscriptionValidationEventData.ValidationCode,
                    };

                    return Ok(responseData);
                }
                else if (eventGridEvent.Data is EventGridEventData eventGridEventData)
                {
                    if (!Guid.TryParse(eventGridEventData.ItemId, out Guid contentId))
                    {
                        throw new InvalidDataException($"Invalid Guid for EventGridEvent.Data.ItemId '{eventGridEventData.ItemId}'");
                    }

                    var cacheOperation = acceptedEventTypes[eventGridEvent.EventType];

                    logger.LogInformation($"Got Event Id: {eventId}: {eventGridEvent.EventType}: Cache operation: {cacheOperation} {eventGridEventData.Api}");

                    var isDraft = eventGridEvent.EventType.Equals("draft", StringComparison.OrdinalIgnoreCase);

                    var result = await webhookService.ProcessMessageAsync(isDraft, cacheOperation, eventId, contentId, eventGridEventData.Api!).ConfigureAwait(false);

                    LogResult(eventId, result);
                }
                else
                {
                    throw new InvalidDataException($"Invalid event type '{eventGridEvent.EventType}' received for Event Id: {eventId}, should be one of '{string.Join(",", acceptedEventTypes.Keys)}'");
                }
            }

            return Ok();
        }

        private void LogResult(Guid eventId, HttpStatusCode result)
        {
            switch (result)
            {
                case HttpStatusCode.OK:
                    logger.LogInformation($"Event Id: {eventId}, Updated Content");
                    break;

                case HttpStatusCode.Created:
                    logger.LogInformation($"Event Id: {eventId}, Created Content");
                    break;

                case HttpStatusCode.AlreadyReported:
                    logger.LogInformation($"Event Id: {eventId}, Content previously updated");
                    break;

                case HttpStatusCode.NoContent:
                    logger.LogInformation($"Event Id: {eventId}, Content previously deleted");
                    break;

                default:
                    throw new InvalidDataException($"Event Id: {eventId}, Content  not updated: Status: {result}");
            }
        }
    }
}
