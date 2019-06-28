namespace streamtest.Server.IntegrationTest
{
    using System;
    using System.Threading.Tasks;
    using streamtest.Abstractions.Grains;
    using streamtest.Server.IntegrationTest.Fixtures;
    using Xunit;

    public class CounterStatelessGrainTest
    {
        [Fact]
        public async Task Increment_Default_EventuallyIncrementsTotalCount()
        {
            using (var fixture = new ClusterFixture())
            {
                var grain = fixture.Cluster.GrainFactory.GetGrain<ICounterStatelessGrain>(0L);
                var counterGrain = fixture.Cluster.GrainFactory.GetGrain<ICounterGrain>(Guid.Empty);

                await grain.Increment();
                var countBefore = await counterGrain.GetCount();

                Assert.Equal(0L, countBefore);

                await Task.Delay(TimeSpan.FromSeconds(2));

                var countAfter = await counterGrain.GetCount();
            }
        }
    }
}
