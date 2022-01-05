using DFC.App.JobGroups.Data.Models.JobGroupModels;
using DFC.App.JobGroups.Models;
using DFC.App.JobGroups.ViewModels;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace DFC.App.JobGroups.UnitTests.ControllerTests.PagesControllerTests
{
    [Trait("Category", "Pages Controller - Head Unit Tests")]
    public class PagesControllerHeadTests : BasePagesControllerTests
    {
        [Theory]
        [MemberData(nameof(HtmlMediaTypes))]
        public async Task PagesControllerHeadHtmlReturnsSuccess(string mediaTypeName)
        {
            // Arrange
            var dummyJobGroupModel = A.Dummy<JobGroupModel>();
            var controller = BuildPagesController(mediaTypeName);
            var socRequestModel = new SocRequestModel { Soc = 3231, FromJobProfileCanonicalName = "a-job-profile", };
            var dummyHeadViewModel = A.Dummy<HeadViewModel>();

            A.CallTo(() => FakeJobGroupDocumentService.GetAsync(A<Expression<Func<JobGroupModel, bool>>>.Ignored, A<string>.Ignored)).Returns(dummyJobGroupModel);
            A.CallTo(() => FakeMapper.Map<HeadViewModel>(A<JobGroupModel>.Ignored)).Returns(dummyHeadViewModel);

            // Act
            var result = await controller.Head(socRequestModel).ConfigureAwait(false);

            // Assert
            A.CallTo(() => FakeJobGroupDocumentService.GetAsync(A<Expression<Func<JobGroupModel, bool>>>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => FakeMapper.Map<HeadViewModel>(A<JobGroupModel>.Ignored)).MustHaveHappenedOnceExactly();

            var viewResult = Assert.IsType<ViewResult>(result);
            _ = Assert.IsAssignableFrom<HeadViewModel>(viewResult.ViewData.Model);
            var model = viewResult.ViewData.Model as HeadViewModel;
            Assert.Equal(dummyHeadViewModel, model);

            controller.Dispose();
        }

        [Theory]
        [MemberData(nameof(JsonMediaTypes))]
        public async Task PagesControllerHeadJsonReturnsSuccess(string mediaTypeName)
        {
            // Arrange
            var dummyJobGroupModel = A.Dummy<JobGroupModel>();
            var controller = BuildPagesController(mediaTypeName);
            var socRequestModel = new SocRequestModel { Soc = 3231, FromJobProfileCanonicalName = "a-job-profile", };
            var dummyHeadViewModel = A.Dummy<HeadViewModel>();

            A.CallTo(() => FakeJobGroupDocumentService.GetAsync(A<Expression<Func<JobGroupModel, bool>>>.Ignored, A<string>.Ignored)).Returns(dummyJobGroupModel);
            A.CallTo(() => FakeMapper.Map<HeadViewModel>(A<JobGroupModel>.Ignored)).Returns(dummyHeadViewModel);

            // Act
            var result = await controller.Head(socRequestModel).ConfigureAwait(false);

            // Assert
            A.CallTo(() => FakeJobGroupDocumentService.GetAsync(A<Expression<Func<JobGroupModel, bool>>>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => FakeMapper.Map<HeadViewModel>(A<JobGroupModel>.Ignored)).MustHaveHappenedOnceExactly();

            var jsonResult = Assert.IsType<OkObjectResult>(result);
            _ = Assert.IsAssignableFrom<HeadViewModel>(jsonResult.Value);

            controller.Dispose();
        }

        [Theory]
        [MemberData(nameof(JsonMediaTypes))]
        [MemberData(nameof(HtmlMediaTypes))]
        public async Task PagesControllerHeadReturnsNoContentWhenNoData(string mediaTypeName)
        {
            // Arrange
            JobGroupModel? nullJobGroupModel = null;
            var controller = BuildPagesController(mediaTypeName);
            var socRequestModel = new SocRequestModel { Soc = 3231, FromJobProfileCanonicalName = "a-job-profile", };

            A.CallTo(() => FakeJobGroupDocumentService.GetAsync(A<Expression<Func<JobGroupModel, bool>>>.Ignored, A<string>.Ignored)).Returns(nullJobGroupModel);

            // Act
            var result = await controller.Head(socRequestModel).ConfigureAwait(false);

            // Assert
            A.CallTo(() => FakeJobGroupDocumentService.GetAsync(A<Expression<Func<JobGroupModel, bool>>>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();

            var statusResult = Assert.IsType<NoContentResult>(result);

            A.Equals((int)HttpStatusCode.NoContent, statusResult.StatusCode);

            controller.Dispose();
        }

        [Theory]
        [MemberData(nameof(InvalidMediaTypes))]
        public async Task PagesControllerHeadReturnsNotAcceptable(string mediaTypeName)
        {
            // Arrange
            var dummyJobGroupModel = A.Dummy<JobGroupModel>();
            var controller = BuildPagesController(mediaTypeName);
            var socRequestModel = new SocRequestModel { Soc = 3231, FromJobProfileCanonicalName = "a-job-profile", };
            var dummyHeadViewModel = A.Dummy<HeadViewModel>();

            A.CallTo(() => FakeJobGroupDocumentService.GetAsync(A<Expression<Func<JobGroupModel, bool>>>.Ignored, A<string>.Ignored)).Returns(dummyJobGroupModel);
            A.CallTo(() => FakeMapper.Map<HeadViewModel>(A<JobGroupModel>.Ignored)).Returns(dummyHeadViewModel);

            // Act
            var result = await controller.Head(socRequestModel).ConfigureAwait(false);

            // Assert
            A.CallTo(() => FakeJobGroupDocumentService.GetAsync(A<Expression<Func<JobGroupModel, bool>>>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => FakeMapper.Map<HeadViewModel>(A<JobGroupModel>.Ignored)).MustHaveHappenedOnceExactly();

            var statusResult = Assert.IsType<StatusCodeResult>(result);

            A.Equals((int)HttpStatusCode.NotAcceptable, statusResult.StatusCode);

            controller.Dispose();
        }
    }
}
