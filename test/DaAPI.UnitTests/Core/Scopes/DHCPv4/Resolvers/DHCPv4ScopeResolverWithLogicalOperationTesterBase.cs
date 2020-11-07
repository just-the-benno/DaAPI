using DaAPI.Core.Common;
using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv4;
using DaAPI.TestHelper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using static DaAPI.Core.Scopes.ScopeResolverPropertyDescription;

namespace DaAPI.UnitTests.Core.Scopes.DHCPv4.Resolvers
{
    public abstract class DHCPv4ScopeResolverWithLogicalOperationTesterBase
    {
        protected void TestDescription(IDHCPv4ScopeResolver resolver, String expectedName)
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
            Func<IDHCPv4ScopeResolverContainingOtherResolvers> resolverCreater,
            IEnumerable<Tuple<Boolean,Boolean,Boolean>> inputs, Random random)
        {
            foreach (var item in inputs)
            {
                IDHCPv4ScopeResolverContainingOtherResolvers resolver = resolverCreater();

                DHCPv4Packet packet = new DHCPv4Packet(
                    new IPv4HeaderInformation(random.GetIPv4Address(), random.GetIPv4Address()),
                    random.NextBytes(6),
                    (UInt32)random.Next(),
                    IPv4Address.Empty,
                    IPv4Address.Empty,
                    IPv4Address.Empty
                    );

                Mock<IDHCPv4ScopeResolver> firstInnerMock = new Mock<IDHCPv4ScopeResolver>(MockBehavior.Strict);
                firstInnerMock.Setup(x => x.PacketMeetsCondition(packet)).Returns(item.Item1);

                Mock<IDHCPv4ScopeResolver> secondInnerMock = new Mock<IDHCPv4ScopeResolver>(MockBehavior.Strict);
                secondInnerMock.Setup(x => x.PacketMeetsCondition(packet)).Returns(item.Item2);

                resolver.AddResolver(firstInnerMock.Object);
                resolver.AddResolver(secondInnerMock.Object);

                Boolean result = resolver.PacketMeetsCondition(packet);
                Assert.Equal(item.Item3, result);
            }

        }
    }
}
