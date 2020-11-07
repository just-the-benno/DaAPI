using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.Host.Application.Commands.DHCPv6Interfaces
{
    public class DeleteDHCPv6InterfaceListenerCommand : IRequest<Boolean>
    {
        public Guid Id { get; }

        public DeleteDHCPv6InterfaceListenerCommand(Guid id)
        {
            Id = id;
        }
    }
}
