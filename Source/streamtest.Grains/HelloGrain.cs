namespace streamtest.Grains
{
    using System.Threading.Tasks;
    using Orleans;
    using streamtest.Abstractions.Grains;

    public class HelloGrain : Grain, IHelloGrain
    {
        public async Task<string> SayHello(string name)
        {
            var counter = this.GrainFactory.GetGrain<ICounterStatelessGrain>(0L);
            await counter.Increment();

            return $"Hello {name}!";
        }
    }
}
