using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ScriptLoadService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace ScriptLoadService.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger,IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            // Build the URL that is used to fetch the FPS script using Request.Url (used by statusPage.js)
            // For azurewebsites or localhost the path does not need to be modified 
            // with the '/fps' prefix, but the production sites that have the Akamai path need to add it in order to resolve.
            //var reqHost = Request.Url != null ? Request.Url.GetLeftPart(UriPartial.Authority) : null;
            //var reqHost = GetUri(Request);            
            var reqHost = new Uri(Request.Scheme + "://" + Request.Host.Value).ToString();
            
            if (!string.IsNullOrEmpty(reqHost))
            {
                if (reqHost.ToLower().Contains("azurewebsites") || reqHost.ToLower().Contains("localhost"))
                {
                    ViewBag.ApiUrl = reqHost;
                }
                else
                {
                    // Otherwise use the configured "ProxyPrefixPath", or default to "fps" if not set
                    var prefixPath = !string.IsNullOrEmpty(_configuration["ProxyPrefixPath"])
                        ? _configuration["ProxyPrefixPath"]
                        : "fps";

                    ViewBag.ApiUrl = reqHost + "/" + prefixPath;
                }
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private static string GetUri(HttpRequest request)
        {
            var builder = new UriBuilder();
            builder.Scheme = request.Scheme;
            builder.Host = Convert.ToString(request.Host.Value);
            builder.Host = request.Host.Value;
            builder.Path = request.Path;
            builder.Query = request.QueryString.ToUriComponent();
            return builder.Uri.Authority;
        }
    }
}
