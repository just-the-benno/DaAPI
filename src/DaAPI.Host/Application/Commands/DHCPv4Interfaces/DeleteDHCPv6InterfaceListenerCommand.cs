using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.Host.Application.Commands.DHCPv4Interfaces
{
    public record DeleteDHCPv4InterfaceListenerCommand(Guid Id) : IRequest<Boolean>;
}
