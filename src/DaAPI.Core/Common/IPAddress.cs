using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Common
{
    public abstract class IPAddress<TAddress> : Value where TAddress: IPAddress<TAddress>
    {
        public abstract Boolean IsBetween(TAddress start, TAddress end);

        public abstract Byte[] GetBytes();

        public abstract Boolean IsGreaterThan(TAddress other);
    }
}
