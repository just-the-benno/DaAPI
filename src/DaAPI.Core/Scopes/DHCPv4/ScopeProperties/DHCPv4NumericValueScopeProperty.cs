using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv4
{
    public enum DHCPv4NumericValueTypes
    {
        Byte = 1,
        UInt16 = 2,
        UInt32 = 3,
    }

    public class DHCPv4NumericValueScopeProperty : DHCPv4ScopeProperty
    {
        #region Properties

        public Int64 Value { get; private set; }
        public DHCPv4NumericValueTypes NumericType { get; private set; }

        #endregion

        #region Constructor

        private DHCPv4NumericValueScopeProperty() : base()
        {

        }

        public DHCPv4NumericValueScopeProperty(
            Byte optionIdentifier, 
            Int64 value,
            DHCPv4NumericValueTypes numericType,
            DHCPv4ScopePropertyType valueType) : base (optionIdentifier, valueType)
        {
            Value = value;
            NumericType = numericType;
        }

        public static DHCPv4NumericValueScopeProperty FromRawValue(Byte optionIdentifier, String rawValue, DHCPv4NumericValueTypes numericValueType)
        {
            if(ValueIsInRange(rawValue,numericValueType) == false)
            {
                throw new ArgumentException(nameof(rawValue));
            }

            Int64 value = Convert.ToInt64(rawValue);

            DHCPv4ScopePropertyType propertyType = DHCPv4ScopePropertyType.Boolean;

            switch (numericValueType)
            {
                case DHCPv4NumericValueTypes.Byte:
                    propertyType = DHCPv4ScopePropertyType.Byte;
                    break;
                case DHCPv4NumericValueTypes.UInt16:
                    propertyType = DHCPv4ScopePropertyType.UInt16;
                    break;
                case DHCPv4NumericValueTypes.UInt32:
                    propertyType = DHCPv4ScopePropertyType.UInt32;
                    break;
                default:
                    break;
            }

            return new DHCPv4NumericValueScopeProperty(optionIdentifier, value, numericValueType, propertyType);
        }

        public static Boolean ValueIsInRange(String rawValue, DHCPv4NumericValueTypes numericValueType)
        {
            Int64 value;
            try
            {
                value = Convert.ToInt64(rawValue);
            }
            catch (Exception)
            {
                return false;
            }

            Boolean result = false;
            switch (numericValueType)
            {
                case DHCPv4NumericValueTypes.Byte:
                    result = value >= Byte.MinValue && value <= Byte.MaxValue;
                    break;
                case DHCPv4NumericValueTypes.UInt16:
                    result = value >= UInt16.MinValue && value <= UInt16.MaxValue;
                    break;
                case DHCPv4NumericValueTypes.UInt32:
                    result = value >= UInt32.MinValue && value <= UInt32.MaxValue;
                    break;
                default:
                    break;
            }

            return result;
        }

        #endregion
    }
}
