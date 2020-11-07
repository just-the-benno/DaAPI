using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv6.ScopeProperties
{
    public class DHCPv6NumericValueScopeProperty : DHCPv6ScopeProperty, INumericValueScopeProperty
    {
        #region Properties

        public Int64 Value { get; private set; }
        public NumericScopePropertiesValueTypes NumericType { get; private set; }

        #endregion

        #region Constructor

        private DHCPv6NumericValueScopeProperty() : base()
        {

        }

        public DHCPv6NumericValueScopeProperty(
            UInt16 optionIdentifier,
            Int64 value,
            NumericScopePropertiesValueTypes numericType,
            DHCPv6ScopePropertyType valueType) : base(optionIdentifier, valueType)
        {
            Value = value;
            NumericType = numericType;
        }

        public static DHCPv6NumericValueScopeProperty FromRawValue(UInt16 optionIdentifier, String rawValue, NumericScopePropertiesValueTypes numericValueType)
        {
            if (INumericValueScopeProperty.ValueIsInRange(rawValue, numericValueType) == false)
            {
                throw new ArgumentException(nameof(rawValue));
            }

            Int64 value = Convert.ToInt64(rawValue);
            var propertyType = numericValueType switch
            {
                NumericScopePropertiesValueTypes.Byte => DHCPv6ScopePropertyType.Byte,
                NumericScopePropertiesValueTypes.UInt16 => DHCPv6ScopePropertyType.UInt16,
                NumericScopePropertiesValueTypes.UInt32 => DHCPv6ScopePropertyType.UInt32,
                _ => throw new ArgumentException(nameof(numericValueType)),
            };

            return new DHCPv6NumericValueScopeProperty(optionIdentifier, value, numericValueType, propertyType);
        }

        #endregion
    }
}
