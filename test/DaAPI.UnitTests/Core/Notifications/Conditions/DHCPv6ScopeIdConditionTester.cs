using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Notifications.Conditions;
using DaAPI.Core.Notifications.Triggers;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv6;
using DaAPI.Core.Services;
using DaAPI.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static DaAPI.Core.Scopes.DHCPv6.DHCPv6LeaseEvents;
using static DaAPI.Core.Scopes.DHCPv6.DHCPv6ScopeEvents;

namespace DaAPI.UnitTests.Core.Notifications.Conditions
{
    public class DHCPv6ScopeIdConditionTester
    {
        public DHCPv6RootScope GetRootScope()
        {
            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            var scope = new DHCPv6RootScope(Guid.NewGuid(), Mock.Of<IScopeResolverManager<DHCPv6Packet, IPv6Address>>(), factoryMock.Object);
            return scope;

        }

        private DHCPv6ScopeIdNotificationCondition GetCondition(Random random, Boolean includeChildren, IEnumerable<Guid> scopeIds, DHCPv6RootScope rootScope = null)
        {
            String serilzedIdContent = random.GetAlphanumericString();
            string serilizedIncludeChildrenValue = random.GetAlphanumericString();

            Mock<ISerializer> serliazerMock = new Mock<ISerializer>();
            serliazerMock.Setup(x => x.Deserialze<IEnumerable<Guid>>(serilzedIdContent)).Returns(scopeIds).Verifiable();
            serliazerMock.Setup(x => x.Deserialze<Boolean>(serilizedIncludeChildrenValue)).Returns(includeChildren).Verifiable();

            var condition = new DHCPv6ScopeIdNotificationCondition(rootScope ?? GetRootScope(), serliazerMock.Object,
                Mock.Of<ILogger<DHCPv6ScopeIdNotificationCondition>>());

            Dictionary<string, string> propertiesAndValues = new Dictionary<String, String>
            {
                { "IncludesChildren", serilizedIncludeChildrenValue },
                { "ScopeIds",serilzedIdContent},
            };

            condition.ApplyValues(propertiesAndValues);

            return condition;
        }

