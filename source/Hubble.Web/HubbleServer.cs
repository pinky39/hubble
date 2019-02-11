using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;
using System.Threading;
using Newtonsoft.Json;
using System.IO;
using NLog.Web;
using NLog.LayoutRenderers;
using Microsoft.AspNetCore.HttpOverrides;

namespace Hubble.Web
{
    public class HubbleServer
    {
        private readonly IWebHost _host;                
        
        public HubbleServer(string[] args) : this(null, args)
        {
            
        }

        public HubbleServer(string contentRoot) : this(contentRoot, null) 
        {       
        }
        
        public HubbleServer(string contentRoot, string[] args)
        {            
            contentRoot = contentRoot ?? string.Empty;
            args = args ?? new string[] {};

            var logger = NLog.Web.NLogBuilder
                .ConfigureNLog(Path.Combine(contentRoot, "nlog.config"))
                .GetCurrentClassLogger();

            _host = CreateWebHostBuilder(contentRoot, args).Build();
        }

        public IDisposable Start()
        {
            _host.Start();
            return _host;
        }

        public void Run()
        {
            _host.Run();
        }

        private IWebHostBuilder CreateWebHostBuilder(string contentRoot, string[] args)
        {
            var builder = WebHost.CreateDefaultBuilder(args);

            if (!String.IsNullOrEmpty(contentRoot))
            {
                builder.UseContentRoot(contentRoot);
            }

            builder
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
            })
            .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;

                    config
                        .AddJsonFile("registeredservices.json",
                            optional: false, reloadOnChange: true)
                        .AddJsonFile("appsettings.json",
                            optional: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json",
                            optional: true)
                        .AddEnvironmentVariables();
                })
            .UseNLog()
            .UseStartup<Startup>();

            return builder;
        }

        private class Startup
        {
            public Startup(IConfiguration configuration)
            {
                Configuration = configuration;
            }
            public IConfiguration Configuration { get; }
            public void ConfigureServices(IServiceCollection services)
            {
                services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
                services.AddMemoryCache();
            }
            public void Configure(IApplicationBuilder app, IHostingEnvironment env)
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }

                app.UseMvc();

                // za RemoteIpAddress, če imamo vmes proxy
                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });
            }
        }
    }
}
