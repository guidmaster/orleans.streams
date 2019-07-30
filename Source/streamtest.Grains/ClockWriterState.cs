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

    public class ClockWriterState
    {
        public DateTime Date { get; set; }
        public string Msg { get; set; }

        public bool FirstTime { get; set; }
    }
}
