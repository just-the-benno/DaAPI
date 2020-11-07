using DaAPI.Core.Common;
using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv4;
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

namespace DaAPI.UnitTests.Core.Scopes.DHCPv4.Resolvers
{
    public class DHCPv4ExclusiveOrResolverTester : DHCPv4ScopeResolverWithLogicalOperationTesterBase
    {
        [Fact]
        public void DHCPv4ExclusiveOrResolver_GetDescription()
        {
            DHCPv4ExclusiveOrResolver resolver = new DHCPv4ExclusiveOrResolver(Mock.Of<ISerializer>());

            TestDescription(resolver, "DHCPv4ExclusiveOrResolver");
        }

        [Fact]
        public void DHCPv4ExclusiveOrResolver_PacketMeetsCondition()
        {
            Random random = new Random();

            List<Tuple<Boolean, Boolean, Boolean>> inputs = new List<Tuple<bool, bool, bool>>
            {
                new Tuple<bool, bool, bool>(false,false,false),
                new Tuple<bool, bool, bool>(false,true,true),
                new Tuple<bool, bool, bool>(true,false,true),
                new Tuple<bool, bool, bool>(true,true,false),
            };

            CheckMeetsConditions(
                () => new DHCPv4ExclusiveOrResolver(Mock.Of<ISerializer>()),
                inputs,
                random
                );
        }
    }
}
