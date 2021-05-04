using DFC.App.JobGroups.Data.Contracts;
using DFC.App.JobGroups.Data.Models;
using DFC.App.JobGroups.Data.Models.ClientOptions;
using DFC.App.JobGroups.Data.Models.JobGroupModels;
using DFC.Compui.Cosmos.Contracts;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace DFC.App.JobGroups.Services.CacheContentService.UnitTests
{
    [Trait("Category", "Job Group published cache refresh Unit Tests")]
    public class JobGroupPublishedRefreshServiceTests
    {
        private readonly ILogger<JobGroupPublishedRefreshService> fakeLogger = A.Fake<ILogger<JobGroupPublishedRefreshService>>();
        private readonly IDocumentService<JobGroupModel> fakeJobGroupDocumentService = A.Fake<IDocumentService<JobGroupModel>>();
        private readonly IJobGroupApiConnector fakeJobGroupApiConnector = A.Fake<IJobGroupApiConnector>();
        private readonly JobGroupDraftApiClientOptions dummyJobGroupDraftApiClientOptions = A.Dummy<JobGroupDraftApiClientOptions>();
        private readonly JobGroupPublishedRefreshService jobGroupPublishedRefreshService;

        public JobGroupPublishedRefreshServiceTests()
        {
            jobGroupPublishedRefreshService = new JobGroupPublishedRefreshService(fakeLogger, fakeJobGroupDocumentService, fakeJobGroupApiConnector, dummyJobGroupDraftApiClientOptions);
        }

        [Fact]
        public async Task JobGroupPublishedRefreshServiceReloadIsSuccessful()
        {
            // arrange
            const HttpStatusCode expectedResult = HttpStatusCode.OK;
            var existingJobGroup = A.Dummy<JobGroupModel>();
            var getSummaryResponse = new List<JobGroupSummaryItemModel>
            {
                 new JobGroupSummaryItemModel
                 {
                     Id = Guid.NewGuid(),
                     Soc = 1,
                     Title = "A title 1",
                 },
                 new JobGroupSummaryItemModel
                 {
                     Id = Guid.NewGuid(),
                     Soc = 2,
                     Title = "A title 2",
                 },
            };
            var getDetailResponse = new JobGroupModel
            {
                Id = Guid.NewGuid(),
                Soc = 2,
                Title = "A title 2",
            };

            A.CallTo(() => fakeJobGroupApiConnector.GetSummaryAsync(A<Uri>.Ignored)).Returns(getSummaryResponse);
            A.CallTo(() => fakeJobGroupDocumentService.PurgeAsync()).Returns(true);
            A.CallTo(() => fakeJobGroupApiConnector.GetDetailsAsync(A<Uri>.Ignored)).Returns(getDetailResponse);
            A.CallTo(() => fakeJobGroupDocumentService.GetAsync(A<Expression<Func<JobGroupModel, bool>>>.Ignored, A<string>.Ignored)).Returns(existingJobGroup);
            A.CallTo(() => fakeJobGroupDocumentService.UpsertAsync(A<JobGroupModel>.Ignored)).Returns(HttpStatusCode.OK);

            // act
            var result = await jobGroupPublishedRefreshService.ReloadAsync(new Uri("https://somewhere.com", UriKind.Absolute)).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeJobGroupApiConnector.GetSummaryAsync(A<Uri>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeJobGroupDocumentService.PurgeAsync()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeJobGroupApiConnector.GetDetailsAsync(A<Uri>.Ignored)).MustHaveHappened(getSummaryResponse.Count, Times.Exactly);
            A.CallTo(() => fakeJobGroupDocumentService.GetAsync(A<Expression<Func<JobGroupModel, bool>>>.Ignored, A<string>.Ignored)).MustHaveHappened(getSummaryResponse.Count, Times.Exactly);
            A.CallTo(() => fakeJobGroupDocumentService.UpsertAsync(A<JobGroupModel>.Ignored)).MustHaveHappened(getSummaryResponse.Count, Times.Exactly);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task JobGroupPublishedRefreshServiceReloadReturnsNoSummaries()
        {
            // arrange
            const HttpStatusCode expectedResult = HttpStatusCode.NoContent;
            var noModels = A.CollectionOfDummy<JobGroupSummaryItemModel>(0);

            A.CallTo(() => fakeJobGroupApiConnector.GetSummaryAsync(A<Uri>.Ignored)).Returns(noModels);

            // act
            var result = await jobGroupPublishedRefreshService.ReloadAsync(new Uri("https://somewhere.com", UriKind.Absolute)).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeJobGroupApiConnector.GetSummaryAsync(A<Uri>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeJobGroupDocumentService.PurgeAsync()).MustNotHaveHappened();
            A.CallTo(() => fakeJobGroupApiConnector.GetDetailsAsync(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeJobGroupDocumentService.GetAsync(A<Expression<Func<JobGroupModel, bool>>>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeJobGroupDocumentService.UpsertAsync(A<JobGroupModel>.Ignored)).MustNotHaveHappened();

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task JobGroupPublishedRefreshServiceReloadReturnsNullForSummaries()
        {
            // arrange
            const HttpStatusCode expectedResult = HttpStatusCode.NoContent;
            IList<JobGroupSummaryItemModel>? nullModels = default;

            A.CallTo(() => fakeJobGroupApiConnector.GetSummaryAsync(A<Uri>.Ignored)).Returns(nullModels);

            // act
            var result = await jobGroupPublishedRefreshService.ReloadAsync(new Uri("https://somewhere.com", UriKind.Absolute)).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeJobGroupApiConnector.GetSummaryAsync(A<Uri>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeJobGroupDocumentService.PurgeAsync()).MustNotHaveHappened();
            A.CallTo(() => fakeJobGroupApiConnector.GetDetailsAsync(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeJobGroupDocumentService.GetAsync(A<Expression<Func<JobGroupModel, bool>>>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeJobGroupDocumentService.UpsertAsync(A<JobGroupModel>.Ignored)).MustNotHaveHappened();

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task JobGroupPublishedRefreshServiceReloadItemForExistingJobGroupIsSuccessful()
        {
            // arrange
            const HttpStatusCode expectedResult = HttpStatusCode.OK;
            var existingJobGroup = A.Dummy<JobGroupModel>();
            var getDetailResponse = new JobGroupModel
            {
                Id = Guid.NewGuid(),
                Soc = 2,
                Title = "A title 2",
            };
            existingJobGroup.Id = Guid.NewGuid();

            A.CallTo(() => fakeJobGroupApiConnector.GetDetailsAsync(A<Uri>.Ignored)).Returns(getDetailResponse);
            A.CallTo(() => fakeJobGroupDocumentService.GetAsync(A<Expression<Func<JobGroupModel, bool>>>.Ignored, A<string>.Ignored)).Returns(existingJobGroup);
            A.CallTo(() => fakeJobGroupDocumentService.UpsertAsync(A<JobGroupModel>.Ignored)).Returns(HttpStatusCode.OK);

            // act
            var result = await jobGroupPublishedRefreshService.ReloadItemAsync(new Uri("https://somewhere.com", UriKind.Absolute)).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeJobGroupApiConnector.GetDetailsAsync(A<Uri>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeJobGroupDocumentService.GetAsync(A<Expression<Func<JobGroupModel, bool>>>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeJobGroupDocumentService.DeleteAsync(A<Guid>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeJobGroupDocumentService.UpsertAsync(A<JobGroupModel>.Ignored)).MustHaveHappenedOnceExactly();

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task JobGroupPublishedRefreshServiceReloadItemForNonExistingJobGroupIsSuccessful()
        {
            // arrange
            const HttpStatusCode expectedResult = HttpStatusCode.OK;
            JobGroupModel? nullJobGroup = null;
            var getDetailResponse = new JobGroupModel
            {
                Soc = 2,
                Title = "A title 2",
            };

            A.CallTo(() => fakeJobGroupApiConnector.GetDetailsAsync(A<Uri>.Ignored)).Returns(getDetailResponse);
            A.CallTo(() => fakeJobGroupDocumentService.GetAsync(A<Expression<Func<JobGroupModel, bool>>>.Ignored, A<string>.Ignored)).Returns(nullJobGroup);
            A.CallTo(() => fakeJobGroupDocumentService.UpsertAsync(A<JobGroupModel>.Ignored)).Returns(HttpStatusCode.OK);

            // act
            var result = await jobGroupPublishedRefreshService.ReloadItemAsync(new Uri("https://somewhere.com", UriKind.Absolute)).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeJobGroupApiConnector.GetDetailsAsync(A<Uri>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeJobGroupDocumentService.GetAsync(A<Expression<Func<JobGroupModel, bool>>>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeJobGroupDocumentService.DeleteAsync(A<Guid>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeJobGroupDocumentService.UpsertAsync(A<JobGroupModel>.Ignored)).MustHaveHappenedOnceExactly();

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task JobGroupPublishedRefreshServiceReloadItemReturnsBadRequest()
        {
            // arrange
            const HttpStatusCode expectedResult = HttpStatusCode.BadRequest;
            JobGroupModel? getDetailResponse = null;

            A.CallTo(() => fakeJobGroupApiConnector.GetDetailsAsync(A<Uri>.Ignored)).Returns(getDetailResponse);

            // act
            var result = await jobGroupPublishedRefreshService.ReloadItemAsync(new Uri("https://somewhere.com", UriKind.Absolute)).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeJobGroupApiConnector.GetDetailsAsync(A<Uri>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeJobGroupDocumentService.GetAsync(A<Expression<Func<JobGroupModel, bool>>>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeJobGroupDocumentService.DeleteAsync(A<Guid>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeJobGroupDocumentService.UpsertAsync(A<JobGroupModel>.Ignored)).MustNotHaveHappened();

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task JobGroupPublishedRefreshServicePurgeIsSuccessful()
        {
            // arrange
            const bool expectedResult = true;
            A.CallTo(() => fakeJobGroupDocumentService.PurgeAsync()).Returns(true);

            // act
            var result = await jobGroupPublishedRefreshService.PurgeAsync().ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeJobGroupDocumentService.PurgeAsync()).MustHaveHappenedOnceExactly();

            Assert.Equal(expectedResult, result);
        }
    }
}
