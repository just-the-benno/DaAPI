using DaAPI.Core.Common.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Listeners
{
    public class NICInterfaceName : LengthConstraintedStringValue<NICInterfaceName>
    {
        internal NICInterfaceName(String value) : base(value)
        {

        }

        public static NICInterfaceName FromString(String input)
        {
            EnforeMinAndMaxLength(input, 1, 255);
            return new NICInterfaceName(input);
        }
    }
}
