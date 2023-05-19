using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.Json.Serialization;
using DbTools;
using DbTools.DataBase;
using MicroServicesCommonTools.MvcExtensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using UserRegistration.API.Common.Configuration;
using UserRegistration.API.Common.Settings;
using UserRegistration.IoC.Configuration.DI;
using UserRegistrationAPI.API.DataContracts.Settings;
using SwaggerDefaultValues = UserRegistration.API.Common.Attributes.SwaggerDefaultValues;

namespace UserRegistration.API
{
    public class Startup
    {
        private AppSettings _appSettings;
        private ApplicationInsights _applicationInsightsSettings;
        private readonly ILogger _logger;

        public Startup(IWebHostEnvironment env, ILogger<Startup> logger)
        {
            Configuration = ConfigurationHelper.GetIConfigurationRoot(env.EnvironmentName, env.ContentRootPath);
            HostEnvironment = env;
            _logger = logger;
            _logger.LogDebug("Startup::Constructor::Settings loaded");
        }

        public IConfiguration Configuration { get; }

        public IHostEnvironment HostEnvironment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var databaseConfiguration =
                Configuration.GetSection(nameof(DatabaseConfiguration)).Get<DatabaseConfiguration>();
            services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlServer(databaseConfiguration?.ConnectionString,
                    x => x.MigrationsAssembly(databaseConfiguration?.MigrationAssemblyName))
                , contextLifetime: ServiceLifetime.Transient);

            _logger.LogTrace("Startup::ConfigureServices");
          
            _appSettings = Configuration.GetSection("AppSettings").Get<AppSettings>();
            services.Configure<AppSettings>(Configuration.GetSection(nameof(AppSettings)));

            _logger.LogDebug("Startup::ConfigureServices::valid AppSettings");
            services.AddMvc();
            services.AddMvcCore().AddApiExplorer();

            services.AddControllers(opt => { opt.UseCentralRoutePrefix(new RouteAttribute(_appSettings.ServiceName)); })
                .AddNewtonsoftJson(options => options.UseMemberCasing())
                .AddJsonOptions(options =>
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
                .SetCompatibilityVersion(CompatibilityVersion.Latest);

            //API versioning
            services.AddApiVersioning(
                o =>
                {
                    //o.Conventions.Controller<UserController>().HasApiVersion(1, 0);
                    o.ReportApiVersions = true;
                    o.AssumeDefaultVersionWhenUnspecified = true;
                    o.DefaultApiVersion = new ApiVersion(1, 0);
                    o.ApiVersionReader = new UrlSegmentApiVersionReader();
                });

            // note: the specified format code will format the version as "'v'major[.minor][-status]"
            services.AddVersionedApiExplorer(
                options =>
                {
                    options.GroupNameFormat = "'v'VVV";

                    // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                    // can also be used to control the format of the API version in route accounting-service
                    options.SubstituteApiVersionInUrl = true;
                });


            //SWAGGER
            if ( _appSettings.Swagger.Enabled )
            {
                // -- Swagger configuration -- 
                services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigurationSwaggerOptions>();

                // Register the Swagger generator, defining 1 or more Swagger documents
                services.AddSwaggerGen(c =>
                {
                    c.OperationFilter<SwaggerDefaultValues>();
                    c.UseAllOfToExtendReferenceSchemas();

                    /*Authorization*/
                    var jwtSecurityScheme = new OpenApiSecurityScheme
                    {
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        Name = "JWT Authentication",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.Http,
                        Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",

                        Reference = new OpenApiReference
                        {
                            Id = JwtBearerDefaults.AuthenticationScheme,
                            Type = ReferenceType.SecurityScheme
                        }
                    };

                    c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        { jwtSecurityScheme, Array.Empty<string>() }
                    });

                    Assembly currentAssembly = Assembly.GetExecutingAssembly();
                    AssemblyName[] referencedAssemblies = currentAssembly.GetReferencedAssemblies();
                    IEnumerable<AssemblyName> allAssemblies = null;

                    if ( referencedAssemblies != null && referencedAssemblies.Any() )
                        allAssemblies = referencedAssemblies.Union(new AssemblyName[]
                            { currentAssembly.GetName() });
                    else
                        allAssemblies = new AssemblyName[] { currentAssembly.GetName() };

                    IEnumerable<string> xmlDocs = allAssemblies
                        .Select(a =>
                            Path.Combine(Path.GetDirectoryName(currentAssembly.Location), $"{a.Name}.xml"))
                        .Where(f => File.Exists(f));

                    if ( xmlDocs != null && xmlDocs.Any() )
                    {
                        foreach (var item in xmlDocs)
                        {
                            c.IncludeXmlComments(item, includeControllerXmlComments: true);
                        }
                    }
                });
            }

            //Mappings
            services.ConfigureMappings();


            services.ConfigureBusinessServices(Configuration);

            ServiceBuilder.BuildServices(services);

            _logger.LogDebug("Startup::ConfigureServices::ApiVersioning, Swagger and DI settings");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            _logger.LogTrace("Startup::Configure");
            _logger.LogDebug($"Startup::Configure::Environment:{env.EnvironmentName}");


            if ( env.IsDevelopment() )
            {
                app.UseDeveloperExceptionPage();
                _logger.LogInformation("Developer exception page loaded.");
            }
            else
            {
                app.UseExceptionHandler(a => a.Run(async context =>
                {
                    var feature = context.Features.Get<IExceptionHandlerPathFeature>();
                    var exception = feature.Error;
                    var code = HttpStatusCode.InternalServerError;

                    if ( exception is ArgumentNullException ) code = HttpStatusCode.BadRequest;
                    else if ( exception is ArgumentException ) code = HttpStatusCode.BadRequest;
                    else if ( exception is UnauthorizedAccessException ) code = HttpStatusCode.Unauthorized;

                    _logger.LogError($"GLOBAL ERROR HANDLER::HTTP:{code}::{exception.Message}");

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

            app.UseRequestLocalization();

            if ( _appSettings.Swagger.Enabled )
            {
                app.UseSwagger(delegate(SwaggerOptions c)
                {
                    c.RouteTemplate = $"/{_appSettings.ServiceName}/swagger/{{documentName}}/swagger.json";
                }).UseSwaggerUI(options =>
                {
                    foreach (var description in provider.ApiVersionDescriptions)
                    {
                        options.SwaggerEndpoint(
                            $"/{_appSettings.ServiceName}/swagger/{description.GroupName}/swagger.json",
                            $"{_appSettings.ServiceTitle} {description.GroupName}");
                        options.RoutePrefix = $"{_appSettings.ServiceName}/swagger";
                    }
                });
            }

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}