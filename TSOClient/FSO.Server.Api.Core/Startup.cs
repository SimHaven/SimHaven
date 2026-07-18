using FSO.Server.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Security;
using System.Security.Authentication;

namespace FSO.Server.Api.Core
{
    public class Startup : IAPILifetime
    {
        public IApplicationLifetime AppLifetime;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
               options.AddDefaultPolicy(
                   builder =>
                   {

                       builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().WithExposedHeaders("content-disposition");
                   });

                options.AddPolicy("AdminAppPolicy",
                    builder =>
                    {
                        builder.WithOrigins("https://freeso.org", "http://localhost:8080").AllowAnyMethod().AllowAnyHeader().AllowCredentials().WithExposedHeaders("content-disposition");
                    });
            }).AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            app.UseCors();
            app.Use(async (context, next) =>
            {
                try
                {
                    await next();
                }
                catch (AuthenticationException)
                {
                    if (context.Response.HasStarted) throw;
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"error\":\"authentication_required\"}");
                }
                catch (SecurityException)
                {
                    if (context.Response.HasStarted) throw;
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"error\":\"forbidden\"}");
                }
            });
            //app.UseHttpsRedirection();
            app.UseMvc();
            AppLifetime = appLifetime;
        }

        public void Stop()
        {
            AppLifetime.StopApplication();
        }
    }
}
