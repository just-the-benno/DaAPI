using DaAPI.Core.Packets.DHCPv4;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv4
{
    public class DHCPv4TimeScopeProperty : DHCPv4ScopeProperty
    {
        #region Property

        public TimeSpan Value { get; private set; }

        #endregion

        #region Constructor

        public DHCPv4TimeScopeProperty(
          DHCPv4OptionTypes optionIdentifier,
          Boolean isOffset,
          TimeSpan span
            ) : this((Byte)optionIdentifier, isOffset, span)
        {

        }

        public DHCPv4TimeScopeProperty(
            Byte optionIdentifier,
            Boolean isOffset,
            TimeSpan span
            ) : base(optionIdentifier, isOffset == true ? DHCPv4ScopePropertyType.TimeOffset : DHCPv4ScopePropertyType.Time)
        {
            if (isOffset == false && span.TotalMinutes < 0)
            {
                throw new ArgumentException(nameof(span));
            }

            Value = span;
        }

        #endregion
    }
}
