using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.Host.Application.Commands.DHCPv4Interfaces
{
    public record CreateDHCPv4InterfaceListenerCommand(String NicId, String IPv4Addres, String Name) : IRequest<Guid?>;
}
