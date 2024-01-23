using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DFC.App.JobGroups.ViewModels
{
    [ExcludeFromCodeCoverage]
    public class SideBarRightViewModel
    {
        public List<JobProfileViewModel>? JobProfiles { get; set; }

        public string? SharedContent { get; set; }
    }
}
