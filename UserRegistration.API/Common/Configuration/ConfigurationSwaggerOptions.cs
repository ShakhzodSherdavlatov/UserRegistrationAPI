using System;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using UserRegistrationAPI.API.DataContracts.Settings;

namespace UserRegistration.API.Common.Configuration
{
    /// <summary>
    /// https://github.com/dotnet/aspnet-api-versioning/wiki/API-Documentation#aspnet-core
    /// </summary>
    public class ConfigurationSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        readonly IApiVersionDescriptionProvider provider;
        readonly AppSettings appSettings;

        public ConfigurationSwaggerOptions(IApiVersionDescriptionProvider provider, IConfiguration configuration)
        {
            this.provider = provider;
            appSettings = configuration.GetSection(nameof(AppSettings)).Get<AppSettings>();
        }

        public void Configure(SwaggerGenOptions options)
        {
            Configure(options, appSettings.API);
        }

        public void Configure(SwaggerGenOptions options, ApiSettings apiSettings)
        {
            if ( options == null )
                throw new ArgumentNullException(nameof(options));

            if ( apiSettings == null )
                throw new ArgumentNullException(nameof(apiSettings));

            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description, apiSettings));
            }
        }

        OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description, ApiSettings apiSettings)
        {
            var info = new OpenApiInfo()
            {
                Title = $"{apiSettings.Title} {description.ApiVersion}",
                Version = description.ApiVersion.ToString(),
                Description = apiSettings?.Description,
                Contact = apiSettings != null && apiSettings.Contact != null
                    ? new OpenApiContact
                    {
                        Name = apiSettings.Contact.Name, Email = apiSettings.Contact.Email,
                        Url = new Uri(apiSettings.Contact.Url)
                    }
                    : null,
                License = apiSettings != null && apiSettings.License != null
                    ? new OpenApiLicense { Name = apiSettings.License.Name, Url = new Uri(apiSettings.License.Url) }
                    : null,
                TermsOfService = !string.IsNullOrEmpty(apiSettings?.TermsOfServiceUrl)
                    ? new Uri(apiSettings.TermsOfServiceUrl)
                    : null
            };

            if ( description.IsDeprecated )
            {
                info.Description += " This API version has been deprecated.";
            }

            return info;
        }
    }
}