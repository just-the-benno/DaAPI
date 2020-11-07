using DaAPI.Core.Common;
using DaAPI.Infrastructure.Services;
using DaAPI.Infrastructure.StorageEngine.DHCPv6;
using DaAPI.TestHelper;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.UnitTests.Infrastructure.Services
{
    public class DatabaseDHCPv6ServerPropertiesResolverTester
    {
        public void GetDUID()
        {
            Random random = new Random();
            DUID expected = new UUIDDUID(random.NextGuid());

            Mock<IDHCPv6ReadStore> readStoreMock = new Mock<IDHCPv6ReadStore>(MockBehavior.Strict);
            readStoreMock.Setup(x => x.GetServerProperties()).ReturnsAsync(new DHCPv6ServerProperties { ServerDuid = expected }).Verifiable();

            DatabaseDHCPv6ServerPropertiesResolver resolver = new DatabaseDHCPv6ServerPropertiesResolver(readStoreMock.Object);

            DUID result =  resolver.GetServerDuid();

            readStoreMock.Verify();
        }

    }
}
