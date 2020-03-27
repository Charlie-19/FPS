using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using fpsLibrary.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ScriptLoadService.Models;

namespace ScriptLoadService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddHttpContextAccessor();
            services.AddTransient<IAzureTableStorage<SettingData>>(factory =>
            {
                return new AzureTableStorage<SettingData>(
                    new AzureTableSettings(
                          storageAccount: Configuration.GetSection("StorageAccount")["Table_StorageAccount"],
                          storageKey: Configuration.GetSection("StorageAccount")["Table_StorageKey"],
                          tableName: Configuration.GetSection("StorageAccount")["Table_SettingDataDev"]));
            });
            services.AddTransient<IAzureTableStorage<SiteData>>(factory =>
            {
                return new AzureTableStorage<SiteData>(
                    new AzureTableSettings(
                        storageAccount: Configuration.GetSection("StorageAccount")["Table_StorageAccount"],
                        storageKey: Configuration.GetSection("StorageAccount")["Table_StorageKey"],
                        tableName: Configuration.GetSection("StorageAccount")["Table_SiteDataDev"]));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "Script",
                    pattern: "{controller=Script_1_8}/{action=Index}/{brand?}/{country?}",                    
                    defaults: new { controller = "Script_1_8", action = "Index" }); ;

                endpoints.MapControllerRoute(
                    name: "Default",
                    pattern: "{controller=Home}/{action=Index}/{id?}",
                    defaults: new { controller = "Home", action = "Index" });

            });
        }
    }
}
