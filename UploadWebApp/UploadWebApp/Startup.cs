using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UploadWebApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.Features;
using UploadWebApp.Filters;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore;

namespace UploadWebApp
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

            services.AddDbContext<UploadingContext>(options =>
            options.UseSqlite(Configuration.GetConnectionString("UploadingContext")));

            services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = int.MaxValue; // or your desired value
            });

            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = 209715200; // if don't set default value is: 30 MB
            });

            services.Configure<FormOptions>(options =>
            {
                options.ValueLengthLimit = 209715200;
                options.MultipartBodyLengthLimit = 209715200; // if don't set default value is: 128 MB
                options.MultipartHeadersLengthLimit = 209715200;
            });

            services.AddRazorPages(options =>
            {
                options.Conventions
                    .AddPageApplicationModelConvention("/Index",
                        model =>
                        {
                            model.Filters.Add(
                                new GenerateAntiforgeryTokenCookieAttribute());
                            model.Filters.Add(
                                new DisableFormValueModelBindingAttribute());
                            model.Filters.Add(
                                new RequestFormLimitsAttribute() { MultipartBodyLengthLimit = 268435456 });
                        });
                options.Conventions
                    .AddPageApplicationModelConvention("/Index",
                        model =>
                        {
                            model.Filters.Add(
                                new GenerateAntiforgeryTokenCookieAttribute());
                            model.Filters.Add(
                                new DisableFormValueModelBindingAttribute());
                        });

                options.Conventions
        .AddPageApplicationModelConvention("/Index",
            model =>
            {
                // Handle requests up to 50 MB
                model.Filters.Add(
                    new RequestSizeLimitAttribute(209715200));
            });

            });

            // To list physical files from a path provided by configuration:
            var physicalProvider = new PhysicalFileProvider(Configuration.GetValue<string>("StoredFilesPath"));

            // To list physical files in the temporary files folder, use:
            //var physicalProvider = new PhysicalFileProvider(Path.GetTempPath());

            services.AddSingleton<IFileProvider>(physicalProvider);

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.ConfigureKestrel((context, options) =>
            {
                // Handle requests up to 50 MB
                options.Limits.MaxRequestBodySize = 209715200;
            })
            .UseStartup<Startup>();
        });

        public static IWebHostBuilder CreateWebHostBuilder
(string[] args) =>
    WebHost.CreateDefaultBuilder(args)
        .UseStartup<Startup>()
        .UseKestrel(options =>
        {
            options.Limits.MaxRequestBodySize = 209715200;
        });

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
                    name: "default",
                    pattern: "{controller=Actions}/{action=Index}/{id?}");
            });
        }
    }
}
