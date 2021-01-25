using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.Host.Application.Commands.DHCPv6Interfaces
{
    public record CreateDHCPv6InterfaceListenerCommand(String NicId, String IPv6Addres, String Name) : IRequest<Guid?>;
}
