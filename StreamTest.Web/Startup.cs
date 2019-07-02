using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using streamtest.Abstractions.Constants;
using streamtest.Abstractions.Grains;
using StreamTest.Web.Streaming;

namespace StreamTest.Web
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
            services.AddControllers();
            services.AddWebSocketManager();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
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

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();
            var webSocketOptions = new WebSocketOptions();
            webSocketOptions.KeepAliveInterval = TimeSpan.FromSeconds(10);

            app.UseWebSockets(webSocketOptions);

            app.MapWebSocketManager("/ws", serviceProvider.GetService<EventMessageHandler>());

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
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
