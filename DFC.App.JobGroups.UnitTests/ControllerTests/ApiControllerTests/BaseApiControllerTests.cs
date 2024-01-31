using AutoMapper;
using DFC.App.Jobgroups.Controllers;
using DFC.App.JobGroups.Data.Models.JobGroupModels;
using DFC.Compui.Cosmos.Contracts;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Mime;

namespace DFC.App.JobGroups.UnitTests.ControllerTests.ApiControllerTests
{
    public class BaseApiControllerTests
    {
        public BaseApiControllerTests()
        {
            FakeLogger = A.Fake<ILogger<ApiController>>();
            FakeJobGroupDocumentService = A.Fake<IDocumentService<JobGroupModel>>();
            FakeMapper = A.Fake<IMapper>();
        }

        public static IEnumerable<object[]> HtmlMediaTypes => new List<object[]>
        {
            new string[] { "*/*" },
            new string[] { MediaTypeNames.Text.Html },
        };

        public static IEnumerable<object[]> InvalidMediaTypes => new List<object[]>
        {
            new string[] { MediaTypeNames.Text.Plain },
        };

        public static IEnumerable<object[]> JsonMediaTypes => new List<object[]>
        {
            new string[] { MediaTypeNames.Application.Json },
        };

        protected ILogger<ApiController> FakeLogger { get; }

        protected IDocumentService<JobGroupModel> FakeJobGroupDocumentService { get; }

        protected IMapper FakeMapper { get; }

        protected ApiController BuildApiController()
        {
            var httpContext = new DefaultHttpContext();

            var controller = new ApiController(FakeLogger, FakeMapper, FakeJobGroupDocumentService)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext,
                },
            };

            return controller;
        }
    }
}
