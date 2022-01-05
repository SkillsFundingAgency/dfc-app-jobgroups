using AutoMapper;
using DFC.App.JobGroups.Data.Contracts;
using DFC.App.JobGroups.Data.Enums;
using DFC.App.JobGroups.Data.Models;
using DFC.App.JobGroups.Data.Models.ClientOptions;
using DFC.App.JobGroups.Data.Models.CmsApiModels;
using DFC.App.JobGroups.Data.Models.ContentModels;
using DFC.App.JobGroups.Services.CacheContentService.Webhooks;
using DFC.Compui.Cosmos.Contracts;
using DFC.Content.Pkg.Netcore.Data.Contracts;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace DFC.App.JobGroups.Services.CacheContentService.UnitTests.WebhooksServices
{
    [Trait("Category", "Webhooks content service  Unit Tests")]
    public class WebhooksContentServiceTests
    {
        private readonly IMapper fakeMapper = A.Fake<IMapper>();

        private readonly ICmsApiService fakeCmsApiService = A.Fake<ICmsApiService>();

        private readonly IDocumentService<ContentItemModel> fakeContentItemDocumentService = A.Fake<IDocumentService<ContentItemModel>>();

        private readonly IJobGroupCacheRefreshService fakeJobGroupCacheRefreshService = A.Fake<IJobGroupCacheRefreshService>();

        private readonly IJobGroupPublishedRefreshService fakeJobGroupPublishedRefreshService = A.Fake<IJobGroupPublishedRefreshService>();
        private readonly IEventGridService fakeEventGridService = A.Fake<IEventGridService>();
        private readonly EventGridClientOptions eventGridClientOptions = new EventGridClientOptions { ApiEndpoint = new Uri("https://somewhere.com", UriKind.Absolute) };
        private readonly WebhooksContentService webhooksContentService;

        public WebhooksContentServiceTests()
        {
            webhooksContentService = new WebhooksContentService(A.Fake<ILogger<WebhooksContentService>>(), fakeMapper, fakeCmsApiService, fakeContentItemDocumentService, fakeJobGroupCacheRefreshService, fakeJobGroupPublishedRefreshService, fakeEventGridService, eventGridClientOptions);
        }

        [Fact]
        public async Task WebhooksContentServiceProcessContentForSharedContentItemReturnsSuccess()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.OK;

            A.CallTo(() => fakeCmsApiService.GetItemAsync<CmsApiSharedContentModel>(A<Uri>.Ignored)).Returns(A.Dummy<CmsApiSharedContentModel>());
            A.CallTo(() => fakeMapper.Map<ContentItemModel>(A<CmsApiSharedContentModel>.Ignored)).Returns(A.Dummy<ContentItemModel>());
            A.CallTo(() => fakeContentItemDocumentService.UpsertAsync(A<ContentItemModel>.Ignored)).Returns(expectedResult);

            // Act
            var result = await webhooksContentService.ProcessContentAsync(false, Guid.NewGuid(), null, "https://somewhere.com", MessageContentType.SharedContentItem).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeJobGroupCacheRefreshService.ReloadAsync(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeJobGroupCacheRefreshService.ReloadItemAsync(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeCmsApiService.GetItemAsync<CmsApiSharedContentModel>(A<Uri>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeMapper.Map<ContentItemModel>(A<CmsApiSharedContentModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeContentItemDocumentService.UpsertAsync(A<ContentItemModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeJobGroupPublishedRefreshService.ReloadAsync(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeJobGroupPublishedRefreshService.ReloadItemAsync(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeEventGridService.SendEventAsync(A<EventGridEventData>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.Created)]
        public async Task WebhooksContentServiceProcessContentForJobGroupForDraftReturnsSuccess(HttpStatusCode expectedResult)
        {
            // Arrange
            A.CallTo(() => fakeJobGroupCacheRefreshService.ReloadAsync(A<Uri>.Ignored)).Returns(expectedResult);

            // Act
            var result = await webhooksContentService.ProcessContentAsync(true, Guid.NewGuid(), null, "https://somewhere.com", MessageContentType.JobGroup).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeJobGroupCacheRefreshService.ReloadAsync(A<Uri>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeJobGroupCacheRefreshService.ReloadItemAsync(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeCmsApiService.GetItemAsync<CmsApiSharedContentModel>(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeMapper.Map<ContentItemModel>(A<CmsApiSharedContentModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeContentItemDocumentService.UpsertAsync(A<ContentItemModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeJobGroupPublishedRefreshService.ReloadAsync(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeJobGroupPublishedRefreshService.ReloadItemAsync(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeEventGridService.SendEventAsync(A<EventGridEventData>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task WebhooksContentServiceProcessContentForJobGroupForPublishedReturnsSuccess()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.OK;

            A.CallTo(() => fakeJobGroupPublishedRefreshService.ReloadAsync(A<Uri>.Ignored)).Returns(expectedResult);

            // Act
            var result = await webhooksContentService.ProcessContentAsync(false, Guid.NewGuid(), null, "https://somewhere.com", MessageContentType.JobGroup).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeJobGroupCacheRefreshService.ReloadAsync(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeJobGroupCacheRefreshService.ReloadItemAsync(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeCmsApiService.GetItemAsync<CmsApiSharedContentModel>(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeMapper.Map<ContentItemModel>(A<CmsApiSharedContentModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeContentItemDocumentService.UpsertAsync(A<ContentItemModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeJobGroupPublishedRefreshService.ReloadAsync(A<Uri>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeJobGroupPublishedRefreshService.ReloadItemAsync(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeEventGridService.SendEventAsync(A<EventGridEventData>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.Created)]
        public async Task WebhooksContentServiceProcessContentForJobGroupItemForDraftReturnsSuccess(HttpStatusCode expectedResult)
        {
            // Arrange
            A.CallTo(() => fakeJobGroupCacheRefreshService.ReloadItemAsync(A<Uri>.Ignored)).Returns(expectedResult);

            // Act
            var result = await webhooksContentService.ProcessContentAsync(true, Guid.NewGuid(), Guid.NewGuid(), "https://somewhere.com", MessageContentType.JobGroupItem).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeJobGroupCacheRefreshService.ReloadAsync(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeJobGroupCacheRefreshService.ReloadItemAsync(A<Uri>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeCmsApiService.GetItemAsync<CmsApiSharedContentModel>(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeMapper.Map<ContentItemModel>(A<CmsApiSharedContentModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeContentItemDocumentService.UpsertAsync(A<ContentItemModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeJobGroupPublishedRefreshService.ReloadAsync(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeJobGroupPublishedRefreshService.ReloadItemAsync(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeEventGridService.SendEventAsync(A<EventGridEventData>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task WebhooksContentServiceProcessContentForJobGroupItemForPublishedReturnsSuccess()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.OK;

            A.CallTo(() => fakeJobGroupPublishedRefreshService.ReloadItemAsync(A<Uri>.Ignored)).Returns(expectedResult);

            // Act
            var result = await webhooksContentService.ProcessContentAsync(false, Guid.NewGuid(), Guid.NewGuid(), "https://somewhere.com", MessageContentType.JobGroupItem).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeJobGroupCacheRefreshService.ReloadAsync(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeJobGroupCacheRefreshService.ReloadItemAsync(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeCmsApiService.GetItemAsync<CmsApiSharedContentModel>(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeMapper.Map<ContentItemModel>(A<CmsApiSharedContentModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeContentItemDocumentService.UpsertAsync(A<ContentItemModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeJobGroupPublishedRefreshService.ReloadAsync(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeJobGroupPublishedRefreshService.ReloadItemAsync(A<Uri>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeEventGridService.SendEventAsync(A<EventGridEventData>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task WebhooksContentServiceProcessContentForJobGroupItemReturnsBadRequest()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.BadRequest;

            // Act
            var result = await webhooksContentService.ProcessContentAsync(false, Guid.NewGuid(), Guid.NewGuid(), "https://somewhere.com", MessageContentType.None).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeJobGroupCacheRefreshService.ReloadAsync(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeJobGroupCacheRefreshService.ReloadItemAsync(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeCmsApiService.GetItemAsync<CmsApiSharedContentModel>(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeMapper.Map<ContentItemModel>(A<CmsApiSharedContentModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeContentItemDocumentService.UpsertAsync(A<ContentItemModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeJobGroupPublishedRefreshService.ReloadAsync(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeJobGroupPublishedRefreshService.ReloadItemAsync(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeEventGridService.SendEventAsync(A<EventGridEventData>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task WebhooksContentServiceProcessContentForBadUriReturnsInvalidDataException()
        {
            // Arrange
            const string apiEndpoint = "https//:somewhere.com";
            var eventId = Guid.NewGuid();
            var contentId = Guid.NewGuid();

            // Act
            var exceptionResult = await Assert.ThrowsAsync<InvalidDataException>(async () => await webhooksContentService.ProcessContentAsync(false, eventId, contentId, apiEndpoint, MessageContentType.None).ConfigureAwait(false)).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeJobGroupCacheRefreshService.ReloadAsync(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeJobGroupCacheRefreshService.ReloadItemAsync(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeCmsApiService.GetItemAsync<CmsApiSharedContentModel>(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeMapper.Map<ContentItemModel>(A<CmsApiSharedContentModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeContentItemDocumentService.UpsertAsync(A<ContentItemModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeJobGroupPublishedRefreshService.ReloadAsync(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeJobGroupPublishedRefreshService.ReloadItemAsync(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeEventGridService.SendEventAsync(A<EventGridEventData>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();

            Assert.Equal($"Invalid Api url '{apiEndpoint}' received for Event Id: {eventId}", exceptionResult.Message);
        }

        [Fact]
        public async Task WebhooksContentServiceProcessSharedContentForCreateReturnsSuccess()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.OK;

            A.CallTo(() => fakeCmsApiService.GetItemAsync<CmsApiSharedContentModel>(A<Uri>.Ignored)).Returns(A.Dummy<CmsApiSharedContentModel>());
            A.CallTo(() => fakeMapper.Map<ContentItemModel>(A<CmsApiSharedContentModel>.Ignored)).Returns(A.Dummy<ContentItemModel>());
            A.CallTo(() => fakeContentItemDocumentService.UpsertAsync(A<ContentItemModel>.Ignored)).Returns(expectedResult);

            // Act
            var result = await webhooksContentService.ProcessSharedContentAsync(Guid.NewGuid(), new Uri("https://somewhere.com", UriKind.Absolute)).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeCmsApiService.GetItemAsync<CmsApiSharedContentModel>(A<Uri>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeMapper.Map<ContentItemModel>(A<CmsApiSharedContentModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeContentItemDocumentService.UpsertAsync(A<ContentItemModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeJobGroupPublishedRefreshService.ReloadAsync(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeJobGroupPublishedRefreshService.ReloadItemAsync(A<Uri>.Ignored)).MustNotHaveHappened();

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task WebhooksContentServiceProcessSharedContentForCreateReturnsNoContent()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.NoContent;
            ContentItemModel? nullContentItemModel = null;

            A.CallTo(() => fakeCmsApiService.GetItemAsync<CmsApiSharedContentModel>(A<Uri>.Ignored)).Returns(A.Dummy<CmsApiSharedContentModel>());
            A.CallTo(() => fakeMapper.Map<ContentItemModel?>(A<CmsApiSharedContentModel>.Ignored)).Returns(nullContentItemModel);
            A.CallTo(() => fakeContentItemDocumentService.UpsertAsync(A<ContentItemModel>.Ignored)).Returns(expectedResult);

            // Act
            var result = await webhooksContentService.ProcessSharedContentAsync(Guid.NewGuid(), new Uri("https://somewhere.com", UriKind.Absolute)).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeCmsApiService.GetItemAsync<CmsApiSharedContentModel>(A<Uri>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeMapper.Map<ContentItemModel>(A<CmsApiSharedContentModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeContentItemDocumentService.UpsertAsync(A<ContentItemModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeJobGroupPublishedRefreshService.ReloadAsync(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeJobGroupPublishedRefreshService.ReloadItemAsync(A<Uri>.Ignored)).MustNotHaveHappened();

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task WebhooksContentServicePostPublishedEventAsyncReturnsSuccessfully()
        {
            // Arrange

            // Act
            await webhooksContentService.PostPublishedEventAsync("hello world", new Uri("https://somewhere.com", UriKind.Absolute), Guid.NewGuid()).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeEventGridService.SendEventAsync(A<EventGridEventData>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
        }
    }
}
