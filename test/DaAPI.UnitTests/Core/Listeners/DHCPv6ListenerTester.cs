using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Listeners;
using DaAPI.TestHelper;
using DaAPI.UnitTests.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using Xunit;
using static DaAPI.Core.Listeners.DHCPv6ListenerEvents;

namespace DaAPI.UnitTests.Core.Listeners
{
    public class DHCPv6ListenerTester  : AggregateRootWithEventsTesterBase
    {
        [Fact]
        public void Create()
        {
            Random random = new Random();
            String interfaceId = random.NextGuid().ToString();
            IPv6Address address = random.GetIPv6Address();
            String interfaceName = random.GetAlphanumericString();

            DHCPv6Listener listener = DHCPv6Listener.Create(interfaceId, DHCPListenerName.FromString(interfaceName), address);
            Assert.NotNull(listener);

            Assert.NotEqual(Guid.Empty, listener.Id);
            Assert.Equal(address, listener.Address);
            Assert.Equal(interfaceName, listener.Name);
            Assert.Equal(interfaceId, listener.PhysicalInterfaceId);

            var @event = GetFirstEvent<DHCPv6ListenerCreatedEvent>(listener);
            Assert.Equal(address.ToString(), @event.IPv6Address);
            Assert.Equal(listener.Id, @event.Id);
            Assert.Equal(interfaceId, @event.InterfaceId);
            Assert.Equal(interfaceName, @event.Name);
        }

        [Fact]
        public void Delete()
        {
            Random random = new Random();
            Guid id = random.NextGuid();

            DHCPv6Listener listener = new DHCPv6Listener();
            listener.Load(new DomainEvent[] {
                new DHCPv6ListenerCreatedEvent
                {
                    Id = id,
                    IPv6Address = random.GetIPv6Address().ToString(),
                }
            });

            Assert.False(listener.IsDeleted);

            listener.Delete();
            
            Assert.True(listener.IsDeleted);

            var @event = GetFirstEvent<DHCPv6ListenerDeletedEvent>(listener);
            Assert.Equal(id, @event.Id);
        }

        [Fact]
        public void FromNIC()
        {
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                var properites = nic.GetIPProperties();
                if (properites == null) { continue; }

                foreach (var ipAddress in properites.UnicastAddresses)
                {
                    if (ipAddress.Address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
                    {
                        continue;
                    }

                    if (ipAddress.Address.IsIPv6LinkLocal == true)
                    {
                        continue;
                    }

                    DHCPv6Listener listener = DHCPv6Listener.FromNIC(nic, ipAddress.Address);
                    Assert.NotNull(listener);

                    Assert.Equal(nic.GetPhysicalAddress().GetAddressBytes(), listener.PhysicalAddress);
                    Assert.Equal(nic.Description, listener.Interfacename);
                    Assert.Equal(nic.Id, listener.PhysicalInterfaceId);
                    Assert.Equal(ipAddress.Address.GetAddressBytes(), listener.Address.GetBytes());
                    Assert.True(String.IsNullOrEmpty(listener.Name));
                }
            }
        }
    }
}
