using AutoMapper;
using DFC.App.JobGroups.Data;
using DFC.App.JobGroups.Data.Models;
using DFC.App.JobGroups.Data.Models.CmsApiModels;
using DFC.App.JobGroups.Data.Models.ContentModels;
using DFC.App.JobGroups.Data.Models.JobGroupModels;
using DFC.App.JobGroups.ViewModels;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

namespace DFC.App.JobGroups.AutoMapperProfiles
{
    [ExcludeFromCodeCoverage]
    public class JobGroupProfile : Profile
    {
        private const string NcsPageTitle = "National Careers Service";

        public JobGroupProfile()
        {
            CreateMap<CmsApiSharedContentModel, ContentItemModel>()
                .ForMember(d => d.Id, s => s.MapFrom(a => a.ItemId))
                .ForMember(d => d.Etag, s => s.Ignore())
                .ForMember(d => d.ParentId, s => s.Ignore())
                .ForMember(d => d.TraceId, s => s.Ignore())
                .ForMember(d => d.ContentType, s => s.MapFrom(a => Constants.ContentTypeSharedContent))
                .ForMember(d => d.PartitionKey, s => s.MapFrom(a => Constants.ContentTypeSharedContent))
                .ForMember(d => d.LastReviewed, s => s.MapFrom(a => a.Published))
                .ForMember(d => d.ContentItems, s => s.Ignore())
                .ForMember(d => d.LastCached, s => s.Ignore());

            CreateMap<JobGroupModel, IndexDocumentViewModel>();

            CreateMap<JobGroupModel, DocumentViewModel>()
                .ForMember(d => d.HtmlHead, s => s.MapFrom(a => a))
                .ForMember(d => d.Breadcrumb, s => s.Ignore())
                .ForMember(d => d.BodyViewModel, s => s.MapFrom(a => a));

            CreateMap<JobGroupModel, HtmlHeadViewModel>()
                .ForMember(d => d.CanonicalUrl, s => s.Ignore())
                .ForMember(d => d.Title, s => s.MapFrom(a => !string.IsNullOrWhiteSpace(a.Title) ? a.Title + " | " + NcsPageTitle : NcsPageTitle))
                .ForMember(d => d.Description, s => s.MapFrom(a => a.Description))
                .ForMember(d => d.Keywords, s => s.Ignore());

            CreateMap<JobGroupModel, BodyViewModel>()
                .ForMember(d => d.SourceDateString, s => s.MapFrom(a => a.TransformedDate.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture)))
                .ForMember(d => d.EmploymentByRegion, s => s.MapFrom(a => a.EmploymentByRegion.FirstOrDefault()))
                .ForMember(d => d.TopIndustriesInJobGroup, s => s.MapFrom(a => a.TopIndustriesInJobGroup.FirstOrDefault()));

            CreateMap<JobGroupModel, SideBarRightViewModel>()
                .ForMember(d => d.SharedContent, s => s.Ignore());

            CreateMap<JobProfileModel, JobProfileViewModel>();

            CreateMap<JobGrowthPredictionModel, JobGrowthViewModel>()
                .ForMember(d => d.JobsCreatedAbsoluteValue, s => s.MapFrom(a => Math.Abs(a.JobsCreated)))
                .ForMember(d => d.GraphicClassName, s => s.MapFrom(a => a.JobsCreated >= 0 ? "job-growth" : "job-decline"))
                .ForMember(d => d.GrowthDeclineString, s => s.MapFrom(a => a.JobsCreated >= 0 ? "Job growth" : "Job decline"))
                .ForMember(d => d.NewOrLostString, s => s.MapFrom(a => a.JobsCreated >= 0 ? "new" : "fewer"));

            CreateMap<QualificationLevelModel, QualificationLevelViewModel>()
                .ForMember(d => d.LevelNumberFromName, s => s.MapFrom(a => !string.IsNullOrWhiteSpace(a.Name) ? a.Name.Split(" ", StringSplitOptions.RemoveEmptyEntries).Last() : default));

            CreateMap<BreakdownModel, BreakdownViewModel>()
                .ForMember(d => d.PredictedEmployment, s => s.MapFrom(a => a.PredictedEmployment.FirstOrDefault()));

            CreateMap<BreakdownYearModel, BreakdownYearViewModel>();

            CreateMap<BreakdownYearValueModel, BreakdownYearValueViewModel>();

            CreateMap<JobGroupModel, JobGroupSummaryItemModel>();
        }
    }
}
