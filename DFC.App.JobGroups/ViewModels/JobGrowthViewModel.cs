﻿using System.Diagnostics.CodeAnalysis;

namespace DFC.App.JobGroups.ViewModels
{
    [ExcludeFromCodeCoverage]
    public class JobGrowthViewModel
    {
        public int StartYearRange { get; set; }

        public int EndYearRange { get; set; }

        public int JobsCreated { get; set; }

        public decimal PercentageGrowth { get; set; }

        public string? GraphicClassName { get; set; }

        public string? GrowthDeclineString { get; set; }

        public string? NewOrLostString { get; set; }

        public int? Retirements { get; set; }

        public decimal? PercentageRetirements { get; set; }
    }
}
