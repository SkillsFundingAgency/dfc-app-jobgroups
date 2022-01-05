using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace DFC.App.JobGroups.ViewModels
{
    [ExcludeFromCodeCoverage]
    public class HeadViewModel
    {
        public string? Title { get; set; }

        [Display(Name = "Canonical URL")]
        public Uri? CanonicalUrl { get; set; }

        public string? Description { get; set; }

        public string? Keywords { get; set; }
    }
}
