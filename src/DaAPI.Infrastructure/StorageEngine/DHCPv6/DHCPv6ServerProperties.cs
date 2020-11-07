using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Infrastructure.StorageEngine.DHCPv6
{
    public class DHCPv6ServerProperties
    {
        public DUID ServerDuid { get; set; }
        public Boolean IsInitilized { get; set; }
        public TimeSpan LeaseLifeTime { get; set; }
        public TimeSpan HandledLifeTime { get; set; }
        public UInt32 MaximumHandldedCounter { get; set; }

        internal void SetDefaultIfNeeded()
        {
            if(LeaseLifeTime == default)
            {
                LeaseLifeTime = TimeSpan.FromDays(60);
            }

            if(HandledLifeTime == default)
            {
                HandledLifeTime = TimeSpan.FromDays(60);
            }

            if(MaximumHandldedCounter == 0)
            {
                MaximumHandldedCounter = 30_000;
            }
        }
    }
}
