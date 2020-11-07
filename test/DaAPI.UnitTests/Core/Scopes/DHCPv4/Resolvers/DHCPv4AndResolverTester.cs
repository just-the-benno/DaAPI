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
    public class DHCPv4AndResolverTester : DHCPv4ScopeResolverWithLogicalOperationTesterBase
    {
        [Fact]
        public void DHCPv4AndResolver_GetDescription()
        {
            DHCPv4AndResolver resolver = new DHCPv4AndResolver(Mock.Of<ISerializer>());

            TestDescription(resolver, "DHCPv4AndResolver");
        }

        [Fact]
        public void DHCPv4AndResolver_PacketMeetsCondition()
        {

            List<Tuple<Boolean, Boolean, Boolean>> inputs = new List<Tuple<bool, bool, bool>>
            {
                new Tuple<bool, bool, bool>(false,false,false),
                new Tuple<bool, bool, bool>(false,true,false),
                new Tuple<bool, bool, bool>(true,false,false),
                new Tuple<bool, bool, bool>(true,true,true),
            };

            Random random = new Random();

            CheckMeetsConditions(
                () => new DHCPv4AndResolver( Mock.Of<ISerializer>()),
                inputs,
                random
                );
        }
    }
}
