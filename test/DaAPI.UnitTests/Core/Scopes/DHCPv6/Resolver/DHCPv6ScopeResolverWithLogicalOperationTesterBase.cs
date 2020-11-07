using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes;
using DaAPI.TestHelper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using static DaAPI.Core.Scopes.ScopeResolverPropertyDescription;

namespace DaAPI.UnitTests.Core.Scopes.DHCPv6.Resolvers
{
    public abstract class DHCPv6ScopeResolverWithLogicalOperationTesterBase
    {
        protected void TestDescription(IScopeResolver<DHCPv6Packet,IPv6Address> resolver, String expectedName)
        {
            ScopeResolverDescription description = resolver.GetDescription();
            Assert.NotNull(description);

            Assert.Equal(expectedName, description.TypeName);

            Assert.NotNull(description.Properties);
            Assert.Single(description.Properties);

            ScopeResolverPropertyDescription propertyDescription = description.Properties.First();
            Assert.Equal("InnerResolvers", propertyDescription.PropertyName);
            Assert.Equal(ScopeResolverPropertyValueTypes.Resolvers, propertyDescription.PropertyValueType);
        }

        protected void CheckMeetsConditions(
            Func<IScopeResolverContainingOtherResolvers<DHCPv6Packet,IPv6Address>> resolverCreater,
            IEnumerable<Tuple<Boolean,Boolean,Boolean>> inputs)
        {
            foreach (var item in inputs)
            {
                var resolver = resolverCreater();

                DHCPv6Packet packet = DHCPv6Packet.AsInner(2, DHCPv6PacketTypes.Solicit, Array.Empty<DHCPv6PacketOption>());

                Mock<IScopeResolver<DHCPv6Packet, IPv6Address>> firstInnerMock = new Mock<IScopeResolver<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict);
                firstInnerMock.Setup(x => x.PacketMeetsCondition(packet)).Returns(item.Item1);

                Mock<IScopeResolver<DHCPv6Packet, IPv6Address>> secondInnerMock = new Mock<IScopeResolver<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict);
                secondInnerMock.Setup(x => x.PacketMeetsCondition(packet)).Returns(item.Item2);

                resolver.AddResolver(firstInnerMock.Object);
                resolver.AddResolver(secondInnerMock.Object);

                Boolean result = resolver.PacketMeetsCondition(packet);
                Assert.Equal(item.Item3, result);
            }

        }
    }
}
