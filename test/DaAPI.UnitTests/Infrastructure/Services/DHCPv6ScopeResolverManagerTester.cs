using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv6.Resolvers;
using DaAPI.Core.Services;
using DaAPI.Infrastructure.Services;
using DaAPI.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using Xunit;
using static DaAPI.Core.Scopes.ScopeResolverPropertyDescription;

namespace DaAPI.UnitTests.Infrastructure.Services
{
    public class DHCPv6ScopeResolverManagerTester
    {
        [Fact]
        public void GetRegisterResolver_Default()
        {
            DHCPv6ScopeResolverManager manager = new DHCPv6ScopeResolverManager(
                Mock.Of<ISerializer>(MockBehavior.Strict),
                Mock.Of<ILogger<DHCPv6ScopeResolverManager>>());

            List<ScopeResolverDescription> descriptions = new List<ScopeResolverDescription>
            {
                new DHCPv6AndResolver().GetDescription(),
                new DHCPv6OrResolver().GetDescription(),
                new DHCPv6RemoteIdentifierEnterpriseNumberResolver(Mock.Of<ILogger<DHCPv6RemoteIdentifierEnterpriseNumberResolver>>()).GetDescription(),
                new DHCPv6RelayAgentSubnetResolver().GetDescription(),
                new DHCPv6RelayAgentResolver().GetDescription(),
                new DHCPv6PseudoResolver().GetDescription(),
                new DHCPv6MilegateResolver().GetDescription(),
                new DHCPv6PeerAddressResolver().GetDescription(),
                new DHCPv6ClientDUIDResolver().GetDescription(),
                new DHCPv6SimpleZyxelIESResolver().GetDescription(),
            };

            IEnumerable<ScopeResolverDescription> result = manager.GetRegisterResolverDescription();
            Assert.NotNull(result);

            for (int i = 0; i < descriptions.Count; i++)
            {
                ScopeResolverDescription expected = descriptions[i];
                ScopeResolverDescription actual = result.ElementAt(i);
                
                Assert.Equal(expected, actual);
            }

            Assert.Equal(descriptions.Count, result.Count());
        }

        [Fact]
        public void GetRegisterResolver_NonEmpty()
        {
            DHCPv6ScopeResolverManager manager = new DHCPv6ScopeResolverManager(
                Mock.Of<ISerializer>(MockBehavior.Strict),
                Mock.Of<ILogger<DHCPv6ScopeResolverManager>>());

            Random random = new Random();

            Int32 amount = random.Next(30, 100);
            List<ScopeResolverDescription> expectedResult = new List<ScopeResolverDescription>(manager.GetRegisterResolverDescription());
            for (int i = 0; i < amount; i++)
            {
                Mock<IScopeResolver<DHCPv6Packet, IPv6Address>> resolverMock =
                    new Mock<IScopeResolver<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict);

                ScopeResolverDescription description = new ScopeResolverDescription(
                    $"lower-resolvername-{random.Next()}", new List<ScopeResolverPropertyDescription>()
                    {
                       new ScopeResolverPropertyDescription($"property-name-{random.Next()}", ScopeResolverPropertyValueTypes.UInt32 ),
                       new ScopeResolverPropertyDescription($"property-name-{random.Next()}", ScopeResolverPropertyValueTypes.NullableUInt32 ),
                    });

                resolverMock.Setup(x => x.GetDescription()).Returns(description);

                expectedResult.Add(description);
                manager.AddOrUpdateScopeResolver(description.TypeName, () => resolverMock.Object);
            }


            IEnumerable<ScopeResolverDescription> result = manager.GetRegisterResolverDescription();
            Assert.NotNull(result);
            Assert.Equal(expectedResult, result);
        }

