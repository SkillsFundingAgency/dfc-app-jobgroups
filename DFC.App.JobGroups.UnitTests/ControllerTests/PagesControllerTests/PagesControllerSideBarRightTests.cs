using DFC.App.JobGroups.Data.Models.ContentModels;
using DFC.App.JobGroups.Data.Models.JobGroupModels;
using DFC.App.JobGroups.Models;
using DFC.App.JobGroups.ViewModels;
using DFC.Common.SharedContent.Pkg.Netcore.Model.ContentItems.SharedHtml;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace DFC.App.JobGroups.UnitTests.ControllerTests.PagesControllerTests
{
    [Trait("Category", "Pages Controller - SideBarRight Unit Tests")]
    public class PagesControllerSideBarRightTests : BasePagesControllerTests
    {
        [Theory]
        [MemberData(nameof(HtmlMediaTypes))]
        public async Task PagesControllerSideBarRightHtmlReturnsSuccess(string mediaTypeName)
        {
            // Arrange
            var socRequestModel = new SocRequestModel { Soc = 3231, FromJobProfileCanonicalName = "a-job-profile", };
            var dummyJobGroupModel = A.Dummy<JobGroupModel>();
            var dummyContentItemModel = A.Dummy<ContentItemModel>();
            var controller = BuildPagesController(mediaTypeName);
            var dummySideBarRightViewModel = A.Dummy<SideBarRightViewModel>();
            dummySideBarRightViewModel.JobProfiles = new List<JobProfileViewModel> { new JobProfileViewModel { CanonicalName = socRequestModel.FromJobProfileCanonicalName, Title = "A title" }, };

            A.CallTo(() => FakeJobGroupDocumentService.GetAsync(A<Expression<Func<JobGroupModel, bool>>>.Ignored, A<string>.Ignored)).Returns(dummyJobGroupModel);
            A.CallTo(() => FakeMapper.Map<SideBarRightViewModel>(A<JobGroupModel>.Ignored)).Returns(dummySideBarRightViewModel);
            A.CallTo(() => FakeSharedContentRedisInterface.GetDataAsync<SharedHtml>(string.Empty, "PUBLISHED"));

            // Act
            var result = await controller.SideBarRight(socRequestModel).ConfigureAwait(false);

            // Assert
            A.CallTo(() => FakeJobGroupDocumentService.GetAsync(A<Expression<Func<JobGroupModel, bool>>>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => FakeMapper.Map<SideBarRightViewModel>(A<JobGroupModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => FakeSharedContentRedisInterface.GetDataAsync<SharedHtml>(string.Empty, "PUBLISHED"));

            var viewResult = Assert.IsType<ViewResult>(result);
            _ = Assert.IsAssignableFrom<SideBarRightViewModel>(viewResult.ViewData.Model);
            var model = viewResult.ViewData.Model as SideBarRightViewModel;
            Assert.Equal(dummySideBarRightViewModel, model);

            controller.Dispose();
        }

        [Theory]
        [MemberData(nameof(JsonMediaTypes))]
        public async Task PagesControllerSideBarRightJsonReturnsSuccess(string mediaTypeName)
        {
            // Arrange
            var dummyJobGroupModel = A.Dummy<JobGroupModel>();
            var dummyContentItemModel = A.Dummy<ContentItemModel>();
            var controller = BuildPagesController(mediaTypeName);
            var socRequestModel = new SocRequestModel { Soc = 3231, FromJobProfileCanonicalName = "a-job-profile", };
            var dummySideBarRightViewModel = A.Dummy<SideBarRightViewModel>();
            dummySideBarRightViewModel.JobProfiles = new List<JobProfileViewModel> { new JobProfileViewModel { CanonicalName = socRequestModel.FromJobProfileCanonicalName, Title = "A title" }, };

            A.CallTo(() => FakeJobGroupDocumentService.GetAsync(A<Expression<Func<JobGroupModel, bool>>>.Ignored, A<string>.Ignored)).Returns(dummyJobGroupModel);
            A.CallTo(() => FakeMapper.Map<SideBarRightViewModel>(A<JobGroupModel>.Ignored)).Returns(dummySideBarRightViewModel);
            A.CallTo(() => FakeSharedContentRedisInterface.GetDataAsync<SharedHtml>(string.Empty, "PUBLISHED"));

            // Act
            var result = await controller.SideBarRight(socRequestModel).ConfigureAwait(false);

            // Assert
            A.CallTo(() => FakeJobGroupDocumentService.GetAsync(A<Expression<Func<JobGroupModel, bool>>>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => FakeMapper.Map<SideBarRightViewModel>(A<JobGroupModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => FakeSharedContentRedisInterface.GetDataAsync<SharedHtml>(string.Empty, "PUBLISHED"));

            var jsonResult = Assert.IsType<OkObjectResult>(result);
            _ = Assert.IsAssignableFrom<SideBarRightViewModel>(jsonResult.Value);

            controller.Dispose();
        }

        [Theory]
        [MemberData(nameof(JsonMediaTypes))]
        [MemberData(nameof(HtmlMediaTypes))]
        public async Task PagesControllerSideBarRightReturnsNoContentWhenNoData(string mediaTypeName)
        {
            // Arrange
            JobGroupModel? nullJobGroupModel = null;
            var controller = BuildPagesController(mediaTypeName);
            var socRequestModel = new SocRequestModel { Soc = 3231, FromJobProfileCanonicalName = "a-job-profile", };

            A.CallTo(() => FakeJobGroupDocumentService.GetAsync(A<Expression<Func<JobGroupModel, bool>>>.Ignored, A<string>.Ignored)).Returns(nullJobGroupModel);

            // Act
            var result = await controller.SideBarRight(socRequestModel).ConfigureAwait(false);

            // Assert
            A.CallTo(() => FakeJobGroupDocumentService.GetAsync(A<Expression<Func<JobGroupModel, bool>>>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => FakeMapper.Map<SideBarRightViewModel>(A<JobGroupModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => FakeSharedContentRedisInterface.GetDataAsync<SharedHtml>(string.Empty, "PUBLISHED"));

            var statusResult = Assert.IsType<NoContentResult>(result);

            A.Equals((int)HttpStatusCode.NoContent, statusResult.StatusCode);

            controller.Dispose();
        }

        [Theory]
        [MemberData(nameof(InvalidMediaTypes))]
        public async Task PagesControllerSideBarRightReturnsNotAcceptable(string mediaTypeName)
        {
            // Arrange
            var dummyJobGroupModel = A.Dummy<JobGroupModel>();
            var dummyContentItemModel = A.Dummy<ContentItemModel>();
            var controller = BuildPagesController(mediaTypeName);
            var socRequestModel = new SocRequestModel { Soc = 3231, FromJobProfileCanonicalName = "a-job-profile", };
            var dummySideBarRightViewModel = A.Dummy<SideBarRightViewModel>();

            A.CallTo(() => FakeJobGroupDocumentService.GetAsync(A<Expression<Func<JobGroupModel, bool>>>.Ignored, A<string>.Ignored)).Returns(dummyJobGroupModel);
            A.CallTo(() => FakeMapper.Map<SideBarRightViewModel>(A<JobGroupModel>.Ignored)).Returns(dummySideBarRightViewModel);
            A.CallTo(() => FakeSharedContentRedisInterface.GetDataAsync<SharedHtml>(string.Empty, "PUBLISHED"));

            // Act
            var result = await controller.SideBarRight(socRequestModel).ConfigureAwait(false);

            // Assert
            A.CallTo(() => FakeJobGroupDocumentService.GetAsync(A<Expression<Func<JobGroupModel, bool>>>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => FakeMapper.Map<SideBarRightViewModel>(A<JobGroupModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => FakeSharedContentRedisInterface.GetDataAsync<SharedHtml>(string.Empty, "PUBLISHED"   ));

            var statusResult = Assert.IsType<StatusCodeResult>(result);

            A.Equals((int)HttpStatusCode.NotAcceptable, statusResult.StatusCode);

            controller.Dispose();
        }
    }
}
