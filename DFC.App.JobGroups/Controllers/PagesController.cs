using DFC.App.JobGroups.Data;
using DFC.App.JobGroups.Data.Models.ContentModels;
using DFC.App.JobGroups.Data.Models.JobGroupModels;
using DFC.App.JobGroups.Extensions;
using DFC.App.JobGroups.Models;
using DFC.App.JobGroups.ViewModels;
using DFC.Compui.Cosmos.Contracts;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
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
        public const string DefaultPageTitleSuffix = "Job group LMI | National Careers Service";
        public const string PageTitleSuffix = " | " + DefaultPageTitleSuffix;

        private readonly ILogger<PagesController> logger;
        private readonly AutoMapper.IMapper mapper;
        private readonly IDocumentService<JobGroupModel> jobGroupDocumentService;
        private readonly IDocumentService<ContentItemModel> sharedContentDocumentService;

        public PagesController(
            ILogger<PagesController> logger,
            AutoMapper.IMapper mapper,
            IDocumentService<JobGroupModel> jobGroupDocumentService,
            IDocumentService<ContentItemModel> sharedContentDocumentService)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.jobGroupDocumentService = jobGroupDocumentService;
            this.sharedContentDocumentService = sharedContentDocumentService;
        }

        [HttpGet]
        [Route("/")]
        [Route("pages")]
        public async Task<IActionResult> Index()
        {
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
            var jobGroupModel = await jobGroupDocumentService.GetAsync(w => w.Soc == soc, soc.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(false);

            if (jobGroupModel != null)
            {
                var viewModel = mapper.Map<DocumentViewModel>(jobGroupModel);

                viewModel.Breadcrumb = BuildBreadcrumb(LocalPath, null, jobGroupModel.Title);
                viewModel.HtmlHead.CanonicalUrl = new Uri($"{Request.GetBaseAddress()}{LocalPath}/{jobGroupModel.Soc}", UriKind.RelativeOrAbsolute);

                logger.LogInformation($"{nameof(Document)} has succeeded for: {soc}");

                return this.NegotiateContentResult(viewModel);
            }

            logger.LogWarning($"{nameof(Document)} has returned no content for: {soc}");

            return NoContent();
        }

        [HttpGet]
        [Route("pages/{soc}/htmlhead")]
        [Route("pages/{soc}/{fromJobProfileCanonicalName}/htmlhead")]
        public async Task<IActionResult> HtmlHead(SocRequestModel socRequest)
        {
            var jobGroupModel = await jobGroupDocumentService.GetAsync(w => w.Soc == socRequest.Soc, socRequest.Soc.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(false);

            if (jobGroupModel != null)
            {
                var viewModel = mapper.Map<HtmlHeadViewModel>(jobGroupModel);
                viewModel.CanonicalUrl = new Uri($"{Request.GetBaseAddress()}{RegistrationPath}/{jobGroupModel.Soc}", UriKind.RelativeOrAbsolute);

                logger.LogInformation($"{nameof(HtmlHead)} has succeeded for: {socRequest.Soc}");

                return this.NegotiateContentResult(viewModel);
            }

            logger.LogWarning($"{nameof(HtmlHead)} has returned no content for: {socRequest.Soc}");

            return NoContent();
        }

        [HttpGet]
        [Route("pages/{soc}/breadcrumb")]
        [Route("pages/{soc}/{fromJobProfileCanonicalName}/breadcrumb")]
        public async Task<IActionResult> Breadcrumb(SocRequestModel socRequest)
        {
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

                var sharedContentAskAdviser = await sharedContentDocumentService.GetByIdAsync(Guid.Parse(Constants.SharedContentAskAdviserItemId)).ConfigureAwait(false);

                viewModel.SharedContent = new SharedContentViewModel
                {
                    Markup = new HtmlString(sharedContentAskAdviser?.Content),
                };

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