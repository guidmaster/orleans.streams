using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Streams;
using streamtest.Abstractions.Grains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace streamtest.Grains
{
    public class ClockWriterGrain : Grain<ClockWriterState>, IClockWriter
    {
        private string _time;
        private IClock _clock;

        private IList<StreamSubscriptionHandle<string>> subscriptionHandles;
        private IAsyncStream<string> stream;
        private IStreamProvider streamProvider;
        private readonly ILogger<ClockWriterGrain> _logger;

        public ClockWriterGrain(ILogger<ClockWriterGrain> logger)
        {
            this._logger = logger;
        }

        public override async Task OnActivateAsync()
        {
            try
            {
                if (State.FirstTime)
                {
                    State = new ClockWriterState();
                    State.FirstTime = false;
                }
                await WriteStateAsync();
            }
            catch (Exception ex)
            {

                throw;
            }
            
            //streamProvider = GetStreamProvider(StreamProviderName.Default);
            streamProvider = GetStreamProvider("myname");
            stream = streamProvider.GetStream<string>(Guid.Empty, "TIME");





            stream = streamProvider.GetStream<WorkspaceCreated>(tenId, "Workspace");
            stream = streamProvider.GetStream<GroupCreated>(tenId, "Workspace");





            _logger.LogDebug("Getting stream {StreamName}", stream.Namespace);
            subscriptionHandles = await stream.GetAllSubscriptionHandles();
            if (subscriptionHandles.Count > 0)
            {
                subscriptionHandles.ToList().ForEach(async x =>
                {
                    _logger.LogDebug("Resuming {StreamName}", stream.Namespace);
                    await x.ResumeAsync((payload, token) => OnNextAsync(payload));
                });
            }

            await base.OnActivateAsync();
        }

        public override async Task OnDeactivateAsync()
        {
            if (subscriptionHandles.Count > 0)
            {
                subscriptionHandles.ToList().ForEach(async x =>
                {
                    await x.UnsubscribeAsync();
                });
            }
            await base.OnDeactivateAsync();
        }

        public Task OnNextAsync(string payload)
        {
            State.Date = DateTime.Parse(payload);
            State.Msg = payload;
            _logger.LogDebug("Inside time is {Time}", payload);
            _time = payload;
            Console.WriteLine("From Inside: " + _time);
            return WriteStateAsync();
        }


        public async Task DeRegisterAsync()
        {
            if (subscriptionHandles.Count > 0)
            {
                subscriptionHandles.ToList().ForEach(async x =>
                {
                    await x.UnsubscribeAsync();
                });
            }
        }

        public Task<string> GetTime()
        {
            return Task.FromResult<string>(_time);
        }

        public Task RegisterClockAsync(IClock clock)
        {
            _clock = clock;
            return stream.SubscribeAsync((data, token) => OnNextAsync(data));
        }
    }
}
