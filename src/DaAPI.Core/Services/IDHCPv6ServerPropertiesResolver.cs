using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Services
{
   
    public interface IDHCPv6ServerPropertiesResolver
    {
        DUID GetServerDuid();
        TimeSpan GetLeaseLifeTime();
        TimeSpan GetHandledLifeTime();
        UInt32 GetMaximumHandledCounter();
    }
}
