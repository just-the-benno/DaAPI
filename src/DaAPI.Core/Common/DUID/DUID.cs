using DaAPI.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DaAPI.Core.Common
{
    public abstract class DUID : Value, IEquatable<DUID>
    {
        public static  DUID Empty => null;

        public enum DUIDTypes : ushort
        {
            Unknown = 0,
            LinkLayerAndTime = 1,
            VendorBased = 2,
            LinkLayer = 3,
            Uuid = 4,
            Empty = UInt16.MaxValue,
        }

        #region Properties

        public DUIDTypes Type { get; private set; }
        public Byte[] Value { get; private set; }

        #endregion

        #region constructor and factories

        protected DUID()
        {

        }

        protected DUID(DUIDTypes type, params Byte[][] data)
        {
            Type = type;
            Value = ByteHelper.ConcatBytes(data);
        }

        public bool Equals(DUID other)
        {
            return ByteHelper.AreEqual(this.Value, other.Value);
        }

        #endregion

        #region Methods

        public Byte[] GetAsByteStream()
        {
            Byte[] result = new Byte[Value.Length + 2];

            Byte[] typeCode = ByteHelper.GetBytes((UInt16)Type);
            result[0] = typeCode[0];
            result[1] = typeCode[1];

            for (int i = 0, j = 2; i < Value.Length; i++,j++)
            {
                result[j] = Value[i];
            }

            return result;
        }

        public override bool Equals(Object other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            if(other is DUID == false)
            {
                return false;
            }

            return Equals((DUID)other);
        }

        public override int GetHashCode()
        {
            return (Int32)Type + Value.Sum(x => x);
        }

        public static bool operator ==(DUID left, DUID right) => Equals(left, right);
        public static bool operator !=(DUID left, DUID right) => !Equals(left, right);

        #endregion
    }
}
