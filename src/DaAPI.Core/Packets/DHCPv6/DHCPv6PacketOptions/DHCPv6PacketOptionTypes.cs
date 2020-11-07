using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Packets.DHCPv6
{
    public enum DHCPv6PacketOptionTypes : ushort
    {
        ClientIdentifier = 1,
        ServerIdentifer = 2,
        IdentityAssociation_NonTemporary = 3,
        IdentityAssociation_Temporary = 4,
        OptionRequest = 6,
        Preference = 7,
        ElapsedTime = 8,
        RelayMessage = 9,
        Auth = 11,
        ServerUnicast = 12,
        RapitCommit = 14,
        UserClass = 15,
        VendorClass = 16,
        VendorOptions = 17,
        InterfaceId = 18,
        Reconfigure = 19,
        ReconfigureAccepte = 20,
        IdentityAssociation_PrefixDelegation = 25,
        InformationRefreshTime = 32,
        RemoteIdentifier = 37,
        SOL_MAX_RT = 82,
        INF_MAX_RT = 83,

        DNSServer = 23,
        SNTPServer = 31,
        NTPServer = 56,
    }
}
