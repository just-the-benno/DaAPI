using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Infrastructure.InterfaceManager
{
    public interface IDHCPv4InterfaceManager
    {
        void OpenSocket(String interfaceId);
        void CloseSocket(String interfaceId);
        void CloseAllSockets();
    }
}
