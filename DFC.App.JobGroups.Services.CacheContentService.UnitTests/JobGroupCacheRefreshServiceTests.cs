﻿using DFC.App.JobGroups.Data.Contracts;
using DFC.App.JobGroups.Data.Models.JobGroupModels;
using DFC.App.JobGroups.Data.Models.LmiTransformationApiModels;
using DFC.Compui.Cosmos.Contracts;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace DFC.App.JobGroups.Services.CacheContentService.UnitTests
{
    [Trait("Category", "Job Group cache refresh Unit Tests")]
    public class JobGroupCacheRefreshServiceTests
    {
        private readonly ILogger<JobGroupCacheRefreshService> fakeLogger = A.Fake<ILogger<JobGroupCacheRefreshService>>();
        private readonly IDocumentService<JobGroupModel> fakeJobGroupDocumentService = A.Fake<IDocumentService<JobGroupModel>>();
        private readonly ILmiTransformationApiConnector fakeLmiTransformationApiConnector = A.Fake<ILmiTransformationApiConnector>();
        private readonly JobGroupCacheRefreshService jobGroupCacheRefreshService;

        public JobGroupCacheRefreshServiceTests()
        {
            jobGroupCacheRefreshService = new JobGroupCacheRefreshService(fakeLogger, fakeJobGroupDocumentService, fakeLmiTransformationApiConnector);
        }

        [Fact]
        public async Task JobGroupCacheRefreshServiceReloadIsSuccessful()
        {
            // arrange
            const HttpStatusCode expectedResult = HttpStatusCode.OK;
            var getSummaryResponse = new List<JobGroupSummaryItemModel>
            {
                 new JobGroupSummaryItemModel
                 {
                     Soc = 1,
                     Title = "A title 1",
                 },
                 new JobGroupSummaryItemModel
                 {
                     Soc = 2,
                     Title = "A title 2",
                 },
            };
            var getDetailResponse = new JobGroupModel
            {
                Soc = 2,
                Title = "A title 2",
            };

            A.CallTo(() => fakeJobGroupDocumentService.PurgeAsync()).Returns(true);
            A.CallTo(() => fakeLmiTransformationApiConnector.GetSummaryAsync(A<Uri>.Ignored)).Returns(getSummaryResponse);
            A.CallTo(() => fakeLmiTransformationApiConnector.GetDetailsAsync(A<Uri>.Ignored)).Returns(getDetailResponse);
            A.CallTo(() => fakeJobGroupDocumentService.UpsertAsync(A<JobGroupModel>.Ignored)).Returns(HttpStatusCode.OK);

            // act
            var result = await jobGroupCacheRefreshService.ReloadAsync(new Uri("https://somewhere.com", UriKind.Absolute)).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeJobGroupDocumentService.PurgeAsync()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeLmiTransformationApiConnector.GetSummaryAsync(A<Uri>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeLmiTransformationApiConnector.GetDetailsAsync(A<Uri>.Ignored)).MustHaveHappened(getSummaryResponse.Count, Times.Exactly);
            A.CallTo(() => fakeJobGroupDocumentService.UpsertAsync(A<JobGroupModel>.Ignored)).MustHaveHappened(getSummaryResponse.Count, Times.Exactly);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task JobGroupCacheRefreshServiceReloadItemIsSuccessful()
        {
            // arrange
            const HttpStatusCode expectedResult = HttpStatusCode.OK;
            var getDetailResponse = new JobGroupModel
            {
                Soc = 2,
                Title = "A title 2",
            };

            A.CallTo(() => fakeLmiTransformationApiConnector.GetDetailsAsync(A<Uri>.Ignored)).Returns(getDetailResponse);
            A.CallTo(() => fakeJobGroupDocumentService.UpsertAsync(A<JobGroupModel>.Ignored)).Returns(HttpStatusCode.OK);

            // act
            var result = await jobGroupCacheRefreshService.ReloadItemAsync(new Uri("https://somewhere.com", UriKind.Absolute)).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeLmiTransformationApiConnector.GetDetailsAsync(A<Uri>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeJobGroupDocumentService.UpsertAsync(A<JobGroupModel>.Ignored)).MustHaveHappenedOnceExactly();

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task JobGroupCacheRefreshServiceReloadItemReturnsBadRequest()
        {
            // arrange
            const HttpStatusCode expectedResult = HttpStatusCode.BadRequest;
            JobGroupModel? getDetailResponse = null;

            A.CallTo(() => fakeLmiTransformationApiConnector.GetDetailsAsync(A<Uri>.Ignored)).Returns(getDetailResponse);

            // act
            var result = await jobGroupCacheRefreshService.ReloadItemAsync(new Uri("https://somewhere.com", UriKind.Absolute)).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeLmiTransformationApiConnector.GetDetailsAsync(A<Uri>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeJobGroupDocumentService.UpsertAsync(A<JobGroupModel>.Ignored)).MustNotHaveHappened();

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task JobGroupCacheRefreshServicePurgeIsSuccessful()
        {
            // arrange
            const bool expectedResult = true;
            A.CallTo(() => fakeJobGroupDocumentService.PurgeAsync()).Returns(true);

            // act
            var result = await jobGroupCacheRefreshService.PurgeAsync().ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeJobGroupDocumentService.PurgeAsync()).MustHaveHappenedOnceExactly();

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task JobGroupCacheRefreshServiceDeleteIsSuccessful()
        {
            // arrange
            const bool expectedResult = true;
            A.CallTo(() => fakeJobGroupDocumentService.DeleteAsync(A<Guid>.Ignored)).Returns(true);

            // act
            var result = await jobGroupCacheRefreshService.DeleteAsync(Guid.NewGuid()).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeJobGroupDocumentService.DeleteAsync(A<Guid>.Ignored)).MustHaveHappenedOnceExactly();

            Assert.Equal(expectedResult, result);
        }
    }
}
