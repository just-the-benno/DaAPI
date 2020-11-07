using DaAPI.Host.Infrastrucutre;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DaAPI.Host.Application.Commands
{
    public class CreateLocalUserCommandHandler : IRequestHandler<CreateLocalUserCommand, String>
    {
        private readonly ILocalUserService _userService;
        private readonly ILogger<CreateLocalUserCommandHandler> _logger;

        public CreateLocalUserCommandHandler(
            ILocalUserService userService, ILogger<CreateLocalUserCommandHandler> logger)
        {
            this._userService = userService;
            this._logger = logger;
        }

        public async Task<String> Handle(CreateLocalUserCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Handle started");

            Guid? id = await _userService.CreateUser(request.Username, request.Password); 
            if(id.HasValue == false) { return null; }

            return id.ToString();
        }
    }
}
