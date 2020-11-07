using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.Host.Application.Commands
{
    public class InitilizeServerCommand : IRequest<Boolean>
    {
        public String UserName { get; private set; }
        public String Password { get; private set; }

        public InitilizeServerCommand(String userName, String password)
        {
            this.UserName = userName;
            this.Password = password;
        }
    }
}
