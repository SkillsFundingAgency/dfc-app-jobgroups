using DFC.App.JobGroups.Controllers;
using DFC.App.JobGroups.Data.Models.JobGroupModels;
using DFC.Common.SharedContent.Pkg.Netcore.Interfaces;
using DFC.Compui.Cosmos.Contracts;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.Collections.Generic;
using System.Net.Mime;

namespace DFC.App.JobGroups.UnitTests.ControllerTests.PagesControllerTests
{
    public abstract class BasePagesControllerTests
    {
        protected BasePagesControllerTests()
        {
            FakeLogger = A.Fake<ILogger<PagesController>>();
            FakeJobGroupDocumentService = A.Fake<IDocumentService<JobGroupModel>>();
            FakeSharedContentRedisInterface = A.Fake<ISharedContentRedisInterface>();
            FakeMapper = A.Fake<AutoMapper.IMapper>();
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

        protected ILogger<PagesController> FakeLogger { get; }

        protected IDocumentService<JobGroupModel> FakeJobGroupDocumentService { get; }

        protected ISharedContentRedisInterface FakeSharedContentRedisInterface { get; }

        protected IConfiguration FakeConfiguration { get; }

        protected AutoMapper.IMapper FakeMapper { get; }

        protected PagesController BuildPagesController(string mediaTypeName)
        {
            var httpContext = new DefaultHttpContext();

            httpContext.Request.Headers[HeaderNames.Accept] = mediaTypeName;

            var controller = new PagesController(FakeLogger, FakeMapper, FakeJobGroupDocumentService, FakeSharedContentRedisInterface, FakeConfiguration)
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
