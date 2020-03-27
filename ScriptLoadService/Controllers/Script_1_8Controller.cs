using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using fpsLibrary.Models;
using ScriptLoadService.Models;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;

namespace ScriptLoadService.Controllers
{
    public class Script_1_8Controller : Controller
    {
        string _versionKey = "1.8";
        string _versionFile = "1_8";

        string script;
        string inactiveScript;
        private enum SettingsType { Base, Extension, Package, Onload };
        private string baseProperties = "actions_lib_reserved";

        private readonly IAzureTableStorage<SettingData> _settingData;

        private readonly IAzureTableStorage<SiteData> _siteData;
        private readonly IWebHostEnvironment _host;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public Script_1_8Controller(IAzureTableStorage<SettingData> settingData, IAzureTableStorage<SiteData> siteData,
            IWebHostEnvironment host, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _settingData = settingData;
            _siteData = siteData;
            _host = host;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;

            script = System.IO.File.ReadAllText(_host.WebRootPath + "/Scripts/FPS_" + _versionFile + ".js");
            // script = System.IO.File.ReadAllText(_host.ContentRootPath("~/App_Data/FPS_" + _versionFile + ".js"));
            //inactiveScript = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/FPS_" + _versionFile + "_inactive.js"));
        }

        private async Task<string> GetOnloadSettingScriptAsync()
        {
            var settingEntities = await _settingData.GetList();
            if (settingEntities != null)
            {
                var onloadScript = settingEntities
                    .Where(e => (e.PartitionKey == _versionKey) && e.RowKey.StartsWith(SettingsType.Onload.ToString()))
                    .Select(e => e.SettingValue).FirstOrDefault();
                return onloadScript;
            }
            return string.Empty;
        }

        private async Task<string> GetSettingsAsync(SettingsType type, List<string> names)
        {
            List<string> rowKeys = names.Select(n => String.Concat(type.ToString(), ".", n)).ToList();
            StringBuilder strBuilder = new StringBuilder();
            var settingEntities = await _settingData.GetList();
            List<string> extensions = settingEntities.Where(e => (e.PartitionKey == _versionKey) && (rowKeys.Contains(e.RowKey))).Select(e => e.SettingValue).ToList();

            foreach (string extension in extensions)
            {
                strBuilder.Append("jQuery.extend(true, window.FPS, {")
                    .Append(extension)
                    .Append("});");
            }

            return strBuilder.ToString();
        }

        private async Task<string> GetPackagesAsync()
        {
            StringBuilder strBuilder = new StringBuilder();
            var settingEntities = await _settingData.GetList();
            List<string> packages = settingEntities.Where(e => (e.PartitionKey == _versionKey) && (e.RowKey.StartsWith(SettingsType.Package.ToString()))).Select(e => e.SettingValue).ToList();

            foreach (string package in packages)
            {
                strBuilder.Append("jQuery.extend(true, window.FPS, {")
                    .Append(package)
                    .Append("});");
            }

            return strBuilder.ToString();
        }

        //private string ResolveCookieValue(http cookie)
        //{
        //    return cookie != null && !string.IsNullOrEmpty(cookie.Value) ? cookie.Value : string.Empty;
        //}
        public async Task<IActionResult> IndexAsync(string brand = "", string country = "", string ns = "", string xdc = "", string rdns = "")
        {
            var siteEntity = await _siteData.GetItem(brand, country);
            string xdcGTUID = null;
            List<JObject> jsonObjects = new List<JObject>();
            var ccpaDomains = string.Empty;

            StringBuilder strBuilder = new StringBuilder(script);
            if (siteEntity != null)
            {
                string xdcSeviceUrl = null;
                if (!string.IsNullOrEmpty(xdc))
                {
                    // use the XDC service path configured in sitesettings first, 
                    // falling back to the URL set in the web.config otherwise
                    xdcSeviceUrl = !string.IsNullOrEmpty(siteEntity.DomainPath) ? siteEntity.DomainPath :  _configuration["XDCServiceEndpoint"];
                }
                //xcdDomain is the domain name to be used 
                //xcdRegSrvc is the url configured in the Admin site settings panel to point to the xdc registration service
                //xcdPath represents the path/method to be used in the case of an xcd cookie request, which will override the default
                return new JavaScriptResult(strBuilder
                    .Replace("`CCPA_DOMAINS`", ccpaDomains)
                    .Replace("`xdcGTUID`", !string.IsNullOrEmpty(xdc) ? (!string.IsNullOrEmpty(xdcGTUID) ? xdcGTUID : string.Empty) : string.Empty)
                    .Replace("`xdcRegSrvc`", !string.IsNullOrEmpty(xdc) ? (!string.IsNullOrEmpty(xdcSeviceUrl) ? xdcSeviceUrl : string.Empty) : string.Empty)
                    .Replace("`xdcDomain`", !string.IsNullOrEmpty(xdc) ? (!string.IsNullOrEmpty(siteEntity.Domain) ? siteEntity.Domain : string.Empty) : string.Empty)
                    .Replace("`xdcPath`", !string.IsNullOrEmpty(siteEntity.XdcPath) ? siteEntity.XdcPath : string.Empty)
                    .Replace("`sCode`", _httpContextAccessor.HttpContext.GetServerVariable("ServerCode"))
                    .Replace("`brand`", brand)
                    .Replace("`country`", country)
                    .Replace("'`cookieDaysToLive`'", siteEntity.CookieDaysToLive.ToString())
                    .Replace("'`personalizationActive`'", siteEntity.PersonalizationActive.ToString().ToLowerInvariant())
                    .Replace("// BASE", await GetSettingsAsync(SettingsType.Base, baseProperties.Split('_').ToList()))
                    .Replace("// PACKAGES",await GetPackagesAsync())
                    .Replace("// EXTENSIONS",await GetSettingsAsync(SettingsType.Extension, ns.Split('_').ToList()))
                    .Replace("// ACTIVE?", inactiveScript)
                    .Replace("// SETTING_ONLOAD", await GetOnloadSettingScriptAsync())
                    .Replace("// SITE_ONLOAD", !string.IsNullOrEmpty(siteEntity.SiteOnloadScript) ? siteEntity.SiteOnloadScript : string.Empty)
                    .ToString()
                );
            }

            return new JavaScriptResult("(function () { if (typeof window.FPS != 'undefined') { delete window.FPS; } }());");
            //return View();
        }

        //[BrowserCache]
        public IActionResult Extend(string ns)
        {
            var script = GetSettingsAsync(SettingsType.Extension, ns.Split('_').ToList());
            return new JavaScriptResult (script.ToString());
        }
    }

    public class JavaScriptResult : ContentResult
    {
        public JavaScriptResult(string script)
        {
            this.Content = script;
            this.ContentType = "application/javascript";
        }
    }
}