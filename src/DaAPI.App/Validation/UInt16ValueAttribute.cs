using DaAPI.Core.Common.DHCPv6;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.App.Validation
{
    public class UInt16ValueAttribute : ValidationAttribute
    {
        public UInt16ValueAttribute()
        {
        }

        public override bool IsValid(object value)
        {
            Int64 realValue = (Int64)value;

            return realValue >= UInt16.MinValue && realValue <= UInt16.MaxValue;
        }
    }
}
