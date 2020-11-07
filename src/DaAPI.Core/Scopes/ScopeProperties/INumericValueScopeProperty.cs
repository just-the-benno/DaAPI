using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes
{
    public enum NumericScopePropertiesValueTypes
    {
        Byte = 1,
        UInt16 = 2,
        UInt32 = 3,
    }

    public interface INumericValueScopeProperty
    {
        #region Properties

         Int64 Value { get; }
         NumericScopePropertiesValueTypes NumericType { get; }

        #endregion

        #region Constructor

        protected static Boolean ValueIsInRange(String rawValue, NumericScopePropertiesValueTypes numericValueType)
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
                case NumericScopePropertiesValueTypes.Byte:
                    result = value >= Byte.MinValue && value <= Byte.MaxValue;
                    break;
                case NumericScopePropertiesValueTypes.UInt16:
                    result = value >= UInt16.MinValue && value <= UInt16.MaxValue;
                    break;
                case NumericScopePropertiesValueTypes.UInt32:
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
