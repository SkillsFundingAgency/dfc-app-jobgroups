﻿using DFC.App.JobGroups.Data.Enums;
using System;
using System.Net;
using System.Threading.Tasks;

namespace DFC.App.JobGroups.Data.Contracts
{
    public interface IWebhooksContentService
    {
        Task<HttpStatusCode> ProcessContentAsync(bool isDraft, Guid eventId, Guid? contentId, string? apiEndpoint, MessageContentType messageContentType);

        Task<HttpStatusCode> ProcessSharedContentAsync(Guid eventId, Uri url);
    }
}