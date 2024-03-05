using DFC.App.JobGroups.Data.Models.JobGroupModels;
using DFC.App.JobGroups.Extensions;
using DFC.App.JobGroups.Models;
using DFC.App.JobGroups.ViewModels;
using DFC.Common.SharedContent.Pkg.Netcore.Interfaces;
using DFC.Common.SharedContent.Pkg.Netcore.Model.ContentItems.SharedHtml;
using DFC.Compui.Cosmos.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace DFC.App.JobGroups.Controllers
{
    public class PagesController : Controller
    {
        public const string RegistrationPath = "job-groups";
        public const string LocalPath = "pages";
        public const string SharedContentStaxId = "2c9da1b3-3529-4834-afc9-9cd741e59788";

        private readonly ILogger<PagesController> logger;
        private readonly AutoMapper.IMapper mapper;
        private readonly IDocumentService<JobGroupModel> jobGroupDocumentService;
        private readonly ISharedContentRedisInterface sharedContentRedis;
        private readonly IConfiguration configuration;
        private string status;

        public PagesController(
            ILogger<PagesController> logger,
            AutoMapper.IMapper mapper,
            IDocumentService<JobGroupModel> jobGroupDocumentService,
            ISharedContentRedisInterface sharedContentRedis,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.jobGroupDocumentService = jobGroupDocumentService;
            this.sharedContentRedis = sharedContentRedis;
            this.configuration = configuration;

            status = this.configuration.GetConnectionString("ContentMode:ContentMode");

            if (string.IsNullOrEmpty(status))
            {
                status = "PUBLISHED";
            }
        }

        [HttpGet]
        [Route("/")]
        [Route("pages")]
        public async Task<IActionResult> Index()
        {
            logger.LogInformation($"{nameof(Index)} has been called");

            var viewModel = new IndexViewModel()
            {
                Path = LocalPath,
                Documents = new List<IndexDocumentViewModel>()
                {
                    new IndexDocumentViewModel { Title = HealthController.HealthViewCanonicalName },
                    new IndexDocumentViewModel { Title = SitemapController.SitemapViewCanonicalName },
                    new IndexDocumentViewModel { Title = RobotController.RobotsViewCanonicalName },
                },
            };
            var jobGroupModels = await jobGroupDocumentService.GetAllAsync().ConfigureAwait(false);

            if (jobGroupModels != null)
            {
                var documents = from a in jobGroupModels.OrderBy(o => o.Soc)
                                select mapper.Map<IndexDocumentViewModel>(a);

                viewModel.Documents.AddRange(documents);

                logger.LogInformation($"{nameof(Index)} has succeeded");
            }
            else
            {
                logger.LogWarning($"{nameof(Index)} has returned with no results");
            }

            return this.NegotiateContentResult(viewModel);
        }

        [HttpGet]
        [Route("pages/{soc}/document")]
        public async Task<IActionResult> Document(int soc)
        {
            logger.LogInformation($"{nameof(Document)} has been called for: {soc}");

            var jobGroupModel = await jobGroupDocumentService.GetAsync(w => w.Soc == soc, soc.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(false);

            if (jobGroupModel != null)
            {
                var viewModel = mapper.Map<DocumentViewModel>(jobGroupModel);

                viewModel.Breadcrumb = BuildBreadcrumb(LocalPath, null, jobGroupModel.Title);
                viewModel.Head.CanonicalUrl = new Uri($"{Request.GetBaseAddress()}{LocalPath}/{jobGroupModel.Soc}", UriKind.RelativeOrAbsolute);

                logger.LogInformation($"{nameof(Document)} has succeeded for: {soc}");

                return this.NegotiateContentResult(viewModel);
            }

            logger.LogWarning($"{nameof(Document)} has returned no content for: {soc}");

            return NoContent();
        }

        [HttpGet]
        [Route("pages/{soc}/head")]
        [Route("pages/{soc}/{fromJobProfileCanonicalName}/head")]
        public async Task<IActionResult> Head(SocRequestModel socRequest)
        {
            logger.LogInformation($"{nameof(Head)} has been called for: {socRequest}");

            var jobGroupModel = await jobGroupDocumentService.GetAsync(w => w.Soc == socRequest.Soc, socRequest.Soc.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(false);

            if (jobGroupModel != null)
            {
                var viewModel = mapper.Map<HeadViewModel>(jobGroupModel);
                viewModel.CanonicalUrl = new Uri($"{Request.GetBaseAddress()}{RegistrationPath}/{jobGroupModel.Soc}", UriKind.RelativeOrAbsolute);

                logger.LogInformation($"{nameof(Head)} has succeeded for: {socRequest.Soc}");

                return this.NegotiateContentResult(viewModel);
            }

            logger.LogWarning($"{nameof(Head)} has returned no content for: {socRequest.Soc}");

            return NoContent();
        }

        [HttpGet]
        [Route("pages/{soc}/breadcrumb")]
        [Route("pages/{soc}/{fromJobProfileCanonicalName}/breadcrumb")]
        public async Task<IActionResult> Breadcrumb(SocRequestModel socRequest)
        {
            logger.LogInformation($"{nameof(Breadcrumb)} has been called for: {socRequest}");

            var jobGroupModel = await jobGroupDocumentService.GetAsync(w => w.Soc == socRequest.Soc, socRequest.Soc.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(false);

            if (jobGroupModel != null)
            {
                BreadcrumbItemModel? breadcrumbItemModel = default;
                if (!string.IsNullOrWhiteSpace(socRequest.FromJobProfileCanonicalName) && jobGroupModel.JobProfiles != null && jobGroupModel.JobProfiles.Any())
                {
                    breadcrumbItemModel = new BreadcrumbItemModel()
                    {
                        Route = "/job-profiles/" + socRequest.FromJobProfileCanonicalName,
                        Title = jobGroupModel.JobProfiles.FirstOrDefault(f => !string.IsNullOrWhiteSpace(f.CanonicalName) && f.CanonicalName.Equals(socRequest.FromJobProfileCanonicalName, System.StringComparison.OrdinalIgnoreCase))?.Title,
                    };
                }

                var viewModel = BuildBreadcrumb(RegistrationPath, breadcrumbItemModel, jobGroupModel.Title);

                logger.LogInformation($"{nameof(Breadcrumb)} has succeeded for: {socRequest.Soc}");

                return this.NegotiateContentResult(viewModel);
            }

            logger.LogWarning($"{nameof(Breadcrumb)} has returned no content for: {socRequest.Soc}");

            return NoContent();
        }

        [HttpGet]
        [Route("pages/{soc}/body")]
        [Route("pages/{soc}/{fromJobProfileCanonicalName}/body")]
        public async Task<IActionResult> Body(SocRequestModel socRequest)
        {
            logger.LogInformation($"{nameof(Body)} has been called for: {socRequest}");

            var jobGroupModel = await jobGroupDocumentService.GetAsync(w => w.Soc == socRequest.Soc, socRequest.Soc.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(false);

            if (jobGroupModel != null)
            {
                var viewModel = mapper.Map<BodyViewModel>(jobGroupModel);

                logger.LogInformation($"{nameof(Body)} has succeeded for: {socRequest.Soc}");

                return this.NegotiateContentResult(viewModel);
            }

            logger.LogWarning($"{nameof(Body)} has returned no content for: {socRequest.Soc}");

            return NotFound();
        }

        [HttpGet]
        [Route("pages/{soc}/sidebarright")]
        [Route("pages/{soc}/{fromJobProfileCanonicalName}/sidebarright")]
        public async Task<IActionResult> SideBarRight(SocRequestModel socRequest)
        {
            logger.LogInformation($"{nameof(SideBarRight)} has been called for: {socRequest}");

            var jobGroupModel = await jobGroupDocumentService.GetAsync(w => w.Soc == socRequest.Soc, socRequest.Soc.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(false);

            if (jobGroupModel != null)
            {
                var viewModel = mapper.Map<SideBarRightViewModel>(jobGroupModel);

                if (!string.IsNullOrWhiteSpace(socRequest.FromJobProfileCanonicalName) && viewModel.JobProfiles != null && viewModel.JobProfiles.Any())
                {
                    var jobProfile = viewModel.JobProfiles.FirstOrDefault(f => !string.IsNullOrWhiteSpace(f.CanonicalName) && f.CanonicalName.Equals(socRequest.FromJobProfileCanonicalName, System.StringComparison.OrdinalIgnoreCase));
                    if (jobProfile != null)
                    {
                        viewModel.JobProfiles.Remove(jobProfile);
                    }
                }

                try
                {
                    var sharedhtml = await sharedContentRedis.GetDataAsync<SharedHtml>("SharedContent/" + SharedContentStaxId, status);

                    viewModel.SharedContent = sharedhtml.Html;
                }
                catch (Exception e)
                {
                    viewModel.SharedContent = "<h1> Error Retrieving Data from Redis<h1><p>" + e.ToString() + "</p>";
                }

                logger.LogInformation($"{nameof(SideBarRight)} has succeeded for: {socRequest.Soc}");

                return this.NegotiateContentResult(viewModel);
            }

            logger.LogWarning($"{nameof(SideBarRight)} has returned no content for: {socRequest.Soc}");

            return NoContent();
        }

        private static BreadcrumbViewModel BuildBreadcrumb(string segmentPath, BreadcrumbItemModel? jpBreadcrumbItemModel, string jobGroupTitle)
        {
            var viewModel = new BreadcrumbViewModel
            {
                Breadcrumbs = new List<BreadcrumbItemViewModel>()
                {
                    new BreadcrumbItemViewModel()
                    {
                        Route = "/",
                        Title = "Home",
                    },
                },
            };

            if (jpBreadcrumbItemModel?.Title != null &&
                !string.IsNullOrWhiteSpace(jpBreadcrumbItemModel.Route))
            {
                var articlePathViewModel = new BreadcrumbItemViewModel
                {
                    Route = jpBreadcrumbItemModel.Route,
                    Title = jpBreadcrumbItemModel.Title,
                };

                viewModel.Breadcrumbs.Add(articlePathViewModel);
            }

            var finalPathViewModel = new BreadcrumbItemViewModel
            {
                Route = $"/{segmentPath}",
                Title = $"Job group: {jobGroupTitle}",
            };

            viewModel.Breadcrumbs.Add(finalPathViewModel);

            viewModel.Breadcrumbs.Last().AddHyperlink = false;

            return viewModel;
        }
    }
}