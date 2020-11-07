using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.Host.Infrastrucutre
{
    public interface IUserIdTokenExtractor
    {
        String GetUserId(Boolean onlySub);
    }
}
