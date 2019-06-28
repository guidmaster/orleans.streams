namespace streamtest.Client
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Orleans;
    using Orleans.Configuration;
    using Orleans.Hosting;
    using Orleans.Providers.Streams.AzureQueue;
    using Orleans.Runtime;
    using Orleans.Streams;
    using streamtest.Abstractions.Constants;
    using streamtest.Abstractions.Grains;

    public class Program
    {
        static async Task<int> Main(string[] args)
        {
            try
            {
                var clusterClient = CreateClientBuilder().Build();
                await clusterClient.Connect();
                RequestContext.Set("TraceId", Guid.NewGuid());

                var clock = clusterClient.GetGrain<IClock>(Guid.Empty);
                await clock.RegisterTask();
                var clockWriter = clusterClient.GetGrain<IClockWriter>(Guid.Empty);
                await clockWriter.RegisterClockAsync(clock);

                //var streamProvider = clusterClient.GetStreamProvider(StreamProviderName.Default);
                var streamProvider = clusterClient.GetStreamProvider("myname");

                Guid streamId = Guid.Empty;
                var stream = streamProvider.GetStream<string>(streamId, "TIME");
                StreamSubscriptionHandle<string> subscription;
                var handlers = await stream.GetAllSubscriptionHandles();

                if (null == handlers || 0 == handlers.Count)
                {
                    subscription = await stream.SubscribeAsync((data, _) => OnStreamMessage(data));
                }
                else
                {
                    foreach (var handler in handlers)
                    {
                        subscription = await handler.ResumeAsync((data, _) => OnStreamMessage(data));
                    }
                }

                Console.ReadLine();

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                return -1;
            }

            return 0;
        }

        private static Task OnStreamMessage(string message)
        {
            Console.WriteLine("Console says " + message);
            return Task.CompletedTask;
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
