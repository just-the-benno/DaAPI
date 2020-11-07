using DaAPI.Host.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DaAPI.Shared.Responses.LocalUsersResponses.V1;

namespace DaAPI.Host.Infrastrucutre
{
    public class LocalUserManagerService : ILocalUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public LocalUserManagerService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<Guid?> CreateUser(String username, String password)
        {
            Guid userId = Guid.NewGuid();

            var user = new ApplicationUser { UserName = username, Id = userId.ToString() };
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded == true)
            {
                return userId;
            }
            else
            {
                return null;
            }
        }

        public async Task<Int32> GetUserAmount() => await _userManager.Users.CountAsync();

        public async Task<IEnumerable<LocalUserOverview>> GetAllUsersSortedByName()
        {
            var result = await _userManager.Users.OrderBy(x => x.NormalizedUserName).Select(x => new LocalUserOverview
            {
                Id = x.Id,
                Name = x.UserName,
            }).ToListAsync();

            return result;
        }

        public async Task<Boolean> DeleteUser(String userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var result = await _userManager.DeleteAsync(user);

            return result.Succeeded;
        }

        public async Task<Boolean> CheckIfUserExists(String userId)
            => await _userManager.Users.CountAsync(x => x.Id == userId) > 0;

        public async Task<Boolean> ResetPassword(String userId, String password)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) { return false; }

            foreach (var item in _userManager.PasswordValidators)
            {
                var validationResult = await item.ValidateAsync(_userManager, null, password);
                if (validationResult.Succeeded == false)
                {
                    return false;
                }
            }

            IdentityResult removeResult = await _userManager.RemovePasswordAsync(user);
            if (removeResult.Succeeded == false) { return false; }

            IdentityResult result = await _userManager.AddPasswordAsync(user, password);
            return result.Succeeded;
        }
    }
}
