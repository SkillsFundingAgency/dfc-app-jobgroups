# dfc-app-jobgroups

## Introduction

This project provides a Job Groups app for use in the Composite UI (Shell application) to dynamically output markup from Job Groups data sources (originating from LMI For All data source).

Details of the Composite UI application may be found here [https://github.com/SkillsFundingAgency/dfc-composite-shell](https://github.com/SkillsFundingAgency/dfc-composite-shell)

This Job Groups app returns:

* Job Group documents

The Job Groups app also provisions the following for consumption by the Composite UI:

* Sitemap.xml for all Job Groups documents
* Robots.txt

## Getting started

This is a self-contained Visual Studio 2019 solution containing a number of projects (web application, service and repository layers, with associated unit test and integration test projects).

### Installing

Clone the project and open the solution in Visual Studio 2019.

## List of dependencies

|Item|Purpose|
|----|-------|
|Azure Cosmos DB|Document storage |
|DFC.Compui.Cosmos|Cosmos DB interface|
|DFC.Compui.Subscriptions|Subscriptions API client|
|DFC.Compui.Telemetry|Telemetry|
|DFC.Content.Pkg.Netcore|Content API client|

## Local Config Files

Once you have cloned the public repo you need to create app settings files from the configuration files listed below.

|Location|Filename|Rename to|
|--------|--------|---------|
|DFC.App.Jobgroups|appsettings-template.json|appsettings.json|
|DFC.App.JobGroups.UI.FunctionalTests|appsettings-template.json|appsettings.json|

## Configuring to run locally

The project contains *appsettings-template.json* files which contains Job Groups appsettings for the web app. To use these files, copy them to *appsettings.json* within each project and edit and replace the configuration item values with values suitable for your environment.

By default, the appsettings include local Azure Cosmos Emulator configurations using the well known Cosmos configuration values for Job Groups data and shared content storage (in separate collections). These may be changed to suit your environment if you are not using the Azure Cosmos Emulator.

## Running locally

To run this product locally, you will need to configure the list of dependencies, once configured and the configuration files updated, it should be F5 to run and debug locally. The application can be run using IIS Express or full IIS.

To run the project, start the web application. Once running, browse to the main entry point which is the [https://localhost:44310/](https://localhost:44310/). This will show the first of the Job Groups views.

The Job Groups app is designed to be run from within the Composite UI, therefore running the Job Groups app outside of the Composite UI will only show simple views of the data.

## Deployments

This Job Groups app will be deployed as an individual deployment for consumption by the Composite UI.

## Assets

CSS, JS, images and fonts used in this site can found in the following repository [https://github.com/SkillsFundingAgency/dfc-digital-assets](https://github.com/SkillsFundingAgency/dfc-digital-assets)

## Built with

* Microsoft Visual Studio 2019
* .Net Core 3.1

## References

Please refer to [https://github.com/SkillsFundingAgency/dfc-digital](https://github.com/SkillsFundingAgency/dfc-digital) for additional instructions on configuring individual components like Cosmos.
