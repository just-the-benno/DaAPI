using DaAPI.Core.Common;
using DaAPI.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DaAPI.Core.Packets.DHCPv6
{
    public abstract class DHCPv6PacketIdentityAsociationOption : DHCPv6PacketOption
    {
        #region MyRegion

        public UInt32 Id { get; set; }
        public IEnumerable<DHCPv6PacketSuboption> Suboptions { get; private set; }

        #endregion

        #region Constructor

        public DHCPv6PacketIdentityAsociationOption(UInt16 code, UInt32 id, Byte[] content, IEnumerable<DHCPv6PacketSuboption> options)
            : base(code,
                  ByteHelper.ConcatBytes(ByteHelper.GetBytes(id),content, 
                  ByteHelper.ConcatBytes(options.Select(x => x.GetByteStream()))))
        {
            Id = id;
            Suboptions = new List<DHCPv6PacketSuboption>(options);
        }

        #endregion

        #region Methods

        protected Byte[] GetIdAsByte()
        {
            return ByteHelper.GetBytes(Id);
        }

        #endregion

    }
}
