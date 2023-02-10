using System.Collections.Generic;
using System.Linq;
using MicroServicesCommonTools;

using System.Security.Claims;
using UserRegistration.Services.Model;

namespace UserRegistration.API.Utils
{
    public static class Helper
    {
        private static readonly Dictionary<int, string> _lanuage = new Dictionary<int, string>()
        {
            { 0, "en" }, { 1, "ru" }, { 2, "uz" }
        };

        public static UserParameters GetUserParameters(this ClaimsPrincipal user, string tin = "", string lang = "")
        {
            var langId = int.Parse(user.Claims.FirstOrDefault(c => c.Type == Constants.LANGUAGE_ID)?.Value ?? "2");
            var model = new UserParameters()
            {
                Lang = _lanuage[langId],
                Tin = user.Claims.FirstOrDefault(c => c.Type == Constants.TIN_VALUE)?.Value ?? string.Empty,
            };
            if (!string.IsNullOrEmpty(tin) && tin != "0")
                model.Tin = tin;
            if (model.Lang != lang && !string.IsNullOrEmpty(lang))
                model.Lang = lang;

            return model;
        }
    }
}
