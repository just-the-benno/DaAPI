using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv6
{
    public class DHCPv6TimeScale : Value<DHCPv6TimeScale>
    {
        private const Double _min = 0.05;
        private const Double _max = 0.95;

        public Double Value { get; }


        internal DHCPv6TimeScale(Double value)
        {
            Value = value;
        }

        public static DHCPv6TimeScale FromDouble(Double input)
        {
            if (input < _min || input > _max)
            {
                throw new ArgumentException(nameof(input));
            }

            return new DHCPv6TimeScale(input);
        }

        public static implicit operator Double(DHCPv6TimeScale input) => input.Value;

        public override string ToString() => Value.ToString();
    }
}
