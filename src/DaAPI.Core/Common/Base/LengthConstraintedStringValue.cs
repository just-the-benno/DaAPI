using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Common.Base
{
    public class LengthConstraintedStringValue<T> : StringValue<T> where T : StringValue<T>
    {
        protected LengthConstraintedStringValue(String input) : base(input)
        {

        }

        protected static void EnforceMinLength(String input, Int32 min)
        {
            if (input.Length < min)
            {
                throw new ArgumentOutOfRangeException($"the input value should have less than {min} characters", nameof(input));
            }
        }

        protected static void EnforceMaxLength(String input, Int32 max)
        {
            if (input.Length > max)
            {
                throw new ArgumentOutOfRangeException($"the input value should not exceed {max} characters", nameof(input));
            }
        }

        protected static void EnforeMinAndMaxLength(String input, Int32 min, Int32 max)
        {
            EnforeNotEmpty(input);
            EnforceMinLength(input, min);
            EnforceMaxLength(input, max);
        }

        public override string ToString() => Value;
    }
}
