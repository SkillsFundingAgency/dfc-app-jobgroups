using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace DFC.App.JobGroups.ViewModels
{
    [ExcludeFromCodeCoverage]
    public class DocumentViewModel
    {
        public HeadViewModel Head { get; set; } = new HeadViewModel();

        public BreadcrumbViewModel? Breadcrumb { get; set; }

        [Display(Name = "Document Id")]
        public Guid Id { get; set; }

        public int Soc { get; set; }

        public string? Title { get; set; }

        public string? Qualifications { get; set; }

        public string? Tasks { get; set; }

        [Display(Name = "Partition Key")]
        public string? PartitionKey { get; set; }

        [Display(Name = "Transformed Date")]
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy HH:mm:ss}")]
        public DateTime TransformedDate { get; set; }

        [Display(Name = "Last Cached")]
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy HH:mm:ss}")]
        public DateTime LastCached { get; set; }

        public BodyViewModel? BodyViewModel { get; set; }
    }
}
