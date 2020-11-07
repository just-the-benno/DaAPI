using DaAPI.Core.Common;
using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv4;
using DaAPI.Core.Scopes.DHCPv6.Resolvers;
using DaAPI.Core.Services;
using DaAPI.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using static DaAPI.Core.Scopes.ScopeResolverPropertyDescription;

namespace DaAPI.UnitTests.Core.Scopes.DHCPv6.Resolvers
{
    public class DHCPv6OrResolverTester : DHCPv6ScopeResolverWithLogicalOperationTesterBase
    {
        [Fact]
        public void GetDescription()
        {
            DHCPv6OrResolver resolver = new DHCPv6OrResolver();
            TestDescription(resolver, "DHCPv6OrResolver");
        }

        [Fact]
        public void HasUniqueIdentifier()
        {
            DHCPv6OrResolver resolver = new DHCPv6OrResolver();
            Assert.False(resolver.HasUniqueIdentifier);
        }

        [Fact]
        public void GetUniqueIdentifier()
        {
            DHCPv6OrResolver resolver = new DHCPv6OrResolver();
            Assert.ThrowsAny<Exception>(() => resolver.GetUniqueIdentifier(null));
        }

        [Fact]
        public void PacketMeetsCondition()
        {

            List<Tuple<Boolean, Boolean, Boolean>> inputs = new List<Tuple<bool, bool, bool>>
            {
                new Tuple<bool, bool, bool>(false,false,false),
                new Tuple<bool, bool, bool>(false,true,true),
                new Tuple<bool, bool, bool>(true,false,true),
                new Tuple<bool, bool, bool>(true,true,true),
            };

            Random random = new Random();

            CheckMeetsConditions(
                () => new DHCPv6OrResolver(),
                inputs
                );
        }

        [Fact]
        public void GetValues()
        {
            DHCPv6AndResolver resolver = new DHCPv6AndResolver();
            Assert.Throws<NotImplementedException>(() => resolver.GetValues());
        }
    }
}
