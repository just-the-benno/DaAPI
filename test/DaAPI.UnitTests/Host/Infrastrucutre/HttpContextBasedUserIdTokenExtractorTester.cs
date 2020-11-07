using DaAPI.Host.Infrastrucutre;
using DaAPI.TestHelper;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Host.Infrastrucutre
{
    public class HttpContextBasedUserIdTokenExtractorTester
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetUserId_OnlySub(Boolean shouldHaveIdpValue)
        {
            Random random = new Random();
            String subClaimValue = random.NextGuid().ToString();

            DefaultHttpContext context = new DefaultHttpContext();
            var claims = new List<Claim>
            {
                new Claim("sub",subClaimValue)
            };

            if (shouldHaveIdpValue == true)
            {
                new Claim("idp", random.GetAlphanumericString());
            }

            var identity = new ClaimsIdentity(claims);
            context.User = new ClaimsPrincipal(identity);

            var contextAccessorMock = new Mock<IHttpContextAccessor>(MockBehavior.Strict);
            contextAccessorMock.SetupGet(x => x.HttpContext).Returns(context).Verifiable();

            var extractor = new HttpContextBasedUserIdTokenExtractor(contextAccessorMock.Object);

            String result = extractor.GetUserId(true);
            Assert.NotNull(result);
            Assert.Equal(subClaimValue, result);

            contextAccessorMock.Verify();
        }

        [Fact]
        public void GetUserId_OnlySub_ButIdpRequested()
        {
            Random random = new Random();
            String subClaimValue = random.NextGuid().ToString();
            String idpValue = random.GetAlphanumericString();

            DefaultHttpContext context = new DefaultHttpContext();
            var claims = new List<Claim>
            {
                new Claim("sub",subClaimValue),
                new Claim("idp", idpValue),
            };

            var identity = new ClaimsIdentity(claims);
            context.User = new ClaimsPrincipal(identity);

            var contextAccessorMock = new Mock<IHttpContextAccessor>(MockBehavior.Strict);
            contextAccessorMock.SetupGet(x => x.HttpContext).Returns(context).Verifiable();

            var extractor = new HttpContextBasedUserIdTokenExtractor(contextAccessorMock.Object);

            String result = extractor.GetUserId(false);
            Assert.NotNull(result);
            Assert.Equal($"{idpValue}:{subClaimValue}", result);

            contextAccessorMock.Verify();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetExternalUserIdentiifer_NoSub(Boolean onlySub)
        {
            DefaultHttpContext context = new DefaultHttpContext();
            var claims = new List<Claim>
            {
            };

            var identity = new ClaimsIdentity(claims);
            context.User = new ClaimsPrincipal(identity);

            var contextAccessorMock = new Mock<IHttpContextAccessor>(MockBehavior.Strict);
            contextAccessorMock.SetupGet(x => x.HttpContext).Returns(context).Verifiable();

            var extractor = new HttpContextBasedUserIdTokenExtractor(contextAccessorMock.Object);

            String result = extractor.GetUserId(onlySub);
            Assert.True(String.IsNullOrEmpty(result));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetExternalUserIdentiifer_NoUser(Boolean onlySub)
        {
            DefaultHttpContext context = new DefaultHttpContext();
            var contextAccessorMock = new Mock<IHttpContextAccessor>(MockBehavior.Strict);
            contextAccessorMock.SetupGet(x => x.HttpContext).Returns(context).Verifiable();

            var extractor = new HttpContextBasedUserIdTokenExtractor(contextAccessorMock.Object);

            String result = extractor.GetUserId(onlySub);
            Assert.True(String.IsNullOrEmpty(result));
        }
    }
}
