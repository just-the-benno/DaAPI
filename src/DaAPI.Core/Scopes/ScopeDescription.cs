using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DaAPI.Core.Scopes
{
    public class ScopeDescription : Value<ScopeDescription>
    {
        #region consts

        public const Int32 MaxLength = 1000;
        public const String AllowedChars = @"^[\w]*((-|\s|\+|&|!|\?|\#|\$|\*)*[\w])*$";

        #endregion

        #region Properties

        public String Value { get; private set; }

        #endregion

        #region constructor and factories

        private ScopeDescription()
        {
        }

        internal ScopeDescription(String value)
        {
            Value = value;
        }

        public static ScopeDescription FromString(String input)
        {
            CheckValidity(input);

            return new ScopeDescription() { Value = input.Trim() };
        }

        public static ScopeDescription Empty => new ScopeDescription() { Value = String.Empty };

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
                throw new ArgumentOutOfRangeException(nameof(value),$"description can not exceed {MaxLength} characters");
            }

            if(Regex.IsMatch(value, AllowedChars) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(value), $"description contains invalid characters");
            }
        }

        public static implicit operator string(ScopeDescription descrption) => descrption.Value;

        #endregion

    }
}
