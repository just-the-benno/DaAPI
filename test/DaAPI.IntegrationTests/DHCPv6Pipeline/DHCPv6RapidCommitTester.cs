using Castle.Core.Logging;
using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.FilterEngines.DHCPv6;
using DaAPI.Core.Notifications;
using DaAPI.Core.Notifications.Actors;
using DaAPI.Core.Notifications.Conditions;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv6;
using DaAPI.Core.Scopes.DHCPv6.Resolvers;
using DaAPI.Core.Scopes.DHCPv6.ScopeProperties;
using DaAPI.Core.Services;
using DaAPI.Infrastructure.FilterEngines.DHCPv6;
using DaAPI.Infrastructure.InterfaceEngines;
using DaAPI.Infrastructure.LeaseEngines.DHCPv6;
using DaAPI.Infrastructure.NotificationEngine;
using DaAPI.Infrastructure.ServiceBus;
using DaAPI.Infrastructure.ServiceBus.MessageHandler;
using DaAPI.Infrastructure.ServiceBus.Messages;
using DaAPI.Infrastructure.Services;
using DaAPI.Infrastructure.StorageEngine;
using DaAPI.Infrastructure.StorageEngine.DHCPv6;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DaAPI.IntegrationTests.DHCPv6Pipeline
{
    public class DHCPv6RapidCommitTester
    {
        //[Fact]
        public async Task SendAndReceive()
        {
            Random random = new Random();

            String dbFileName = $"mydb-{random.Next()}.db";
            //File.Create(dbFileName);
            try
            {
                UInt32 enterpriseNumber = (UInt32)random.Next();

                DHCPv6RootScope initialRootScope = new DHCPv6RootScope(Guid.NewGuid(), new DHCPv6ScopeResolverManager(new JSONBasedSerializer()
                    , Mock.Of<ILogger<DHCPv6ScopeResolverManager>>()), null);

                initialRootScope.AddScope(new DHCPv6ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                            IPv6Address.FromString("fe80::0"),
                            IPv6Address.FromString("fe80::ff"),
                            new List<IPv6Address> { IPv6Address.FromString("fe80::1") },
                            preferredLifeTime: TimeSpan.FromDays(0.5),
                            validLifeTime: TimeSpan.FromDays(1),
                            rapitCommitEnabled: true,
                            informsAreAllowd: true,
                            supportDirectUnicast: true,
                            reuseAddressIfPossible: false,
                            acceptDecline: true,
                            addressAllocationStrategy: DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next),
                    ScopeProperties = DHCPv6ScopeProperties.Empty,
                    ResolverInformation = new CreateScopeResolverInformation
                    {
                        Typename = nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver),
                        PropertiesAndValues = new Dictionary<String, String>
                        {
                            { nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver.EnterpriseNumber), enterpriseNumber.ToString()  },
                            { nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver.RelayAgentIndex), 0.ToString()  },
                        }
                    },
                    Name = "Testscope",
                    Id = Guid.NewGuid(),
                });

                DbContextOptionsBuilder<StorageContext> dbContextOptionsBuilder = new DbContextOptionsBuilder<StorageContext>();
                dbContextOptionsBuilder.UseSqlite($"Filename={dbFileName}", options =>
                {
                    options.MigrationsAssembly(typeof(StorageContext).Assembly.FullName);
                });

                DbContextOptions<StorageContext> contextOptions = dbContextOptionsBuilder.Options;
                StorageContext initicalContext = new StorageContext(contextOptions);
                await initicalContext.Database.MigrateAsync();
                await initicalContext.Save(initialRootScope);

                await initicalContext.SaveInitialServerConfiguration(new DHCPv6ServerProperties { ServerDuid = new UUIDDUID(new Guid()) });

                initicalContext.Dispose();
                initialRootScope = null;

                IPv6Address expectedAdress = IPv6Address.FromString("fe80::0");

                var services = new ServiceCollection();
                services.AddScoped<ServiceFactory>(p => p.GetService);

                services.AddSingleton<DHCPv6RootScope>(sp =>
                {
                    var storageEngine = sp.GetRequiredService<IDHCPv6StorageEngine>();
                    var scope = storageEngine.GetRootScope(sp.GetRequiredService<IScopeResolverManager<DHCPv6Packet, IPv6Address>>()).GetAwaiter().GetResult();
                    return scope;
                });

                services.AddTransient<IDHCPv6ServerPropertiesResolver, DatabaseDHCPv6ServerPropertiesResolver>();
                services.AddSingleton<ISerializer, JSONBasedSerializer>();
                services.AddSingleton<IScopeResolverManager<DHCPv6Packet, IPv6Address>, DHCPv6ScopeResolverManager>();
                services.AddSingleton<IServiceBus, MediaRBasedServiceBus>();
                services.AddSingleton<IDHCPv6PacketFilterEngine, SimpleDHCPv6PacketFilterEngine>();
                services.AddSingleton<IDHCPv6InterfaceEngine, DHCPv6InterfaceEngine>();
                services.AddSingleton<IDHCPv6LeaseEngine, DHCPv6LeaseEngine>();
                services.AddSingleton<IDHCPv6StorageEngine, DHCPv6StorageEngine>();
                services.AddSingleton<IDHCPv6ReadStore, StorageContext>();
                services.AddSingleton<IDHCPv6EventStore, StorageContext>();
                services.AddSingleton<DbContextOptions<StorageContext>>(contextOptions);

                services.AddTransient<INotificationHandler<DHCPv6PacketArrivedMessage>>(sp => new DHCPv6PacketArrivedMessageHandler(
                   sp.GetRequiredService<IServiceBus>(), sp.GetRequiredService<IDHCPv6PacketFilterEngine>(), sp.GetService<ILogger<DHCPv6PacketArrivedMessageHandler>>()));

                services.AddTransient<INotificationHandler<ValidDHCPv6PacketArrivedMessage>>(sp => new ValidDHCPv6PacketArrivedMessageHandler(
                  sp.GetRequiredService<IServiceBus>(), sp.GetRequiredService<IDHCPv6LeaseEngine>(), sp.GetService<ILogger<ValidDHCPv6PacketArrivedMessageHandler>>()));

                services.AddTransient<INotificationHandler<DHCPv6PacketReadyToSendMessage>>(sp => new DHCPv6PacketReadyToSendMessageHandler(
                  sp.GetRequiredService<IDHCPv6InterfaceEngine>(), sp.GetService<ILogger<DHCPv6PacketReadyToSendMessageHandler>>()));

                services.AddTransient<DHCPv6RateLimitBasedFilter>();
                services.AddTransient<DHCPv6PacketConsistencyFilter>();
                services.AddTransient<DHCPv6PacketServerIdentifierFilter>((sp) => new DHCPv6PacketServerIdentifierFilter(
                   new UUIDDUID(new Guid()), sp.GetService<ILogger<DHCPv6PacketServerIdentifierFilter>>()));

                services.AddLogging();

                var provider = services.BuildServiceProvider();

                IDHCPv6PacketFilterEngine packetFilterEngine = provider.GetService<IDHCPv6PacketFilterEngine>();
                packetFilterEngine.AddFilter(provider.GetService<DHCPv6RateLimitBasedFilter>());
                packetFilterEngine.AddFilter(provider.GetService<DHCPv6PacketConsistencyFilter>());
                packetFilterEngine.AddFilter(provider.GetService<DHCPv6PacketServerIdentifierFilter>());


                IDHCPv6InterfaceEngine interfaceEngine = provider.GetService<IDHCPv6InterfaceEngine>();
                var possibleListener = interfaceEngine.GetPossibleListeners();

                var listener = possibleListener.First();

                interfaceEngine.OpenListener(listener);

                IPAddress address = new IPAddress(listener.Address.GetBytes());
                IPEndPoint ownEndPoint = new IPEndPoint(address, 546);
                IPEndPoint serverEndPoint = new IPEndPoint(address, 547);

                UdpClient client = new UdpClient(ownEndPoint);

                DHCPv6Packet packet = DHCPv6Packet.AsInner(14, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit),
                new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier,new UUIDDUID(Guid.NewGuid())),
                new DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption(15,TimeSpan.Zero,TimeSpan.Zero,new List<DHCPv6PacketSuboption>()),
            });

                DHCPv6RelayPacket outerPacket = DHCPv6RelayPacket.AsOuterRelay(new IPv6HeaderInformation(listener.Address, listener.Address),
                    true, 0, IPv6Address.FromString("fe80::2"), IPv6Address.FromString("fe80::2"), new DHCPv6PacketOption[]
                    {
                        new DHCPv6PacketRemoteIdentifierOption(enterpriseNumber,new byte[]{ 1,2,3,4,5,6,7,8,9 }),
                    }, packet);

                byte[] packetStream = outerPacket.GetAsStream();
                await client.SendAsync(packetStream, packetStream.Length, serverEndPoint);

                await Task.Delay(2000);

                var content = await client.ReceiveAsync();
                Byte[] receivedBytes = content.Buffer;
                DHCPv6Packet response = DHCPv6Packet.FromByteArray(receivedBytes, new IPv6HeaderInformation(listener.Address, listener.Address));


                var iaOption = response.GetInnerPacket().GetNonTemporaryIdentiyAssocation(15);
                Assert.NotNull(iaOption);
                Assert.Single(iaOption.Suboptions);
                Assert.IsAssignableFrom<DHCPv6PacketIdentityAssociationAddressSuboption>(iaOption.Suboptions.First());
                Assert.Equal(expectedAdress, ((DHCPv6PacketIdentityAssociationAddressSuboption)iaOption.Suboptions.First()).Address);
            }
            finally
            {
                File.Delete(dbFileName);
            }
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        //[Fact]
        //public async Task Blub()
        //{
        //    Random random = new Random();

        //    String sourcDbFileName = $"TestAssets/daapi-null-address.db";
        //    String copiedDbFileName = $"TestAssets/daapi-4.db";
        //    File.Copy(sourcDbFileName, copiedDbFileName, true);
        //    //File.Create(dbFileName);
        //    try
        //    {
        //        DbContextOptionsBuilder<StorageContext> dbContextOptionsBuilder = new DbContextOptionsBuilder<StorageContext>();
        //        dbContextOptionsBuilder.UseSqlite($"Filename={copiedDbFileName}", options =>
        //        {
        //            options.MigrationsAssembly(typeof(StorageContext).Assembly.FullName);
        //        });

        //        DbContextOptions<StorageContext> contextOptions = dbContextOptionsBuilder.Options;

        //        var services = new ServiceCollection();
        //        services.AddScoped<ServiceFactory>(p => p.GetService);

        //        services.AddSingleton<DHCPv6RootScope>(sp =>
        //        {
        //            var storageEngine = sp.GetRequiredService<IDHCPv6StorageEngine>();
        //            var scope = storageEngine.GetRootScope(sp.GetRequiredService<IScopeResolverManager<DHCPv6Packet, IPv6Address>>()).GetAwaiter().GetResult();
        //            return scope;
        //        });

        //        services.AddTransient<IDHCPv6ServerPropertiesResolver, DatabaseDHCPv6ServerPropertiesResolver>();
        //        services.AddSingleton<ISerializer, JSONBasedSerializer>();
        //        services.AddSingleton<IScopeResolverManager<DHCPv6Packet, IPv6Address>, DHCPv6ScopeResolverManager>();
        //        services.AddSingleton<IServiceBus, MediaRBasedServiceBus>();
        //        services.AddSingleton<IDHCPv6PacketFilterEngine, SimpleDHCPv6PacketFilterEngine>();
        //        services.AddSingleton<IDHCPv6InterfaceEngine, DHCPv6InterfaceEngine>();
        //        services.AddSingleton<IDHCPv6LeaseEngine, DHCPv6LeaseEngine>();
        //        services.AddSingleton<IDHCPv6StorageEngine, DHCPv6StorageEngine>();
        //        services.AddSingleton<IDHCPv6ReadStore, StorageContext>();
        //        services.AddSingleton<IDHCPv6EventStore, StorageContext>();
        //        services.AddSingleton<DbContextOptions<StorageContext>>(contextOptions);

        //        services.AddTransient<INotificationHandler<DHCPv6PacketArrivedMessage>>(sp => new DHCPv6PacketArrivedMessageHandler(
        //           sp.GetRequiredService<IServiceBus>(), sp.GetRequiredService<IDHCPv6PacketFilterEngine>(), sp.GetService<ILogger<DHCPv6PacketArrivedMessageHandler>>()));

        //        services.AddTransient<INotificationHandler<ValidDHCPv6PacketArrivedMessage>>(sp => new ValidDHCPv6PacketArrivedMessageHandler(
        //          sp.GetRequiredService<IServiceBus>(), sp.GetRequiredService<IDHCPv6LeaseEngine>(), sp.GetService<ILogger<ValidDHCPv6PacketArrivedMessageHandler>>()));

        //        services.AddTransient<INotificationHandler<DHCPv6PacketReadyToSendMessage>>(sp => new DHCPv6PacketReadyToSendMessageHandler(
        //          sp.GetRequiredService<IDHCPv6InterfaceEngine>(), sp.GetService<ILogger<DHCPv6PacketReadyToSendMessageHandler>>()));

        //        services.AddTransient<INotificationHandler<NewTriggerHappendMessage>>(sp => new NewTriggerHappendMessageHandler(
        //  sp.GetRequiredService<INotificationEngine>(), sp.GetService<ILogger<NewTriggerHappendMessageHandler>>()));

        //        services.AddSingleton<INotificationEngine, NotificationEngine>();
        //        services.AddSingleton<INotificationActorFactory, ServiceProviderBasedNotificationActorFactory>();
        //        services.AddSingleton<INotificationConditionFactory, ServiceProviderBasedNotificationConditionFactory>();
        //        services.AddTransient<INxOsDeviceConfigurationService, HttpBasedNxOsDeviceConfigurationService>();

        //        services.AddTransient<NxOsStaticRouteUpdaterNotificationActor>();
        //        services.AddTransient<DHCPv6ScopeIdNotificationCondition>();

        //        services.AddTransient<DHCPv6RateLimitBasedFilter>();
        //        services.AddTransient<DHCPv6PacketConsistencyFilter>();
        //        services.AddTransient<DHCPv6PacketServerIdentifierFilter>((sp) => new DHCPv6PacketServerIdentifierFilter(
        //           new UUIDDUID(Guid.Parse("3c379ac3-b779-417e-a8a5-5b3f40db9f02")), sp.GetService<ILogger<DHCPv6PacketServerIdentifierFilter>>()));

        //        services.AddLogging();

        //        var provider = services.BuildServiceProvider();

        //        IDHCPv6PacketFilterEngine packetFilterEngine = provider.GetService<IDHCPv6PacketFilterEngine>();
        //        packetFilterEngine.AddFilter(provider.GetService<DHCPv6RateLimitBasedFilter>());
        //        packetFilterEngine.AddFilter(provider.GetService<DHCPv6PacketConsistencyFilter>());
        //        packetFilterEngine.AddFilter(provider.GetService<DHCPv6PacketServerIdentifierFilter>());

        //        IDHCPv6InterfaceEngine interfaceEngine = provider.GetService<IDHCPv6InterfaceEngine>();
        //        var possibleListener = interfaceEngine.GetPossibleListeners();

        //        var notificationEngine = provider.GetService<INotificationEngine>();
        //        notificationEngine.Initialize().GetAwaiter().GetResult();

        //        var listener = possibleListener.First();

        //        interfaceEngine.OpenListener(listener);

        //        IPAddress address = new IPAddress(listener.Address.GetBytes());
        //        IPEndPoint ownEndPoint = new IPEndPoint(address, 546);
        //        IPEndPoint serverEndPoint = new IPEndPoint(address, 547);

        //        //UdpClient client = new UdpClient(ownEndPoint);

        //        //String input = "0c002a0ccac5020100960000000000000001fe80000000000000025d73fffe66ce880009003601047aaa00080002ef0c0001000a00030001005d7366ce87000e0000000600060019001700180019000c0024000100000000000000000025000e00000009000308002c4f52ceac850012000409010060";
        //        //String input = "0c002a0ccac5020100960000000000000001fe80000000000000025d73fffe66ce880009005b084406500008000200000001000a00030001005d7366ce87000200120004c39a373c79b77e41a8a55b3f40db9f0200190029002400010000000000000000001a001900000000000000003c2a0ccac63296ab6000000000000000000025000e00000009000308002c4f52ceac850012000409010060";
        //        //String input = "0c002a0ccac5020100960000000000000001fe80000000000000025d73fffe66ce880009005a089c64630008000200000001000a00030001005d7366ce87000200120004c39a373c79b77e41a8a55b3f40db9f0200030028002400010000000000000000000500182a0ccac50201009600000000000000f90000a8c0000151800025000e00000009000308002c4f52ceac850012000409010060";
        //        //String input = "0c002a0ccac5020100960000000000000001fe80000000000000025d73fffe66ce880009005b08a7904a0008000200000001000a00030001005d7366ce87000200120004c39a373c79b77e41a8a55b3f40db9f0200190029002400010000000000000000001a001900000000000000003c2a0ccac63296b5b000000000000000000025000e00000009000308002c4f52ceac850012000409010060";
        //        //String input = "0c002a0ccac5020100960000000000000001fe80000000000000025d73fffe66ce880009005b08d571d00008000200000001000a00030001005d7366ce87000200120004c39a373c79b77e41a8a55b3f40db9f0200190029002400010000000000000000001a001900000000000000003c2a0ccac6329660a000000000000000000025000e00000009000308002c4f52ceac850012000409010060";
        //        //String input = "0c012a0ccac5010200970000000000000001fe80000000000000aab456fffe8ffcbe000900ab0c00fe800000000000000000000000000000fe80000000000000f2b014fffea467bf0009007b019d40d90008000200000001000a00030001f0b014a467bf000e00000003000c14a467bf00000000000000000019002914a467bf0000000000000000001a001900000000000000000000000000000000000000000000000000001400000006001200170038001f001900030011005200530056001000040000036800120006564c414e39370025000e0000000900030800780cf016dc9d0012000409010061";
        //        //String input = "0c012a0ccac5010200970000000000000001fe80000000000000aab456fffe8ffcbe000900ab0c00fe800000000000000000000000000000fe80000000000000f2b014fffea467bf0009007b01cf858b00080002dcb50001000a00030001f0b014a467bf000e00000003000c14a467bf00000000000000000019002914a467bf0000000000000000001a001900000000000000000000000000000000000000000000000000001400000006001200170038001f001900030011005200530056001000040000036800120006564c414e39370025000e0000000900030800780cf016dc9d0012000409010061";
        //        //String input = "0c002a0ccac5010200110000000000000001fe80000000000000f2b014fffea4728f0009007b011d1aba0008000200000001000a00030001f0b014a4728f000e00000003000c14a4728f00000000000000000019002914a4728f0000000000000000001a001900000000000000004000000000000000000000000000000000001400000006001200170038001f00190003001100520053005600100004000003680025000e0000000900030800780cf016dc9d001200040901000b";
        //        //String input = "0c012a0ccac5010200970000000000000001fe80000000000000f2b014fffea4728f000900d10c0000000000000000000000000000000000fe80000000000000f2b014fffea4728f0012000c746f6e79736167746d6f696e0009008a01af1d440008000200000001000a00030001f0b014a4728f000e00000003000c14a4728f00000000000000000019002914a4728f0000000000000000001a001900000000000000000000000000000000000000000000000000001400000006001200170038001f00190003001100520053005600100004000003680011000b00000de9009000030101000025000d00000de93132333435363738390025000e0000000900030800780cf016dc9d0012000409010061";
        //        //String input = "0c012a0ccac5010038900000000000000001fe8000000000000072617bfffeeb83000009006f0c0000000000000000000000000000000000fe8000000000000072617bfffeeb830000120003312f310025000a0000000054833a9d34070009003401e88e920008000202d40001000a0003000170617beb83000006000800170018003b00110003000c0012000100000000000000000025000e0000000900030800780cf016dc9d0012000409010f32";
        //        String input = "0c012a0ccac6200000960000000000000002fe8000000000000002eeabfffe284695000900950c00fe800000000000000000000000000000fe800000000000007683c2fffe1e3a4900090065018c79f70001000e000100011c375ddc7483c21e3a470003000c000000000000000000000000000e000000080002ffff000600040017001800190029000000000000000000000000001a0019ffffffffffffffff3c0000000000000000000000000000000000120006564c414e39360025000e0000000900030800780cf016dc9d0012000409010060";
        //        Byte[] content = StringToByteArray(input);

        //        DHCPv6Packet packet = DHCPv6Packet.FromByteArray(content, new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2")));

        //        IServiceBus serviceBus = provider.GetService<IServiceBus>();
        //        await serviceBus.Publish(new DHCPv6PacketArrivedMessage(packet));

        //        while (true)
        //        {
        //            await Task.Delay(2000);
        //        }

        //        //var content = await client.ReceiveAsync();
        //        //Byte[] receivedBytes = content.Buffer;
        //        //DHCPv6Packet response = DHCPv6Packet.FromByteArray(receivedBytes, new IPv6HeaderInformation(listener.Address, listener.Address));


        //        //var iaOption = response.GetInnerPacket().GetNonTemporaryIdentiyAssocation(15);
        //        //Assert.NotNull(iaOption);
        //        //Assert.Single(iaOption.Suboptions);
        //        //Assert.IsAssignableFrom<DHCPv6PacketIdentityAssociationAddressSuboption>(iaOption.Suboptions.First());
        //        //Assert.Equal(expectedAdress, ((DHCPv6PacketIdentityAssociationAddressSuboption)iaOption.Suboptions.First()).Address);
        //    }
        //    finally
        //    {
        //        File.Delete(copiedDbFileName);
        //    }
        //}
    }
}
