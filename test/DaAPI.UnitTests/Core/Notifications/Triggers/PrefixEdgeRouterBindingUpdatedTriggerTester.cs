using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Notifications.Triggers;
using DaAPI.TestHelper;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Core.Notifications.Triggers
{
    public class PrefixEdgeRouterBindingUpdatedTriggerTester
    {
        [Fact]
        public void NoChanges()
        {
            Random random = new Random();
            Guid guid = random.NextGuid();

            var trigger = PrefixEdgeRouterBindingUpdatedTrigger.NoChanges(guid);

            Assert.NotNull(trigger);
            Assert.Equal(guid, trigger.ScopeId);
            Assert.Null(trigger.NewBinding);
            Assert.Null(trigger.OldBinding);
        }

        [Fact]
        public void WithNewBinding()
        {
            Random random = new Random();
            Guid guid = random.NextGuid();

            PrefixBinding prefixBinding = new PrefixBinding(
                IPv6Address.FromString("fe80:1:2::0"), new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(62)),
                IPv6Address.FromString("fe80:FF:2::3"));

            var trigger = PrefixEdgeRouterBindingUpdatedTrigger.WithNewBinding(guid, prefixBinding);

            Assert.NotNull(trigger);
            Assert.Equal(guid, trigger.ScopeId);

            Assert.Null(trigger.OldBinding);
            Assert.Equal(prefixBinding, trigger.NewBinding);
        }

        [Fact]
        public void WithOldBinding()
        {
            Random random = new Random();
            Guid guid = random.NextGuid();

            PrefixBinding prefixBinding = new PrefixBinding(
                IPv6Address.FromString("fe80:1:2::0"), new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(62)),
                IPv6Address.FromString("fe80:FF:2::3"));

            var trigger = PrefixEdgeRouterBindingUpdatedTrigger.WithOldBinding(guid, prefixBinding);

            Assert.NotNull(trigger);
            Assert.Equal(guid, trigger.ScopeId);

            Assert.Equal(prefixBinding, trigger.OldBinding);
            Assert.Null(trigger.NewBinding);
        }

        [Fact]
        public void WithOldAndNewBinding()
        {
            Random random = new Random();
            Guid guid = random.NextGuid();

            PrefixBinding oldPrefixBinding = new PrefixBinding(
                IPv6Address.FromString("fe80:1:2::0"), new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(62)),
                IPv6Address.FromString("fe80:FF:2::3"));

            PrefixBinding newPrefixBinding = new PrefixBinding(
                IPv6Address.FromString("fd80:1:4::0"), new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(64)),
                IPv6Address.FromString("fd80:FE:2::3"));

            var trigger = PrefixEdgeRouterBindingUpdatedTrigger.WithOldAndNewBinding(guid, oldPrefixBinding, newPrefixBinding);

            Assert.NotNull(trigger);
            Assert.Equal(guid, trigger.ScopeId);

            Assert.Equal(oldPrefixBinding, trigger.OldBinding);
            Assert.Equal(newPrefixBinding, trigger.NewBinding);
        }

        [Fact]
        public void GetTypeIdentifier()
        {
            var trigger = PrefixEdgeRouterBindingUpdatedTrigger.NoChanges(Guid.Empty);
            var identifier = trigger.GetTypeIdentifier();

            Assert.Equal("PrefixEdgeRouterBindingUpdatedTrigger", identifier);
        }
    }
}
