using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv6.Resolvers;
using DaAPI.Core.Services;
using DaAPI.TestHelper;
using Moq;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Core.Scopes.DHCPv6.Resolvers
{
    public class DHCPv6PseudoResolverTester
    {
        [Fact]
        public void HasUniqueIdentifier()
        {
            var resolver = new DHCPv6PseudoResolver();
            Assert.False(resolver.HasUniqueIdentifier);
        }

        [Fact]
        public void ImplementsIPseudeResolver()
        {
            var resolver = new DHCPv6PseudoResolver();
            Assert.True(resolver is IPseudoResolver);
        }

        [Fact]
        public void GetUniqueIdentifier()
        {
            var resolver = new DHCPv6PseudoResolver();
            Assert.ThrowsAny<Exception>(() => resolver.GetUniqueIdentifier(null));
        }

        [Fact]
        public void ArePropertiesAndValuesValid()
        {
            var resolver = new DHCPv6PseudoResolver();
            Boolean actual = resolver.ArePropertiesAndValuesValid(null, null);
            Assert.True(actual);
        }

        [Fact]
        public void ApplyValues()
        {
            var resolver = new DHCPv6PseudoResolver();
            resolver.ApplyValues(null, null);

            var values = resolver.GetValues();
            Assert.Empty(values);
        }

        [Fact]
        public void PacketMeetsCondition()
        {
            var resolver = new DHCPv6PseudoResolver();

            Boolean result = resolver.PacketMeetsCondition(null);
            Assert.True(result);
        }

        [Fact]
        public void GetDescription()
        {
            var resolver = new DHCPv6PseudoResolver();
            var actual = resolver.GetDescription();

            Assert.Equal("DHCPv6PseudoResolver", actual.TypeName);
            Assert.Empty(actual.Properties);
        }

    }
}
