using DaAPI.Core.Common;
using DaAPI.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaAPI.Core.Packets.DHCPv6
{
    public class DHCPv6PacketUserClassOption : DHCPv6PacketOption, IEquatable<DHCPv6PacketUserClassOption>
    {
        #region Properties

        public IEnumerable<Byte[]> UserClassData { get; private set; }

        #endregion

        #region Constructor

        public DHCPv6PacketUserClassOption(IEnumerable<Byte[]> userClasses) :
            base((UInt16)DHCPv6PacketOptionTypes.UserClass,
                ByteHelper.ConcatBytes(
                    userClasses.Select(x => ByteHelper.ConcatBytes(
                        ByteHelper.GetBytes((UInt16)x.Length), x))))
        {
            UserClassData = new List<Byte[]>(userClasses);
        }

        public static DHCPv6PacketUserClassOption FromByteArray(Byte[] data, Int32 offset)
        {
            UInt16 length = ByteHelper.ConvertToUInt16FromByte(data, offset + 2);

            Int32 pointer = 0;
            List<Byte[]> userClasses = new List<byte[]>();
            while (pointer < length)
            {

                UInt16 classLength = ByteHelper.ConvertToUInt16FromByte(data, offset + 4 + pointer);
                Byte[] classData = ByteHelper.CopyData(data, offset + 4 + pointer + 2, classLength);
                userClasses.Add(classData);

                pointer += 2 + classLength;
            }

            return new DHCPv6PacketUserClassOption(userClasses);
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            String options = String.Empty;
            foreach (var item in UserClassData)
            {
                options += $"{ByteHelper.ToString(item)},";
            }

            return $"users classes: {options}";
        }

        public bool Equals(DHCPv6PacketUserClassOption other)
        {
            return base.Equals(other);
        }

        public override bool Equals(object other) =>
             other is DHCPv6PacketUserClassOption option ? Equals(option) : base.Equals(other);

        public override int GetHashCode() => Data != null ? Data.Length : 0;

        #endregion
    }
}