        private DHCPv6ScopeIdNotificationCondition GetCondition(Random random, Boolean includeChildren, out IEnumerable<Guid> scopeIds, DHCPv6RootScope rootScope = null)
        {
            scopeIds = random.NextGuids();
            return GetCondition(random, includeChildren, scopeIds, rootScope);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ApplyValues_Cycle(Boolean includeChildren)
        {
            Random random = new Random();

            String serilzedIdContent = random.GetAlphanumericString();
            string serilizedIncludeChildrenValue = random.GetAlphanumericString();
            IEnumerable<Guid> expectedAddress = random.NextGuids();

            Mock<ISerializer> serliazerMock = new Mock<ISerializer>();
            serliazerMock.Setup(x => x.Deserialze<IEnumerable<Guid>>(serilzedIdContent)).Returns(expectedAddress).Verifiable();
            serliazerMock.Setup(x => x.Deserialze<Boolean>(serilizedIncludeChildrenValue)).Returns(includeChildren).Verifiable();

            serliazerMock.Setup(x => x.Seralize(includeChildren)).Returns(serilizedIncludeChildrenValue).Verifiable();
            serliazerMock.Setup(x => x.Seralize(expectedAddress)).Returns(serilzedIdContent).Verifiable();

            var condition = new DHCPv6ScopeIdNotificationCondition(GetRootScope(), serliazerMock.Object,
                Mock.Of<ILogger<DHCPv6ScopeIdNotificationCondition>>());

            Dictionary<string, string> propertiesAndValues = new Dictionary<String, String>
            {
                { "IncludesChildren", serilizedIncludeChildrenValue },
                { "ScopeIds",serilzedIdContent},
            };

            Boolean applyResult = condition.ApplyValues(propertiesAndValues);

            Assert.True(applyResult);

            Assert.Equal(expectedAddress, condition.ScopeIds);
            Assert.Equal(includeChildren, condition.IncludesChildren);

            var createModel = condition.ToCreateModel();

            Assert.Equal("DHCPv6ScopeIdNotificationCondition", createModel.Typename);
            Assert.Equal(propertiesAndValues, createModel.PropertiesAndValues, new NonStrictDictionaryComparer<String, String>());
        }

        [Fact]
        public void ApplyValues_Failed()
        {
            Random random = new Random();

            String serilzedIdContent = random.GetAlphanumericString();
            string serilizedIncludeChildrenValue = random.GetAlphanumericString();
            IEnumerable<Guid> expectedAddress = random.NextGuids();

            Mock<ISerializer> serliazerMock = new Mock<ISerializer>();
            serliazerMock.Setup(x => x.Deserialze<IEnumerable<Guid>>(serilzedIdContent)).Returns(expectedAddress).Verifiable();
            serliazerMock.Setup(x => x.Deserialze<Boolean>(serilizedIncludeChildrenValue)).Returns(true).Verifiable();

            var condition = new DHCPv6ScopeIdNotificationCondition(GetRootScope(), serliazerMock.Object,
                Mock.Of<ILogger<DHCPv6ScopeIdNotificationCondition>>());

            Dictionary<string, string> propertiesAndValues = new Dictionary<String, String>
            {
                { "IncludesChildren", serilizedIncludeChildrenValue },
                { "ScopeIds2",serilzedIdContent},
            };

            Boolean applyResult = condition.ApplyValues(propertiesAndValues);

            Assert.False(applyResult);
        }

        [Fact]
        public async Task IsValid_False_WrongTrigger()
        {
            Random random = new Random();

            var trigger = new NotificationPipelineTester.DummyNotifcationTrigger(random.GetAlphanumericString());
            var condition = GetCondition(random, false, out IEnumerable<Guid> _);

            Boolean actual = await condition.IsValid(trigger);
            Assert.False(actual);
        }

        [Fact]
        public async Task IsValid_True_IdIsDirectMember()
        {
            Random random = new Random();

            var condition = GetCondition(random, false, out IEnumerable<Guid> scopeIds);

            Guid scopeId = scopeIds.ElementAt(random.Next(0, scopeIds.Count()));

            var trigger = PrefixEdgeRouterBindingUpdatedTrigger.NoChanges(scopeId);
            Boolean actual = await condition.IsValid(trigger);
            Assert.True(actual);
        }

        [Fact]
        public async Task IsValid_False_NoChildScopes()
        {
            Random random = new Random();

            var condition = GetCondition(random, false, out _);

            var trigger = PrefixEdgeRouterBindingUpdatedTrigger.NoChanges(random.NextGuid());
            Boolean actual = await condition.IsValid(trigger);
            Assert.False(actual);
        }

        private DHCPv6ScopeIdNotificationCondition GetConditionWithScopeTree(Random random, out Guid childGuid)
        {
            Guid grantParentId = random.NextGuid();
            Guid parentId = random.NextGuid();
            childGuid = random.NextGuid();

            var rootScooe = GetRootScope();
            rootScooe.Load(new[] {
                new DHCPv6ScopeAddedEvent
                {
                    Instructions = new DHCPv6ScopeCreateInstruction
                    {
                        Id = grantParentId,
                        Name = "Grant parent",
                    },
                },
                new DHCPv6ScopeAddedEvent
                {
                    Instructions = new DHCPv6ScopeCreateInstruction
                    {
                        Id = parentId,
                        ParentId = grantParentId,
                        Name = " parent - 1",
                    },
                },
                new DHCPv6ScopeAddedEvent
                {
                    Instructions = new DHCPv6ScopeCreateInstruction
                    {
                        Id = random.NextGuid(),
                        ParentId = grantParentId,
                        Name = "parent - 2",
                    },
                },
                new DHCPv6ScopeAddedEvent
                {
                    Instructions = new DHCPv6ScopeCreateInstruction
                    {
                        Id = random.NextGuid(),
                        ParentId = grantParentId,
                        Name = "parent - 3",
                    },
                },
                new DHCPv6ScopeAddedEvent
                {
                    Instructions = new DHCPv6ScopeCreateInstruction
                    {
                        Id = childGuid,
                        ParentId = parentId,
                        Name = "child",
                    },
                },
                new DHCPv6ScopeAddedEvent
                {
                    Instructions = new DHCPv6ScopeCreateInstruction
                    {
                        Id = random.NextGuid(),
                        ParentId = parentId,
                        Name = "child 1 ",
                    },
                },
                new DHCPv6ScopeAddedEvent
                {
                    Instructions = new DHCPv6ScopeCreateInstruction
                    {
                        Id = random.NextGuid(),
                        ParentId = parentId,
                        Name = "child 2 ",
                    },
                }
            });

            var condition = GetCondition(random, true, new[] { childGuid }, rootScooe);
            return condition;
        }

        [Fact]
        public async Task IsValid_True_NoDirectMeber_ButFoundAsChild()
        {
            Random random = new Random();

            var condition = GetConditionWithScopeTree(random, out Guid childId);

            var trigger = PrefixEdgeRouterBindingUpdatedTrigger.NoChanges(childId);
            Boolean actual = await condition.IsValid(trigger);
            Assert.True(actual);
        }

        [Fact]
        public async Task IsValid_False_NoDirectMeber_AndNotChild()
        {
            Random random = new Random();

            var condition = GetConditionWithScopeTree(random, out Guid _);

            var trigger = PrefixEdgeRouterBindingUpdatedTrigger.NoChanges(random.NextGuid());
            Boolean actual = await condition.IsValid(trigger);
            Assert.False(actual);
        }
    }
}
