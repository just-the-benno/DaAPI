using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DaAPI.Core.Scopes
{
    public class ScopeName : Value<ScopeName>
    {
        #region consts

        public const Int32 MaxLength = 250;
        public const Int32 MinLenth = 10;

        public const String AllowedChars = @"^[\w]*((-|\s|\+|&|\#|\$|\*)*[\w])*$";

        #endregion

        #region Properties

        public String Value { get; private set; }

        #endregion

        #region constructor and factories

        private ScopeName()
        {
        }

        internal ScopeName(String value)
        {
            Value = value;
        }

        public static ScopeName FromString(String input)
        {
            CheckValidity(input);

            return new ScopeName() { Value = input };
        }

        #endregion

        #region Methods

        private static void CheckValidity(String value)
        {
            if(value != null)
            {
                value = value.Trim();
            }

            if (String.IsNullOrEmpty(value) == true)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (value.Length > MaxLength)
            {
                throw new ArgumentOutOfRangeException(nameof(value), $"the name can not exceed {MaxLength} characters");
            }

            if (value.Length < MinLenth)
            {
                throw new ArgumentOutOfRangeException(nameof(value), $"the name can not have less than {MinLenth} characters");
            }

            if (Regex.IsMatch(value, AllowedChars) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(value), $"the name contains invalid characters");
            }
        }

        public static implicit operator string(ScopeName descrption) => descrption.Value;

        #endregion
    }
}
