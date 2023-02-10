using System.ComponentModel.DataAnnotations;

namespace UserRegistration.API.Common.Settings
{
    public class AppSettings
    {
        public string Secret { get; set; } = string.Empty;
        public string CrmKey { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string ServiceTitle { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string MockThis { get; set; } = string.Empty;
        public ApiSettings API { get; set; }
        public Swagger Swagger { get; set; }
    }


    public class ApiSettings
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public int DefaultVersionMinor { get; set; }

        public int DefaultVersionMajor { get; set; }

        public ApiContact Contact { get; set; }

        public string TermsOfServiceUrl { get; set; }

        public ApiLicense License { get; set; }
    }

    public class ApiContact
    {
        public string Name { get; set; }
        public string Email { get; set; }

        public string Url { get; set; }
    }

    public class ApiLicense
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }

    public class Swagger
    {
        public bool Enabled { get; set; }
    }
}
