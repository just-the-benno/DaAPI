using DaAPI.Core.Common;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv4;
using DaAPI.Core.Services;
using DaAPI.Infrastructure;
using DaAPI.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DaAPI.IntegrationTests.Serializations
{
    public class CreateScopeSerializerTester
    {
        [Fact]
        public void CheckSerialization()
        {
            Value.InitalizeMembers(System.Reflection.Assembly.GetExecutingAssembly());

            DHCPv4ScopeCreateInstruction instruction = new DHCPv4ScopeCreateInstruction
            {
                Name = ScopeName.FromString("my firt scope"),
                Description = ScopeDescription.FromString("some description"),
                AddressProperties = new DHCPv4ScopeAddressProperties
                (
                    IPv4Address.FromString("123.244.124.233"),
                    IPv4Address.FromString("123.244.124.240"),
                    new List<IPv4Address>
                    {
                        IPv4Address.FromString("123.244.124.238"),
                        IPv4Address.FromString("123.244.124.239"),
                        IPv4Address.FromString("123.244.124.240"),
                    },
                    TimeSpan.FromMinutes(30),
                    TimeSpan.FromMinutes(45),
                    TimeSpan.FromMinutes(60),
                    false,
                    DHCPv4ScopeAddressProperties.DHCPv4AddressAllocationStrategies.Next,
                    true,
                    false,
                    true
                ),
                Properties = new DHCPv4ScopeProperties
                (
                    new DHCPv4TextScopeProperty(78, "something not important"),
                    DHCPv4NumericValueScopeProperty.FromRawValue(17, "1457", DHCPv4NumericValueTypes.UInt16),
                    new DHCPv4AddressScopeProperty(15, "157.180.125.2")
                ),
            };
            
            ISerializer serializer = new JSONBasedSerializer();
            String temp = serializer.Seralize(instruction);

            DHCPv4ScopeCreateInstruction deseriaizedInstruction = serializer.Deserialze<DHCPv4ScopeCreateInstruction>(temp);

            Assert.NotNull(deseriaizedInstruction);

            Assert.Equal(instruction.AddressProperties, deseriaizedInstruction.AddressProperties);
            Assert.Equal(instruction.Description, deseriaizedInstruction.Description);
            Assert.Equal(instruction.Id, deseriaizedInstruction.Id);
            Assert.Equal(instruction.Name, deseriaizedInstruction.Name);
            Assert.Equal(instruction.ParentId, deseriaizedInstruction.ParentId);
            Assert.Equal(instruction.Properties, deseriaizedInstruction.Properties);

            //Assert.Equal(instruction.ResolverInformations, deseriaizedInstruction.ResolverInformations);




        }

    }
}
