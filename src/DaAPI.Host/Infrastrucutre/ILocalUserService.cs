using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DaAPI.Shared.Responses.LocalUsersResponses.V1;

namespace DaAPI.Host.Infrastrucutre
{
    public interface ILocalUserService
    {
        Task<Int32> GetUserAmount();
        Task<Guid?> CreateUser(String username, String password);
        Task<IEnumerable<LocalUserOverview>> GetAllUsersSortedByName();
        Task<Boolean> DeleteUser(String userId);
        Task<Boolean> CheckIfUserExists(String userId);
        Task<Boolean> ResetPassword(String userId, String password);
    }
}