        private void GenerateResolverTree(
            Mock<ISerializer> serilzierMock,
            Boolean applyValues,
    Mock<IScopeResolverContainingOtherResolvers<DHCPv6Packet, IPv6Address>> parent,
    CreateScopeResolverInformation parentCreateModel,
    Dictionary<String, Tuple<Mock<IScopeResolverContainingOtherResolvers<DHCPv6Packet, IPv6Address>>, CreateScopeResolverInformation, List<IScopeResolver<DHCPv6Packet, IPv6Address>>>> typeNameToResolverContainingOtherMapper,
    Dictionary<String, Tuple<Mock<IScopeResolver<DHCPv6Packet, IPv6Address>>, CreateScopeResolverInformation>> typeNameToResolverMapper,
     Random random, Double value)
        {
            List<CreateScopeResolverInformation> childCreateModels = new List<CreateScopeResolverInformation>();
            parent.Setup(x => x.ExtractResolverCreateModels(parentCreateModel, serilzierMock.Object)).Returns(childCreateModels);

            if (value > 1)
            {
                return;
            }

            Int32 childAmount = random.Next(3, 10);

            for (int i = 0; i < childAmount; i++)
            {
                CreateScopeResolverInformation childCreateModel = new CreateScopeResolverInformation
                {
                    Typename = $"typename-{random.Next()}",
                };

                childCreateModels.Add(childCreateModel);

                Int32 propertyAmount = random.Next(3, 30);
                for (int j = 0; j < propertyAmount; j++)
                {
                    childCreateModel.PropertiesAndValues.Add($"property-{j + 1}", $"{random.Next()}");
                }

                Boolean containingOthers = random.NextDouble() > 0.5;
                IScopeResolver<DHCPv6Packet, IPv6Address> childElement = null;
                if (containingOthers == true)
                {
                    Mock<IScopeResolverContainingOtherResolvers<DHCPv6Packet, IPv6Address>> child = new Mock<IScopeResolverContainingOtherResolvers<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict);
                    if (applyValues == true)
                    {
                        child.Setup(x => x.ApplyValues(childCreateModel.PropertiesAndValues, serilzierMock.Object));
                    }
                    else
                    {
                        child.Setup(x => x.ArePropertiesAndValuesValid(childCreateModel.PropertiesAndValues, serilzierMock.Object));
                    }

                    childElement = child.Object;

                    typeNameToResolverContainingOtherMapper.Add(childCreateModel.Typename,
                       new Tuple<Mock<IScopeResolverContainingOtherResolvers<DHCPv6Packet, IPv6Address>>, CreateScopeResolverInformation, List<IScopeResolver<DHCPv6Packet, IPv6Address>>>(
                           child,
                           childCreateModel, new List<IScopeResolver<DHCPv6Packet, IPv6Address>>()));

                    GenerateResolverTree(serilzierMock, applyValues, child, childCreateModel, typeNameToResolverContainingOtherMapper, typeNameToResolverMapper, random, value + random.NextDouble());

                }
                else
                {
                    Mock<IScopeResolver<DHCPv6Packet, IPv6Address>> resolverMock = new Mock<IScopeResolver<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict);
                    if (applyValues == true)
                    {
                        resolverMock.Setup(x => x.ApplyValues(childCreateModel.PropertiesAndValues, serilzierMock.Object));
                    }
                    else
                    {
                        resolverMock.Setup(x => x.ArePropertiesAndValuesValid(childCreateModel.PropertiesAndValues, serilzierMock.Object));
                    }

                    childElement = resolverMock.Object;
                    typeNameToResolverMapper.Add(childCreateModel.Typename, new Tuple<Mock<IScopeResolver<DHCPv6Packet, IPv6Address>>, CreateScopeResolverInformation>(resolverMock, childCreateModel));
                }

