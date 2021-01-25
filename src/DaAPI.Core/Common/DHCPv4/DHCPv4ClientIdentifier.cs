using DaAPI.Core.Helper;
using DaAPI.Core.Packets.DHCPv4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaAPI.Core.Common
{
    public class DHCPv4ClientIdentifier : Value, IEquatable<DHCPv4ClientIdentifier>
    {
        #region Properties

        public Byte[] HwAddress { get; set; }
        public DUID DUID { get; set; }

        #endregion

        #region Constructor

        private DHCPv4ClientIdentifier()
        {

        }

        public static DHCPv4ClientIdentifier FromDuid(DUID duid)
        {
            if (duid == null || duid == DUID.Empty)
            {
                throw new ArgumentNullException(nameof(duid));
            }

            return new DHCPv4ClientIdentifier
            {
                DUID = duid,
                HwAddress = Array.Empty<Byte>(),
            };
        }

        public static DHCPv4ClientIdentifier FromDuid(DUID duid, Byte[] hwAddres)
        {
            if (duid == null || duid == DUID.Empty)
            {
                throw new ArgumentNullException(nameof(duid));
            }

            if (hwAddres == null || hwAddres.Length == 0)
            {
                throw new ArgumentNullException(nameof(hwAddres));
            }

            return new DHCPv4ClientIdentifier
            {
                DUID = duid,
                HwAddress = ByteHelper.CopyData(hwAddres)
            };
        }

        public static DHCPv4ClientIdentifier FromOptionData(byte[] identifierRawVaue)
        {
            if (identifierRawVaue.Length == 7 && 
                identifierRawVaue[0] == (Byte)DHCPv4Packet.DHCPv4PacketHardwareAddressTypes.Ethernet)
            {
                return DHCPv4ClientIdentifier.FromHwAddress(ByteHelper.CopyData(identifierRawVaue,1));
            }
            else
            {
                return DHCPv4ClientIdentifier.FromDuid(
                    DUIDFactory.GetDUID(identifierRawVaue));
            }
        }

        public static DHCPv4ClientIdentifier FromHwAddress(Byte[] hwAddres)
        {
            if (hwAddres == null || hwAddres.Length == 0)
            {
                throw new ArgumentNullException(nameof(hwAddres));
            }

            return new DHCPv4ClientIdentifier
            {
                DUID = DUID.Empty,
                HwAddress = ByteHelper.CopyData(hwAddres)
            };
        }

        public DHCPv4ClientIdentifier AddHardwareAddress(byte[] clientHardwareAddress)
        {
            return new DHCPv4ClientIdentifier
            {
                DUID = this.DUID,
                HwAddress = ByteHelper.CopyData(clientHardwareAddress),
            };
        }

        #endregion

        public override bool Equals(object other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            if(other is DHCPv4ClientIdentifier  == true)
            {
                return Equals((DHCPv4ClientIdentifier)other);
            }
            else
            {
                return false;
            }
        }

        public bool Equals(DHCPv4ClientIdentifier other)
        {
            if(this.DUID != DUID.Empty)
            {
                return this.DUID.Equals(other.DUID);
            }

            return ByteHelper.AreEqual(this.HwAddress, other.HwAddress);
        }

        public static bool operator ==(DHCPv4ClientIdentifier left, DHCPv4ClientIdentifier right) => Equals(left, right);
        public static bool operator !=(DHCPv4ClientIdentifier left, DHCPv4ClientIdentifier right) => !Equals(left, right);

        public override int GetHashCode()
        {
            if (DUID != DUID.Empty)
            {
                return DUID.GetHashCode();
            }

            return this.HwAddress.Sum(x => x);
        }

        public string AsUniqueString()
        {
            Byte[] appendix = HwAddress;
            if (DUID != DUID.Empty)
            {
                appendix = DUID.GetAsByteStream();
            }

            String result = $"DHCPv4Client-{SimpleByteToStringConverter.Convert(appendix)}";
            return result;
        }
    }
}
