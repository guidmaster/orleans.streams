using Orleans;
using System.Threading.Tasks;

namespace streamtest.Abstractions.Grains
{
    public interface IClock : IGrainWithGuidKey
    {

        Task<string> RegisterTask();

    }
}