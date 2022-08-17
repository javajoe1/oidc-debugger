using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Hosting;

namespace OidcDebugger
{
    public class Startup
    {
        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            Configuration = configuration;
            Logger = logger;
        }

        public IConfiguration Configuration { get; }

        public ILogger<Startup> Logger { get; }

        public void ConfigureDevelopmentServices(IServiceCollection services)
        {
            ConfigureCommonServices(services);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureCommonServices(services);

            // Require HTTPS in production
            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new RequireHttpsAttribute());
            });

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });
        }

        private void ConfigureCommonServices(IServiceCollection services)
        {
            services.Configure<MultitenancyOptions>(Configuration.GetSection("Multitenancy"));
            services.AddMultitenancy<AppTenant, AppTenantResolver>();

            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseForwardedHeaders();

            app.UseReferrerPolicy(opts => opts.NoReferrerWhenDowngrade());

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                // JBF told to comment this out to avoid some webpack issue. see https://itecnote.com/tecnote/c-cannot-find-module-aspnet-webpack-when-using-dotnet-publish-in-net-core-2-0-angular/
                //                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                //                {
                //                    HotModuleReplacement = true,
                //                });
            }
            else
            {
                app.UseHsts(options => options.MaxAge(days: 365).IncludeSubdomains());

                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseRouting();

            app.UseXfo(options => options.SameOrigin());
            app.UseXXssProtection(options => options.EnabledWithBlockMode());
            app.UseXContentTypeOptions();

            app.UseMultitenancy<AppTenant>();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}
