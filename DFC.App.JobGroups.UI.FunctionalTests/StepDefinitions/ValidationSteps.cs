// <copyright file="ValidationSteps.cs" company="National Careers Service">
// Copyright (c) National Careers Service. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using DFC.App.JobGroups.Model;
using DFC.TestAutomation.UI.Extension;
using OpenQA.Selenium;
using System.Globalization;
using TechTalk.SpecFlow;

namespace DFC.App.JobGroups.UI.FunctionalTests.StepDefinitions
{
    [Binding]
    internal class ValidationSteps
    {
        public ValidationSteps(ScenarioContext context)
        {
            this.Context = context;
        }

        private ScenarioContext Context { get; set; }

        [Then(@"I am on the (.*) page")]
        public void ThenIAmOnThePage(string pageName)
        {
            By locator = null;

            switch (pageName.ToLower(CultureInfo.CurrentCulture))
            {
                case "job group: nurses":
                    locator = By.CssSelector("h1");
                    break;

                default:
                    locator = By.CssSelector("h1");
                    break;
            }

            this.Context.GetHelperLibrary<AppSettings>().WebDriverWaitHelper.WaitForElementToContainText(locator, pageName);
        }

        [Then(@"the (.*) information is displayed")]
        public void ThenTheJobGrowthInformationIsDisplayed(string lMI)
        {
            By locator = null;

            switch (lMI.ToLower(CultureInfo.CurrentCulture))
            {
                case "job growth":
                    locator = By.CssSelector(".dfc-app-lmi-panel.panel-green");
                    break;

                case "qualifications":
                    locator = By.CssSelector(".dfc-app-lmi-panel.panel-green.panel-qualifications");
                    break;

                case "regional":
                    locator = By.XPath("//*[@id='main-content']/div/div/div/div[1]/div/table[2]/thead/tr/th[1]");
                    break;

                case "industry":
                    locator = By.XPath("//*[@id='main-content']/div/div/div/div[1]/div/table[1]/thead/tr/th[1]");
                    break;

                default:
                    locator = By.CssSelector("h1");
                    break;
            }

            this.Context.GetHelperLibrary<AppSettings>().WebDriverWaitHelper.WaitForElementToBeDisplayed(locator);
        }
    }
}