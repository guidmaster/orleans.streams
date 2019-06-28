namespace streamtest.Server.IntegrationTest
{
    using System;
    using System.Threading.Tasks;
    using Orleans.TestingHost;
    using streamtest.Abstractions.Grains;
    using streamtest.Server.IntegrationTest.Fixtures;
    using Xunit;

    public class HelloGrainTest : IClassFixture<ClusterFixture>
    {
        private readonly TestCluster cluster;

        public HelloGrainTest(ClusterFixture fixture) => this.cluster = fixture.Cluster;

        [Fact]
        public async Task SayHello_PassName_ReturnsGreeting()
        {
            var grain = this.cluster.GrainFactory.GetGrain<IHelloGrain>(Guid.NewGuid());

            var greeting = await grain.SayHello("Rehan");

            Assert.Equal("Hello Rehan!", greeting);
        }
    }
}
