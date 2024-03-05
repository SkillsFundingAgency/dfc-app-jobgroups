using DFC.App.JobGroups.Controllers;
using DFC.App.JobGroups.Data.Models.JobGroupModels;
using DFC.App.JobGroups.ViewModels;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using Xunit;

namespace DFC.App.JobGroups.UnitTests.ControllerTests.PagesControllerTests
{
    [Trait("Category", "Pages Controller - Route Unit Tests")]
    public class PagesControllerRouteTests : BasePagesControllerTests
    {
        public static IEnumerable<object[]> PagesRouteDataOk => new List<object[]>
        {
            new object[] { "/", 0, nameof(PagesController.Index) },
            new object[] { "/pages", 0, nameof(PagesController.Index) },
            new object[] { "/pages/{soc}/document", 3231, nameof(PagesController.Document) },
        };

        [Theory]
        [MemberData(nameof(PagesRouteDataOk))]
        public async Task PagesControllerCallsContentPageServiceUsingPagesRouteForOkResult(string route, int soc, string actionMethod)
        {
            // Arrange
            var controller = BuildController(route);
            var expectedResult = new JobGroupModel() { Title = "A title", };
            var expectedResults = new List<JobGroupModel> { expectedResult };
            var expectedViewModel = new DocumentViewModel { Title = "A title", Head = A.Dummy<HeadViewModel>(), };
            A.CallTo(() => FakeJobGroupDocumentService.GetAllAsync(A<string>.Ignored)).Returns(expectedResults);
            A.CallTo(() => FakeJobGroupDocumentService.GetByIdAsync(A<Guid>.Ignored, A<string>.Ignored)).Returns(expectedResult);
            A.CallTo(() => FakeMapper.Map<DocumentViewModel>(A<JobGroupModel>.Ignored)).Returns(expectedViewModel);

            // Act
            var result = await RunControllerAction(controller, soc, actionMethod).ConfigureAwait(false);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            A.CallTo(() => FakeJobGroupDocumentService.GetAllAsync(A<string>.Ignored)).MustHaveHappenedOnceOrLess();
            A.CallTo(() => FakeJobGroupDocumentService.GetByIdAsync(A<Guid>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceOrLess();

            controller.Dispose();
        }

        private static async Task<IActionResult> RunControllerAction(PagesController controller, int soc, string actionName)
        {
            return actionName switch
            {
                nameof(PagesController.Document) => await controller.Document(soc).ConfigureAwait(false),
                _ => await controller.Index().ConfigureAwait(false),
            };
        }

        private PagesController BuildController(string route)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = route;
            httpContext.Request.Headers[HeaderNames.Accept] = MediaTypeNames.Application.Json;

            return new PagesController(FakeLogger, FakeMapper, FakeJobGroupDocumentService, FakeSharedContentRedisInterface, FakeConfiguration)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext,
                },
            };
        }
    }
}