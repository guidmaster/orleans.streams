using Orleans;
using streamtest.Abstractions.Constants;
using streamtest.Abstractions.Grains;
using System;
using System.Threading.Tasks;

namespace streamtest.Grains
{
    public class ClockGrain : Grain, IClock
    {
        private string _currentTime;

        public override Task OnActivateAsync()
        {
            //this.RegisterTimer(this.OnTimerTick, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

            //var streamProvider = GetStreamProvider(StreamProviderName.Default);
            var streamProvider = GetStreamProvider("myname");
            //Get the reference to a stream
            var stream = streamProvider.GetStream<string>(Guid.Empty, "TIME");

            this.RegisterTimer(s =>
            {
                return stream.OnNextAsync(DateTime.UtcNow.ToString());
            }, null, TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(1000));


            return base.OnActivateAsync();
        }

        private Task OnTimerTick(object arg)
        {
            _currentTime = DateTime.UtcNow.ToString();
            var streamProvider = GetStreamProvider(StreamProviderName.Default);
            //Get the reference to a stream
            var stream = streamProvider.GetStream<string>(Guid.Empty, "TIME");
            return stream.OnNextAsync(_currentTime);
        }

        public Task<string> RegisterTask()
        {
            return Task.FromResult<string>("Hello");
        }
    }
}
