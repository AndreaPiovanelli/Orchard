using System;
using System.Web;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Localization.Services;
using Orchard.Mvc;
using Orchard.Services;

namespace Orchard.Localization.Providers {
    [OrchardFeature("Orchard.Localization.CutlureSelector")]
    public class CultureStorageProvider : ICultureStorageProvider {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IClock _clock;
        private readonly ShellSettings _shellSettings;

        private const string CookieName = "OrchardCurrentCulture";
        private const int DefaultExpireTimeYear = 1;

        public CultureStorageProvider(IHttpContextAccessor httpContextAccessor,
            IClock clock,
            ShellSettings shellSettings) {
            _httpContextAccessor = httpContextAccessor;
            _clock = clock;
            _shellSettings = shellSettings;
        }

        public void SetCulture(string culture) {
            var httpContext = _httpContextAccessor.Current();

            if (httpContext == null) return;

            var cookie = new HttpCookie(CookieName, culture) {
                Expires = _clock.UtcNow.AddYears(DefaultExpireTimeYear), 
            };

            if (!String.IsNullOrEmpty(_shellSettings.RequestUrlHost)) {
                cookie.Domain = _shellSettings.RequestUrlHost;
            }

            if (!String.IsNullOrEmpty(_shellSettings.RequestUrlPrefix)) {
                cookie.Path = GetCookiePath(httpContext);
            }

            httpContext.Request.Cookies.Remove(CookieName);
            httpContext.Response.Cookies.Remove(CookieName);
            httpContext.Response.Cookies.Add(cookie);
        }

        public string GetCulture() {
            var httpContext = _httpContextAccessor.Current();

            if (httpContext == null) return null;

            var cookie = httpContext.Request.Cookies.Get(CookieName);

            if (cookie != null)
                return cookie.Value;

            return null;
        }

        private string GetCookiePath(HttpContextBase httpContext) {
            var cookiePath = httpContext.Request.ApplicationPath;
            if (cookiePath != null && cookiePath.Length > 1) {
                cookiePath += '/';
            }

            cookiePath += _shellSettings.RequestUrlPrefix;

            return cookiePath;
        }
    }
}