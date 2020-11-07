using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.Host.Application.Commands
{
    public class CreateLocalUserCommand : IRequest<String>
    {
        public String Username { get; }
        public String Password { get; }

        public CreateLocalUserCommand(String username, String password)
        {
            Username = username;
            Password = password;
        }
    }
}
