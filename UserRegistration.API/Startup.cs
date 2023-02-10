using DbTools;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore.SqlServer;
using Newtonsoft.Json;

using Swashbuckle.AspNetCore.SwaggerGen;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using DbTools.DataBase;
using UserRegistration.API.Common.Attributes;
using UserRegistration.API.Common.Settings;
using UserRegistration.API.Middleware;
using UserRegistration.API.Swagger;
using UserRegistration.IoC.Configuration.DI;

namespace UserRegistration.API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment HostingEnvironment { get; private set; }

        private IConfigurationSection _appsettingsConfigurationSection;
        private AppSettings _appSettings;
        private ApplicationInsights _applicationInsightsSettings;
        private DatabaseConfiguration _dbSettings = default;
        private readonly ILogger _logger;
        private IServiceProvider _serviceProvider;

        public Startup(IConfiguration configuration, IWebHostEnvironment env, IServiceProvider serviceProvider, ILogger<Startup> logger)
        {
            HostingEnvironment = env;
            Configuration = configuration;
            _serviceProvider = serviceProvider;
            _logger = logger;

            //AppSettings
            _appsettingsConfigurationSection = Configuration.GetSection(nameof(AppSettings));
            if (_appsettingsConfigurationSection == null)
                throw new Exception("No appsettings has been found");

            _appSettings = _appsettingsConfigurationSection.Get<AppSettings>();
            _dbSettings = Configuration.GetSection(nameof(DatabaseConfiguration)).Get<DatabaseConfiguration>();

            //Application Insights
            var applicationInsightsConfiturationSection = Configuration.GetSection(nameof(ApplicationInsights));
            if (applicationInsightsConfiturationSection == null)
                throw new Exception("No appsettings has been found");

            _applicationInsightsSettings = applicationInsightsConfiturationSection.Get<ApplicationInsights>();

            _logger.LogDebug("Startup::Constructor::Settings loaded");
        }

        public void ConfigureServices(IServiceCollection services)
        {
            _logger.LogTrace("Startup::ConfigureServices");
            if (_applicationInsightsSettings.Enabled)
            {
                services.AddApplicationInsightsTelemetry();
                _logger.LogTrace("Startup::ConfigureService::Configuring Application Insights");
            }

            try
            {
                if (_appSettings.IsValid())
                {
                    _logger.LogDebug("Startup::ConfigureServices::valid AppSettings");

                    services.Configure<AppSettings>(_appsettingsConfigurationSection);
                    _logger.LogDebug("Startup::ConfigureServices::AppSettings loaded for DI");

                    services.AddControllers(
                        opt =>
                        {
                            //Custom filters, if needed
                            //opt.Filters.Add(typeof(CustomFilterAttribute));
                            opt.Filters.Add(new ProducesAttribute("application/json"));
                        }
                        ).SetCompatibilityVersion(CompatibilityVersion.Latest);

                    //API versioning
                    services.AddApiVersioning(
                        o =>
                        {
                            //o.Conventions.Controller<UserController>().HasApiVersion(1, 0);
                            o.ReportApiVersions = true;
                            o.AssumeDefaultVersionWhenUnspecified = true;
                            o.DefaultApiVersion = new ApiVersion(1, 0);
                            o.ApiVersionReader = new UrlSegmentApiVersionReader();
                        }
                        );

                    // note: the specified format code will format the version as "'v'major[.minor][-status]"
                    services.AddVersionedApiExplorer(
                    options =>
                    {
                        options.GroupNameFormat = "'v'VVV";

                        // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                        // can also be used to control the format of the API version in route userregistrationapis
                        options.SubstituteApiVersionInUrl = true;
                    });


                    //SWAGGER
                    if (_appSettings.Swagger.Enabled)
                    {
                        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

                        services.AddSwaggerGen(options =>
                        {
                            options.OperationFilter<SwaggerDefaultValues>();

                            //1-Get all the assemblies of the project to add the related XML Comments
                            Assembly currentAssembly = Assembly.GetExecutingAssembly();
                            AssemblyName[] referencedAssemblies = currentAssembly.GetReferencedAssemblies();
                            IEnumerable<AssemblyName> allAssemblies = null;

                            if (referencedAssemblies != null && referencedAssemblies.Any())
                                allAssemblies = referencedAssemblies.Union(new AssemblyName[] { currentAssembly.GetName() });
                            else
                                allAssemblies = new AssemblyName[] { currentAssembly.GetName() };

                            IEnumerable<string> xmlDocs = allAssemblies
                                    .Select(a => Path.Combine(Path.GetDirectoryName(currentAssembly.Location), $"{a.Name}.xml"))
                                    .Where(f => File.Exists(f));

                            //2-Add the path to the XML comments for the assemblies having enabled the doc generation
                            if (xmlDocs != null && xmlDocs.Any())
                            {
                                foreach (var item in xmlDocs)
                                {
                                    options.IncludeXmlComments(item);
                                }
                            }
                        });
                    }

                    //Mappings
                    services.ConfigureMappings();

                    //Business settings            
                    services.ConfigureBusinessServices(Configuration);
                    services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(_dbSettings.ConnectionString));
                    services.Configure<DatabaseConfiguration>(Configuration.GetSection(nameof(DatabaseConfiguration)));
                    ServiceBuilder.BuildServices(services);

                    _logger.LogDebug("Startup::ConfigureServices::ApiVersioning, Swagger and DI settings");
                }
                else
                    _logger.LogDebug("Startup::ConfigureServices::invalid AppSettings");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            _logger.LogTrace("Startup::Configure");
            _logger.LogDebug($"Startup::Configure::Environment:{env.EnvironmentName}");

            try
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                    _logger.LogInformation("Developer exception page loaded.");
                }
                else
                {
                    app.ConfigureCustomMiddleware(_appSettings);
                    _logger.LogInformation("Setting not development exception handling settings.");
                    //Both alternatives are usable for general error handling:
                    // - middleware
                    // - UseExceptionHandler()

                    //app.UseMiddleware(typeof(ErrorHandlingMiddleware));

                    app.UseExceptionHandler(a => a.Run(async context =>
                    {
                        var feature = context.Features.Get<IExceptionHandlerPathFeature>();
                        var exception = feature.Error;
                        var code = HttpStatusCode.InternalServerError;

                        if (exception is ArgumentNullException) code = HttpStatusCode.BadRequest;
                        else if (exception is ArgumentException) code = HttpStatusCode.BadRequest;
                        else if (exception is UnauthorizedAccessException) code = HttpStatusCode.Unauthorized;

                        _logger.LogError($"GLOBAL ERROR HANDLER::HTTP:{code}::{exception.Message}");

                        //New feature to avoid recursive serialization issues. However, it seems there are still errors which avoid using System.Text.Json instead of Newtonsoft.
                        //https://devblogs.microsoft.com/dotnet/announcing-net-5-0-rc-1/
                        //var result = JsonSerializer.Serialize<Exception>(exception, new JsonSerializerOptions
                        //{
                        //    WriteIndented = true,
                        //    ReferenceHandler = ReferenceHandler.Preserve,
                        //    IncludeFields = true
                        //});

                        //Newtonsoft.Json serializer (should be replaced once the known issue in System.Text.Json will be solved)
                        var result = JsonConvert.SerializeObject(exception, Formatting.Indented);

                        context.Response.Clear();
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(result);
                    }));

                    app.UseHsts();
                }

                app.UseHttpsRedirection();
                app.UseRouting();
                app.UseAuthorization();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
                app.UseRequestLocalization();

                //SWAGGER
                if (_appSettings.IsValid())
                {
                    if (_appSettings.Swagger.Enabled)
                    {
                        app.UseSwagger();
                        app.UseSwaggerUI(options =>
                        {
                            foreach (var description in provider.ApiVersionDescriptions)
                            {
                                options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                                //options.RoutePrefix = "swagger";
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}
