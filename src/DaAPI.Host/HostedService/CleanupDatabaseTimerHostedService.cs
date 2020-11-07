using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv6;
using DaAPI.Core.Services;
using DaAPI.Infrastructure.StorageEngine.DHCPv6;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DaAPI.Host.HostedService
{
    public class CleanupDatabaseTimerHostedService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<LeaseTimerHostedService> _logger;
        private Timer _timer;

        private static Boolean _operationInProgress = false;

        public CleanupDatabaseTimerHostedService(IServiceProvider services,
            ILogger<LeaseTimerHostedService> logger)
        {
            this._services = services ?? throw new ArgumentNullException(nameof(services));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            Random random = new Random();

            _logger.LogInformation("CleanupDatabaseTimerHostedService Service running.");

            _timer = new Timer((x) => { try { DoWork(x); } catch { } }, null, TimeSpan.FromMilliseconds(10_000 + random.Next(200, 600)),
                TimeSpan.FromMinutes(2));

            return Task.CompletedTask;
        }

        private async void DoWork(object _)
        {
            _logger.LogInformation("Database cleanup timer intervall started. Checking for leases and data to delete");

            if (_operationInProgress == true)
            {
                _logger.LogInformation("another cleanup process hasn't finished yet. Canceling this cycle");
                return;
            }

            _operationInProgress = true;
            try
            {
                using (var scope = _services.CreateScope())
                {
                    var serverPropertiesResolver = scope.ServiceProvider.GetRequiredService<IDHCPv6ServerPropertiesResolver>();
                    var rootScope = scope.ServiceProvider.GetRequiredService<DHCPv6RootScope>();
                    var eventStore = scope.ServiceProvider.GetRequiredService<IDHCPv6EventStore>();

                    DateTime now = DateTime.UtcNow;
                    DateTime leaseThreshold = now - serverPropertiesResolver.GetLeaseLifeTime();
                    DateTime handledEventThreshold = now - serverPropertiesResolver.GetHandledLifeTime();

                    rootScope.DropUnusedLeasesOlderThan(leaseThreshold);
                    await eventStore.DeleteLeaseRelatedEventsOlderThan(leaseThreshold);

                    await eventStore.DeletePacketHandledEventsOlderThan(handledEventThreshold);
                    await eventStore.DeletePacketHandledEventMoreThan(serverPropertiesResolver.GetMaximumHandledCounter());

                    _logger.LogInformation("Database cleanup intervall finished");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "clean up intervall finished with error");
            }
            finally
            {
                _operationInProgress = false;
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Database clean timer is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
