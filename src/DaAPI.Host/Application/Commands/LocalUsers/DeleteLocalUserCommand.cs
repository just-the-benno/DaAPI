using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.Host.Application.Commands
{
    public class DeleteLocalUserCommand : IRequest<Boolean>
    {
        public string UserId { get; }

        public DeleteLocalUserCommand(String userId)
        {
            UserId = userId;
        }

    }
}
