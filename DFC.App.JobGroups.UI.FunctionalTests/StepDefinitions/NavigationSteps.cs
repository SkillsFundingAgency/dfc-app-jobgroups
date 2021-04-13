﻿// <copyright file="NavigationSteps.cs" company="National Careers Service">
// Copyright (c) National Careers Service. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using DFC.App.JobGroups.Model;
using DFC.App.JobGroups.UI.FunctionalTests.Pages;
using DFC.TestAutomation.UI.Extension;
using OpenQA.Selenium;
using System;
using System.Globalization;
using TechTalk.SpecFlow;

namespace DFC.App.JobGroups.UI.FunctionalTests.StepDefinitions
{
    [Binding]
    internal class NavigationSteps
    {
        public NavigationSteps(ScenarioContext context)
        {
            this.Context = context;
        }

        private ScenarioContext Context { get; set; }

        [Given(@"I am on the (.*) job profile page")]
        public void GivenIAmOnThePage(string pageName)
        {
            switch (pageName.ToLower(CultureInfo.CurrentCulture))
            {
                case "nurse":
                    var jobGroupsPage = new JobGroupsPage(this.Context);
                    jobGroupsPage.NavigateToJobGroupsPage();
                    var pageHeadingLocator = By.CssSelector("h1");
                    this.Context.GetHelperLibrary<AppSettings>().WebDriverWaitHelper.WaitForElementToContainText(pageHeadingLocator, "Nurse");
                    break;

                default:
                    throw new OperationCanceledException($"Unable to perform the step: {this.Context.StepContext.StepInfo.Text}. The page name provided was not recognised.");
            }
        }
    }
}
