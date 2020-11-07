using DaAPI.Core.Common;
using DaAPI.Core.Services;
using DaAPI.Infrastructure.StorageEngine;
using DaAPI.Infrastructure.StorageEngine.DHCPv6;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.Services
{
    public class DatabaseDHCPv6ServerPropertiesResolver : IDHCPv6ServerPropertiesResolver
    {
        private readonly IDHCPv6ReadStore store;

        public DatabaseDHCPv6ServerPropertiesResolver(IDHCPv6ReadStore store)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
        }

        private DHCPv6ServerProperties GetServerConfigModel() => store.GetServerProperties().GetAwaiter().GetResult();

        public DUID GetServerDuid() => GetServerConfigModel().ServerDuid;
        public TimeSpan GetLeaseLifeTime() => GetServerConfigModel().LeaseLifeTime;
        public TimeSpan GetHandledLifeTime() => GetServerConfigModel().HandledLifeTime;
        public UInt32 GetMaximumHandledCounter() => GetServerConfigModel().MaximumHandldedCounter;
    }
}
