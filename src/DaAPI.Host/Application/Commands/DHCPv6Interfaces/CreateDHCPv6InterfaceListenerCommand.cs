using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.Host.Application.Commands.DHCPv6Interfaces
{
    public class CreateDHCPv6InterfaceListenerCommand : IRequest<Guid?>
    {
        public String NicId { get; }
        public String IPv6Addres { get; }
        public String Name { get; }

        public CreateDHCPv6InterfaceListenerCommand(String nicId, String ipv6Addres, String name)
        {
            NicId = nicId;
            IPv6Addres = ipv6Addres;
            Name = name;
        }


    }
}