                parent.Setup(x => x.AddResolver(childElement)).Returns(true);
                typeNameToResolverContainingOtherMapper[parentCreateModel.Typename].Item3.Add(childElement);
            }

        }

        [Fact]
        public void InitializeResolver_Flat()
        {
            Random random = new Random();
            CreateScopeResolverInformation inputModel = new CreateScopeResolverInformation();
            Int32 propertyAmount = random.Next(3, 30);
            for (int i = 0; i < propertyAmount; i++)
            {
                inputModel.PropertiesAndValues.Add($"property-{i + 1}", $"{random.Next()}");
            }

            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);

            DHCPv6ScopeResolverManager scopeManager = new DHCPv6ScopeResolverManager(serializerMock.Object, Mock.Of<ILogger<DHCPv6ScopeResolverManager>>());

            Mock<IScopeResolver<DHCPv6Packet, IPv6Address>> resolverMock = new Mock<IScopeResolver<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict);
            resolverMock.Setup(x => x.ApplyValues(inputModel.PropertiesAndValues, serializerMock.Object)).Verifiable();

            String typeName = $"typname-{random.Next()}";
            scopeManager.AddOrUpdateScopeResolver(typeName, () => resolverMock.Object);
            inputModel.Typename = typeName;

            IScopeResolver<DHCPv6Packet, IPv6Address> actual = scopeManager.InitializeResolver(inputModel);
            Assert.Equal(resolverMock.Object, actual);

            resolverMock.Verify();
        }

        [Fact]
        public void InitializeResolver_Fail_TypeNotFound()
        {
            Random random = new Random();
            CreateScopeResolverInformation inputModel = new CreateScopeResolverInformation();

            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);

            DHCPv6ScopeResolverManager scopeManager = new DHCPv6ScopeResolverManager(Mock.Of<ISerializer>(MockBehavior.Strict), Mock.Of<ILogger<DHCPv6ScopeResolverManager>>());

            String typeName = $"typname-{random.Next()}";
            inputModel.Typename = typeName;

            Exception exp = Assert.Throws<Exception>(() => scopeManager.InitializeResolver(inputModel));
            Assert.NotNull(exp);
        }

        [Fact]
        public void InitializeResolver_ComplexStructure()
        {
            for (int seed = 1; seed <= 10; seed++)
            {
                Random random = new Random(seed);

                CreateScopeResolverInformation inputModel = new CreateScopeResolverInformation
                {
                    Typename = $"typename-{random.Next()}",
                };

                Int32 propertyAmount = random.Next(3, 30);
                for (int i = 0; i < propertyAmount; i++)
                {
                    inputModel.PropertiesAndValues.Add($"property-{i + 1}", $"{random.Next()}");
                }

                Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);

                DHCPv6ScopeResolverManager scopeManager = new DHCPv6ScopeResolverManager(serializerMock.Object, Mock.Of<ILogger<DHCPv6ScopeResolverManager>>());

                Mock<IScopeResolverContainingOtherResolvers<DHCPv6Packet, IPv6Address>> resolverMock = new Mock<IScopeResolverContainingOtherResolvers<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict);
                resolverMock.Setup(x => x.ApplyValues(inputModel.PropertiesAndValues, serializerMock.Object));

                Dictionary<String, Tuple<Mock<IScopeResolverContainingOtherResolvers<DHCPv6Packet, IPv6Address>>, CreateScopeResolverInformation, List<IScopeResolver<DHCPv6Packet, IPv6Address>>>> mocksWithChildren =
                    new Dictionary<string, Tuple<Mock<IScopeResolverContainingOtherResolvers<DHCPv6Packet, IPv6Address>>, CreateScopeResolverInformation, List<IScopeResolver<DHCPv6Packet, IPv6Address>>>>
                    {
                        {
                            inputModel.Typename,
                            new Tuple<Mock<IScopeResolverContainingOtherResolvers<DHCPv6Packet, IPv6Address>>, CreateScopeResolverInformation, List<IScopeResolver<DHCPv6Packet, IPv6Address>>>(
                    resolverMock, inputModel, new List<IScopeResolver<DHCPv6Packet, IPv6Address>>())
                        }
                    };

                Dictionary<String, Tuple<Mock<IScopeResolver<DHCPv6Packet, IPv6Address>>, CreateScopeResolverInformation>> mocksWithoutChildren =
                    new Dictionary<string, Tuple<Mock<IScopeResolver<DHCPv6Packet, IPv6Address>>, CreateScopeResolverInformation>>();

                GenerateResolverTree(serializerMock, true, resolverMock, inputModel, mocksWithChildren, mocksWithoutChildren, random, random.NextDouble());

                foreach (var item in mocksWithChildren)
                {
                    scopeManager.AddOrUpdateScopeResolver(item.Key, () => item.Value.Item1.Object);
                }

                foreach (var item in mocksWithoutChildren)
                {
                    scopeManager.AddOrUpdateScopeResolver(item.Key, () => item.Value.Item1.Object);
                }

                IScopeResolver<DHCPv6Packet, IPv6Address> actual = scopeManager.InitializeResolver(inputModel);
                Assert.Equal(resolverMock.Object, actual);


                foreach (var item in mocksWithChildren)
                {
                    item.Value.Item1.Verify(x => x.ApplyValues(item.Value.Item2.PropertiesAndValues, serializerMock.Object), Times.Once());
                    item.Value.Item1.Verify(x => x.ExtractResolverCreateModels(item.Value.Item2, serializerMock.Object), Times.Once());
                    foreach (var scopeResolver in item.Value.Item3)
                    {
                        item.Value.Item1.Verify(x => x.AddResolver(scopeResolver), Times.Once());
                    }
                }

                foreach (var item in mocksWithoutChildren)
                {
                    item.Value.Item1.Verify(x => x.ApplyValues(item.Value.Item2.PropertiesAndValues, serializerMock.Object), Times.Once());
                }
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsResolverInformationValid_Flat_IsValid(Boolean shouldBeValid)
        {
            Random random = new Random();

            CreateScopeResolverInformation inputModel = new CreateScopeResolverInformation();
            Int32 propertyAmount = random.Next(3, 30);
            for (int i = 0; i < propertyAmount; i++)
            {
                inputModel.PropertiesAndValues.Add($"property-{i + 1}", $"{random.Next()}");
            }

            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            DHCPv6ScopeResolverManager scopeManager = new DHCPv6ScopeResolverManager(serializerMock.Object, Mock.Of<ILogger<DHCPv6ScopeResolverManager>>());

            Mock<IScopeResolver<DHCPv6Packet, IPv6Address>> resolverMock = new Mock<IScopeResolver<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict);
            resolverMock.Setup(x => x.ArePropertiesAndValuesValid(inputModel.PropertiesAndValues, serializerMock.Object)).Returns(shouldBeValid);

            String typeName = $"typname-{random.Next()}";
            scopeManager.AddOrUpdateScopeResolver(typeName, () => resolverMock.Object);
            inputModel.Typename = typeName;

            Boolean actual = scopeManager.IsResolverInformationValid(inputModel);
            Assert.Equal(shouldBeValid, actual);

            resolverMock.Verify(x => x.ArePropertiesAndValuesValid(inputModel.PropertiesAndValues, serializerMock.Object), Times.Once());
        }

        [Fact]
        public void IsResolverInformationValid_InvalidModel()
        {
            List<CreateScopeResolverInformation> invalidModels = new List<CreateScopeResolverInformation>
            {
                null,
                new CreateScopeResolverInformation(),
                new CreateScopeResolverInformation { Typename = "" },
                new CreateScopeResolverInformation { Typename = String.Empty },
            };

            DHCPv6ScopeResolverManager scopeManager = new DHCPv6ScopeResolverManager(Mock.Of<ISerializer>(MockBehavior.Strict), Mock.Of<ILogger<DHCPv6ScopeResolverManager>>());

            foreach (CreateScopeResolverInformation item in invalidModels)
            {
                Boolean actual = scopeManager.IsResolverInformationValid(item);
                Assert.False(actual);
            }
        }

        [Fact]
        public void IsResolverInformationValid_ResolverTypeNotFound()
        {
            Random random = new Random();
            CreateScopeResolverInformation inputModel = new CreateScopeResolverInformation();

            DHCPv6ScopeResolverManager scopeManager = new DHCPv6ScopeResolverManager(Mock.Of<ISerializer>(MockBehavior.Strict), Mock.Of<ILogger<DHCPv6ScopeResolverManager>>());

            String typeName = $"typname-{random.Next()}";
            inputModel.Typename = typeName;

            Boolean actual = scopeManager.IsResolverInformationValid(inputModel);
            Assert.False(actual);
        }

        private void GenerateResolverTreeForValidation(
            Mock<ISerializer> seralizerMock,
         Mock<IScopeResolverContainingOtherResolvers<DHCPv6Packet, IPv6Address>> parent,
         CreateScopeResolverInformation parentCreateModel,
         Dictionary<String, Tuple<Mock<IScopeResolverContainingOtherResolvers<DHCPv6Packet, IPv6Address>>, CreateScopeResolverInformation>> typeNameToResolverContainingOtherMapper,
         Dictionary<String, Tuple<Mock<IScopeResolver<DHCPv6Packet, IPv6Address>>, CreateScopeResolverInformation>> typeNameToResolverMapper,
          Random random, Double value)
        {
            List<CreateScopeResolverInformation> childCreateModels = new List<CreateScopeResolverInformation>();
            parent.Setup(x => x.ExtractResolverCreateModels(parentCreateModel, seralizerMock.Object)).Returns(childCreateModels);

            if (value > 1)
            {
                return;
            }

            Int32 childAmount = random.Next(3, 10);

            for (int i = 0; i < childAmount; i++)
            {
                CreateScopeResolverInformation childCreateModel = new CreateScopeResolverInformation
                {
                    Typename = $"typename-{random.Next()}",
                };

                childCreateModels.Add(childCreateModel);

                Int32 propertyAmount = random.Next(3, 30);
                for (int j = 0; j < propertyAmount; j++)
                {
                    childCreateModel.PropertiesAndValues.Add($"property-{j + 1}", $"{random.Next()}");
                }

                Boolean containingOthers = random.NextDouble() > 0.5;
                IScopeResolver<DHCPv6Packet, IPv6Address> childElement = null;
                if (containingOthers == true)
                {
                    Mock<IScopeResolverContainingOtherResolvers<DHCPv6Packet, IPv6Address>> child = new Mock<IScopeResolverContainingOtherResolvers<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict);
                    child.Setup(x => x.ArePropertiesAndValuesValid(childCreateModel.PropertiesAndValues, seralizerMock.Object)).Returns(true);

                    childElement = child.Object;

                    typeNameToResolverContainingOtherMapper.Add(childCreateModel.Typename,
                       new Tuple<Mock<IScopeResolverContainingOtherResolvers<DHCPv6Packet, IPv6Address>>, CreateScopeResolverInformation>(
                           child,
                           childCreateModel));

                    GenerateResolverTreeForValidation(seralizerMock, child, childCreateModel, typeNameToResolverContainingOtherMapper, typeNameToResolverMapper, random, value + random.NextDouble());

                }
                else
                {
                    Mock<IScopeResolver<DHCPv6Packet, IPv6Address>> resolverMock = new Mock<IScopeResolver<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict);
                    resolverMock.Setup(x => x.ArePropertiesAndValuesValid(childCreateModel.PropertiesAndValues, seralizerMock.Object)).Returns(true);

                    childElement = resolverMock.Object;
                    typeNameToResolverMapper.Add(childCreateModel.Typename, new Tuple<Mock<IScopeResolver<DHCPv6Packet, IPv6Address>>, CreateScopeResolverInformation>(resolverMock, childCreateModel));
                }

                parent.Setup(x => x.AddResolver(childElement)).Returns(true);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsResolverInformationValid_ComplexStructure(Boolean shouldBeValid)
        {
            for (int seed = 1; seed <= 10; seed++)
            {
                Random random = new Random(seed);

                CreateScopeResolverInformation inputModel = new CreateScopeResolverInformation
                {
                    Typename = $"typename-{random.Next()}",
                };

                Int32 propertyAmount = random.Next(3, 30);
                for (int i = 0; i < propertyAmount; i++)
                {
                    inputModel.PropertiesAndValues.Add($"property-{i + 1}", $"{random.Next()}");
                }

                Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
                DHCPv6ScopeResolverManager scopeManager = new DHCPv6ScopeResolverManager(serializerMock.Object, Mock.Of<ILogger<DHCPv6ScopeResolverManager>>());

                Mock<IScopeResolverContainingOtherResolvers<DHCPv6Packet, IPv6Address>> resolverMock = new Mock<IScopeResolverContainingOtherResolvers<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict);
                resolverMock.Setup(x => x.ArePropertiesAndValuesValid(inputModel.PropertiesAndValues, serializerMock.Object)).Returns(true);

                Dictionary<String, Tuple<Mock<IScopeResolverContainingOtherResolvers<DHCPv6Packet, IPv6Address>>, CreateScopeResolverInformation>> mocksWithChildren =
                                  new Dictionary<string, Tuple<Mock<IScopeResolverContainingOtherResolvers<DHCPv6Packet, IPv6Address>>, CreateScopeResolverInformation>>
                                  {
                                      {
                                          inputModel.Typename,
                                          new Tuple<Mock<IScopeResolverContainingOtherResolvers<DHCPv6Packet, IPv6Address>>, CreateScopeResolverInformation>(
                    resolverMock, inputModel)
                                      }
                                  };

                Dictionary<String, Tuple<Mock<IScopeResolver<DHCPv6Packet, IPv6Address>>, CreateScopeResolverInformation>> mocksWithoutChildren = new Dictionary<string, Tuple<Mock<IScopeResolver<DHCPv6Packet, IPv6Address>>, CreateScopeResolverInformation>>
                     ();

                GenerateResolverTreeForValidation(serializerMock, resolverMock, inputModel, mocksWithChildren, mocksWithoutChildren, random, random.NextDouble());

                foreach (var item in mocksWithChildren)
                {
                    scopeManager.AddOrUpdateScopeResolver(item.Key, () => item.Value.Item1.Object);
                }

                foreach (var item in mocksWithoutChildren)
                {
                    scopeManager.AddOrUpdateScopeResolver(item.Key, () => item.Value.Item1.Object);
                }

                if (shouldBeValid == false)
                {
                    if (random.NextDouble() > 0.5)
                    {
                        var item = mocksWithChildren.ElementAt(random.Next(0, mocksWithChildren.Count));
                        item.Value.Item1.Setup(x => x.ArePropertiesAndValuesValid(item.Value.Item2.PropertiesAndValues, serializerMock.Object)).Returns(false);
                    }
                    else
                    {
                        var item = mocksWithoutChildren.ElementAt(random.Next(0, mocksWithoutChildren.Count));
                        item.Value.Item1.Setup(x => x.ArePropertiesAndValuesValid(item.Value.Item2.PropertiesAndValues, serializerMock.Object)).Returns(false);
                    }
                }

                Boolean actual = scopeManager.IsResolverInformationValid(inputModel);
                Assert.Equal(shouldBeValid, actual);

                var times = Times.AtMost(1);
                if (shouldBeValid == true)
                {
                    times = Times.Exactly(1);
                }

                foreach (var item in mocksWithChildren)
                {
                    item.Value.Item1.Verify(x => x.ArePropertiesAndValuesValid(item.Value.Item2.PropertiesAndValues, serializerMock.Object), times);
                    item.Value.Item1.Verify(x => x.ExtractResolverCreateModels(item.Value.Item2, serializerMock.Object), times);
                }

                foreach (var item in mocksWithoutChildren)
                {
                    item.Value.Item1.Verify(x => x.ArePropertiesAndValuesValid(item.Value.Item2.PropertiesAndValues, serializerMock.Object), times);
                }
            }
        }

        [Fact]
        public void InitializeResolver_NotMocked()
        {
            Random random = new Random();
            UInt32 enterpriseId = random.NextUInt32();
            Int32 relayAgentIndex = random.Next();

            CreateScopeResolverInformation createScopeResolverInformation = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv6AndResolver),
                PropertiesAndValues = new Dictionary<String, String>
                {
                    { "InnerResolvers", JsonConvert.SerializeObject(new CreateScopeResolverInformation[]
                        {
                        new CreateScopeResolverInformation {
                            Typename = nameof(DHCPv6AndResolver),
                            PropertiesAndValues = new Dictionary<String, String>
                            {
                                { "InnerResolvers", JsonConvert.SerializeObject(new CreateScopeResolverInformation[]
                                    {
                                    new CreateScopeResolverInformation {
                                         Typename = nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver),
                                         PropertiesAndValues = new Dictionary<String,String>
                                         {
                                             { nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver.EnterpriseNumber), enterpriseId.ToString() },
                                             { nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver.RelayAgentIndex), relayAgentIndex.ToString() },
                                         }

                                    }
                                    })
                                }
                            }
                        }
                        })
                    }
                }
            };

            DHCPv6ScopeResolverManager scopeManager = new DHCPv6ScopeResolverManager(
                new JSONBasedSerializer(), Mock.Of<ILogger<DHCPv6ScopeResolverManager>>());

            scopeManager.AddOrUpdateScopeResolver(nameof(DHCPv6AndResolver), () => new DHCPv6AndResolver());
            scopeManager.AddOrUpdateScopeResolver(nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver), () => new DHCPv6RemoteIdentifierEnterpriseNumberResolver(Mock.Of<ILogger<DHCPv6RemoteIdentifierEnterpriseNumberResolver>>()));

            var firstLevelResolver = scopeManager.InitializeResolver(createScopeResolverInformation);
            Assert.NotNull(firstLevelResolver);

            Assert.IsAssignableFrom<DHCPv6AndResolver>(firstLevelResolver);

            DHCPv6AndResolver firstLevelAndResolver = (DHCPv6AndResolver)firstLevelResolver;
            Assert.Single(firstLevelAndResolver.GetScopeResolvers());

            var secondLevelResolver = firstLevelAndResolver.GetScopeResolvers().First();
            Assert.IsAssignableFrom<DHCPv6AndResolver>(secondLevelResolver);

            DHCPv6AndResolver secondLevelAndResolver = (DHCPv6AndResolver)secondLevelResolver;
            Assert.Single(secondLevelAndResolver.GetScopeResolvers());

            var innerResolver = secondLevelAndResolver.GetScopeResolvers().First();
            Assert.IsAssignableFrom<DHCPv6RemoteIdentifierEnterpriseNumberResolver>(innerResolver);

            DHCPv6RemoteIdentifierEnterpriseNumberResolver innerCasedResolver = (DHCPv6RemoteIdentifierEnterpriseNumberResolver)innerResolver;

            Assert.Equal(enterpriseId, innerCasedResolver.EnterpriseNumber);
            Assert.Equal(relayAgentIndex, innerCasedResolver.RelayAgentIndex);


        }
    }
}
