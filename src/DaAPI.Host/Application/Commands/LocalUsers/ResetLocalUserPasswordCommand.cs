using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.Host.Application.Commands
{
    public class ResetLocalUserPasswordCommand : IRequest<Boolean>
    {
        public string UserId { get; }
        public string Password { get; }

        public ResetLocalUserPasswordCommand(String userId, String password)
        {
            UserId = userId;
            Password = password;
        }

    }
}
