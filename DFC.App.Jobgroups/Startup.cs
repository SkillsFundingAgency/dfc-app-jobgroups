using AutoMapper;
using DFC.App.JobGroups.Data.Contracts;
using DFC.App.JobGroups.Data.Models.ClientOptions;
using DFC.App.JobGroups.Data.Models.JobGroupModels;
using DFC.App.JobGroups.Services.CacheContentService.Connectors;
using DFC.Common.SharedContent.Pkg.Netcore;
using DFC.Common.SharedContent.Pkg.Netcore.Infrastructure;
using DFC.Common.SharedContent.Pkg.Netcore.Infrastructure.Strategy;
using DFC.Common.SharedContent.Pkg.Netcore.Interfaces;
using DFC.Common.SharedContent.Pkg.Netcore.Model.ContentItems.SharedHtml;
using DFC.Common.SharedContent.Pkg.Netcore.RequestHandler;
using DFC.Compui.Cosmos;
using DFC.Compui.Cosmos.Contracts;
using DFC.Compui.Subscriptions.Pkg.Netstandard.Extensions;
using DFC.Compui.Telemetry;
using DFC.Content.Pkg.Netcore.Data.Models.PollyOptions;
using DFC.Content.Pkg.Netcore.Extensions;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

namespace DFC.App.JobGroups
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        private const string AppSettingsPolicies = "Policies";
        private const string CosmosDbJobGroupConfigAppSettings = "Configuration:CosmosDbConnections:JobGroup";
        private const string RedisCacheConnectionStringAppSettings = "Cms:RedisCacheConnectionString";
        private const string GraphApiUrlAppSettings = "Cms:GraphApiUrl";

        private readonly IConfiguration configuration;
        private readonly IWebHostEnvironment env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            this.configuration = configuration;
            this.env = env;
        }

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env, IMapper mapper)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();

                // add the default route
                endpoints.MapControllerRoute("default", "{controller=Health}/{action=Ping}");
            });
            mapper?.ConfigurationProvider.AssertConfigurationIsValid();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var cosmosDbConnectionJobGroup = configuration.GetSection(CosmosDbJobGroupConfigAppSettings).Get<CosmosDbConnection>() ?? throw new ArgumentNullException();
            var cosmosRetryOptions = new RetryOptions { MaxRetryAttemptsOnThrottledRequests = 20, MaxRetryWaitTimeInSeconds = 60 };
            services.AddDocumentServices<JobGroupModel>(cosmosDbConnectionJobGroup, env.IsDevelopment(), cosmosRetryOptions);
            services.AddStackExchangeRedisCache(options => { options.Configuration = configuration.GetSection(RedisCacheConnectionStringAppSettings).Get<string>(); });

            services.AddHttpClient();
            services.AddSingleton<IGraphQLClient>(s =>
            {
                var option = new GraphQLHttpClientOptions()
                {
                    EndPoint = new Uri(configuration.GetSection(GraphApiUrlAppSettings).Get<string>() ?? throw new ArgumentNullException()),
                    HttpMessageHandler = new CmsRequestHandler(s.GetService<IHttpClientFactory>(), s.GetService<IConfiguration>(), s.GetService<IHttpContextAccessor>()),
                };
                var client = new GraphQLHttpClient(option, new NewtonsoftJsonSerializer());
                return client;
            });

            services.AddSingleton<ISharedContentRedisInterfaceStrategy<SharedHtml>, SharedHtmlQueryStrategy>();

            services.AddSingleton<ISharedContentRedisInterfaceStrategyFactory, SharedContentRedisStrategyFactory>();

            services.AddScoped<ISharedContentRedisInterface, SharedContentRedis>();

            services.AddApplicationInsightsTelemetry();
            services.AddHttpContextAccessor();
            services.AddHostedServiceTelemetryWrapper();
            services.AddAutoMapper(typeof(Startup).Assembly);
            services.AddSubscriptionBackgroundService(configuration);
            services.AddTransient<IApiConnector, ApiConnector>();
            services.AddTransient<IApiDataConnector, ApiDataConnector>();

            var policyOptions = configuration.GetSection(AppSettingsPolicies).Get<PolicyOptions>() ?? new PolicyOptions();
            var policyRegistry = services.AddPolicyRegistry();

            services.AddSingleton(configuration.GetSection(nameof(LmiTransformationApiClientOptions)).Get<LmiTransformationApiClientOptions>() ?? new LmiTransformationApiClientOptions());
            services.AddSingleton(configuration.GetSection(nameof(JobGroupDraftApiClientOptions)).Get<JobGroupDraftApiClientOptions>() ?? new JobGroupDraftApiClientOptions());
            services.AddApiServices(configuration, policyRegistry);

            services
                .AddPolicies(policyRegistry, nameof(LmiTransformationApiClientOptions), policyOptions)
                .AddHttpClient<ILmiTransformationApiConnector, LmiTransformationApiConnector, LmiTransformationApiClientOptions>(nameof(LmiTransformationApiClientOptions), nameof(PolicyOptions.HttpRetry), nameof(PolicyOptions.HttpCircuitBreaker));

            services
                .AddPolicies(policyRegistry, nameof(JobGroupDraftApiClientOptions), policyOptions)
                .AddHttpClient<IJobGroupApiConnector, JobGroupApiConnector, JobGroupDraftApiClientOptions>(nameof(JobGroupDraftApiClientOptions), nameof(PolicyOptions.HttpRetry), nameof(PolicyOptions.HttpCircuitBreaker));

            services.AddMvc(config =>
                {
                    config.RespectBrowserAcceptHeader = true;
                    config.ReturnHttpNotAcceptable = true;
                })
                .AddNewtonsoftJson();
        }
    }
}