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
    public class DeleteLocalUserCommandHandler : IRequestHandler<DeleteLocalUserCommand, Boolean>
    {
        private readonly ILocalUserService _userService;
        private readonly ILogger<DeleteLocalUserCommandHandler> _logger;
        private readonly IUserIdTokenExtractor _userIdTokenExtractor;

        public DeleteLocalUserCommandHandler(
            IUserIdTokenExtractor userIdTokenExtractor,
            ILocalUserService userService, ILogger<DeleteLocalUserCommandHandler> logger)
        {
            _userIdTokenExtractor = userIdTokenExtractor;
            this._userService = userService;
            this._logger = logger;
        }


        public async Task<Boolean> Handle(DeleteLocalUserCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Handle started");

            if (request.UserId == _userIdTokenExtractor.GetUserId(true))
            {
                return false;
            }

            if (await _userService.CheckIfUserExists(request.UserId) == false)
            {
                return false;
            }

            if(await _userService.GetUserAmount() == 1)
            {
                return false;
            }

            Boolean result = await _userService.DeleteUser(request.UserId);
            return result;
        }
    }
}
