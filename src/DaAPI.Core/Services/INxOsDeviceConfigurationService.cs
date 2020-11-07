using DaAPI.Core.Common.DHCPv6;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DaAPI.Core.Services
{
    public interface INxOsDeviceConfigurationService
    {
        Task<Boolean> Connect(String iPAddress, String username, String password);
        Task<Boolean> RemoveIPv6StaticRoute(IPv6Address prefix, IPv6SubnetMaskIdentifier length, IPv6Address host);
        Task<Boolean> AddIPv6StaticRoute(IPv6Address prefix, IPv6SubnetMaskIdentifier length, IPv6Address host);
    }
}
