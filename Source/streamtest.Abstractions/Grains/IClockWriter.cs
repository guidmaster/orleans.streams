using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace streamtest.Abstractions.Grains
{
    public interface IClockWriter : IGrainWithGuidKey
    {

        Task<string> GetTime();

        Task RegisterClockAsync(IClock clock);

        Task DeRegisterAsync();
    }
}
