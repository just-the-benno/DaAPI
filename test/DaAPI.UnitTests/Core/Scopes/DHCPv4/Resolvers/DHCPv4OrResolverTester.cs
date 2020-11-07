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
    public class DHCPv4OrResolverTester : DHCPv4ScopeResolverWithLogicalOperationTesterBase
    {
        [Fact]
        public void DHCPv4OrResolver_GetDescription()
        {
            DHCPv4OrResolver resolver = new DHCPv4OrResolver(Mock.Of<ISerializer>());

            TestDescription(resolver, "DHCPv4OrResolver");
        }

        [Fact]
        public void DHCPv4OrResolver_PacketMeetsCondition()
        {
            Random random = new Random();

            List<Tuple<Boolean, Boolean, Boolean>> inputs = new List<Tuple<bool, bool, bool>>
            {
                new Tuple<bool, bool, bool>(false,false,false),
                new Tuple<bool, bool, bool>(false,true,true),
                new Tuple<bool, bool, bool>(true,false,true),
                new Tuple<bool, bool, bool>(true,true,true),
            };

            CheckMeetsConditions(
                () => new DHCPv4OrResolver(Mock.Of<ISerializer>()),
                inputs,
                random
                );
        }
    }
}
