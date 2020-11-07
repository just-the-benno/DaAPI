using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.Host.Infrastrucutre
{
    public class HttpContextBasedUserIdTokenExtractor : IUserIdTokenExtractor
    {
        private const String _identityIdClaimType = "sub";
        private const String _idenityProviderClaimType = "idp";

        private readonly IHttpContextAccessor _contextAccessor;

        public HttpContextBasedUserIdTokenExtractor(IHttpContextAccessor contextAccessor)
        {
            this._contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        }

        public string GetUserId(Boolean onlySub)
        {
            HttpContext context = _contextAccessor.HttpContext;
            var principal = context.User;

            var idClaim = principal.Claims.FirstOrDefault(
               x => x.Type == _identityIdClaimType);

            if (idClaim == null)
            {
                return String.Empty;
            }

            var identityProviderClaim = principal.Claims.FirstOrDefault(
                x => x.Type == _idenityProviderClaimType);

            String result = idClaim.Value;
            if (onlySub == false && identityProviderClaim != null)
            {
                result = result.Insert(0, $"{identityProviderClaim.Value}:");
            }

            return result;
        }
    }
}
