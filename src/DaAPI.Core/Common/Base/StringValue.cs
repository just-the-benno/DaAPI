using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Common.Base
{
    public abstract class StringValue<T> : Value<T>, IEquatable<StringValue<T>> where T : StringValue<T>
    {
        public String Value { get; protected set; }

        protected StringValue(String value)
        {
            Value = value;
        }

        public static implicit operator String(StringValue<T> value) => value.Value;

        protected static void EnforeNotEmpty(String input)
        {
            if (String.IsNullOrEmpty(input) == true || String.IsNullOrWhiteSpace(input) == true)
            {
                throw new ArgumentNullException(nameof(input));
            }
        }

        public bool Equals(StringValue<T> other)
        {
            if (other is null == true) { return false; }

            return StringComparer.InvariantCultureIgnoreCase.Compare(Value, other.Value) == 0;
        }
    }
}
