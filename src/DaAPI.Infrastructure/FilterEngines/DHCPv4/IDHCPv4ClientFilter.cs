using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.FilterEngines.DHCPv4
{
    public interface IDHCPv4ClientFilter
    {
        Task<Boolean> FilterClientByHardwareAddress(Byte[] hardwareAddress);
        Task<Boolean> FilterClientByClientIdentifier(Byte[] identifier);

    }
}
