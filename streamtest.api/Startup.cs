using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Providers.Streams.AzureQueue;
using streamtest.Abstractions.Constants;
using streamtest.Abstractions.Grains;
using streamtest.api.Streaming;

namespace streamtest.api
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddWebSocketManager();

            var clusterClient = CreateClientBuilder().Build();

            clusterClient.Connect().Wait();


            services.AddSingleton<IClusterClient>(clusterClient);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            var webSocketOptions = new WebSocketOptions();
            webSocketOptions.KeepAliveInterval = TimeSpan.FromSeconds(10);

            app.UseWebSockets(webSocketOptions);

            app.MapWebSocketManager("/ws", serviceProvider.GetService<EventMessageHandler>());

            app.UseHttpsRedirection();
            app.UseMvc();
        }


        private static IClientBuilder CreateClientBuilder() =>
           new ClientBuilder()
               .UseAzureStorageClustering(options => options.ConnectionString = "UseDevelopmentStorage=true")
               .Configure<ClusterOptions>(options =>
               {
                   options.ClusterId = Cluster.ClusterId;
                   options.ServiceId = Cluster.ServiceId;
               })
               .ConfigureApplicationParts(
                   parts => parts
                       .AddApplicationPart(typeof(ICounterGrain).Assembly)
                       .WithReferences())
               .AddSimpleMessageStreamProvider(StreamProviderName.Default)
               .AddAzureQueueStreams<AzureQueueDataAdapterV2>("myname",
                           configurator => configurator.Configure(configure =>
                           {
                               configure.ConnectionString = "UseDevelopmentStorage=true";
                           })
                );
    }
}
