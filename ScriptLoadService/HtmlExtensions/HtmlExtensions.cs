using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using System.Web;

namespace ScriptLoadService.HtmlExtensions
{
    public static class HtmlExtensions
    {
        // Return appropriate path to locate resources based on environment.
        // Due to the Akamai proxy for fps in production, cannot just use ~/ in the views to render
        // from application root, so can configure "ProxyPrefixPath" which will be used when not running
        // on as azurewebsites.net or localhost
        public static string EnvironmentPathString(IHtmlHelper htmlHelper, IConfiguration configuration)
        {
            if (htmlHelper.ViewContext.HttpContext.Request != null)
            {
                var reqHost = htmlHelper.ViewContext.HttpContext.Request.Host.Value;
                if (!string.IsNullOrEmpty(reqHost))
                {
                    // for azurewebsites or localhost, just return "~" to render from application root
                    if (reqHost.ToLower().Contains("azurewebsites") || reqHost.ToLower().Contains("localhost") || reqHost.ToLower().Contains("na.fps"))                                                                                                                
                    {
                        return "~";
                    }
                   
                    // Otherwise use the configured "ProxyPrefixPath", or default to "fps"
                    var prefixPath = !string.IsNullOrEmpty(configuration["ProxyPrefixPath"]) ?
                                    configuration["ProxyPrefixPath"] :
                                    "fps";

                    return "/" + prefixPath;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Return path string using the configuration variable: qunit:ApiUrl
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <returns></returns>
        public static string ApiPathString(IHtmlHelper htmlHelper, IConfiguration configuration)
        {
            var apiUrl = !string.IsNullOrEmpty(configuration["qunit:ApiUrl"])
                ? configuration["qunit:ApiUrl"]
                : null;

            if (apiUrl != null)
            {
                var apiPathStr = htmlHelper.ViewContext.HttpContext.Request != null
                    ? htmlHelper.ViewContext.HttpContext.Request.Scheme + "://" + configuration["qunit:ApiUrl"]
                    : string.Empty;

                return apiPathStr;
            }

            return null;
        }
    }
}