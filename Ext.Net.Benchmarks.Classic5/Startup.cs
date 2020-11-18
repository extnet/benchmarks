using System.Linq;

using Ext.Net.Core;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Ext.Net.Benchmarks.Classic5
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
            services.AddExtNet();

            if (services
                .LastOrDefault(x => x.ServiceType == typeof(ApplicationPartManager))
                ?.ImplementationInstance is ApplicationPartManager appPartManager)
            {
                appPartManager.ApplicationParts.Add(new AssemblyPart(GetType().Assembly));
            }
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
                app.UseExceptionHandler("/Benchmark/Error");
            }

            app.UseStaticFiles();

            app.UseExtNetResources(cfg =>
            {
                cfg.UseEmbedded();
                cfg.UseThemeSpotless();
            });

            app.UseRouting();

            app.UseExtNet(cfg =>
            {
                cfg.Theme = ThemeKind.Spotless;
                cfg.DisableAntiforgery = true;
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Benchmark}/{action=Index}/{id?}");
            });
        }
    }
}
