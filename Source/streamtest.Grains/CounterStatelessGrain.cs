namespace streamtest.Grains
{
    using System;
    using System.Threading.Tasks;
    using Orleans;
    using Orleans.Concurrency;
    using streamtest.Abstractions.Grains;

    /// <summary>
    /// An implementation of the 'Reduce' pattern (See https://github.com/OrleansContrib/DesignPatterns/blob/master/Reduce.md).
    /// </summary>
    /// <seealso cref="Grain" />
    /// <seealso cref="ICounterStatelessGrain" />
    [StatelessWorker]
    public class CounterStatelessGrain : Grain, ICounterStatelessGrain
    {
        private long count = 0;

        public Task Increment()
        {
            this.count += 1;
            return Task.CompletedTask;
        }

        public override Task OnActivateAsync()
        {
            this.RegisterTimer(this.OnTimerTick, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            return base.OnActivateAsync();
        }

        private Task OnTimerTick(object arg)
        {
            var count = this.count;
            this.count = 0;
            var counter = this.GrainFactory.GetGrain<ICounterGrain>(Guid.Empty);
            return counter.AddCount(count);
        }
    }
}
