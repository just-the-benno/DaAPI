using DaAPI.Core.Common.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Listeners
{
    public class DHCPListenerName : LengthConstraintedStringValue<DHCPListenerName>
    {
        internal DHCPListenerName(String value) : base(value)
        {

        }

        public static DHCPListenerName FromString(String input)
        {
            EnforeMinAndMaxLength(input, 3, 150);
            return new DHCPListenerName(input);
        }
    }
}
