using DaAPI.Core.Scopes.DHCPv6;
using DaAPI.Infrastructure.ServiceBus;
using DaAPI.Infrastructure.ServiceBus.Messages;
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
    public class LeaseTimerHostedService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<LeaseTimerHostedService> _logger;
        private Timer _timer;

        public LeaseTimerHostedService(IServiceProvider services,
            ILogger<LeaseTimerHostedService> logger)
        {
            this._services = services;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("LeaseTimerHostedService Service running.");

            _timer = new Timer((x) => { try { DoWork(x); } catch { } }, null, TimeSpan.Zero,
                TimeSpan.FromMinutes(1));

            return Task.CompletedTask;
        }

        private async void DoWork(object _)
        {
            _logger.LogInformation("Lease Timer intervall started. Checking for expired leases");
            try
            {
                using (var scope = _services.CreateScope())
                {
                    var eventStore = scope.ServiceProvider.GetRequiredService<IDHCPv6EventStore>();
                    var rootScope = scope.ServiceProvider.GetRequiredService<DHCPv6RootScope>();
                    var serviceBus = scope.ServiceProvider.GetRequiredService<IServiceBus>();

                    Int32 changeAmount = rootScope.CleanUpLeases();
                    _logger.LogInformation("{ChangeAmount} expired leases found", changeAmount);
                    if (await eventStore.Exists() == true)
                    {
                        await eventStore.Save(rootScope);
                    }

                    if (changeAmount > 0)
                    {
                        var triggers = rootScope.GetTriggers();
                        if (triggers.Any() == true)
                        {
                            await serviceBus.Publish(new NewTriggerHappendMessage(triggers));
                            rootScope.ClearTriggers();
                        }
                    }

                    _logger.LogInformation("Lease Timer intervall finished");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "clean up intervall finished with error");
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Lease Timer is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
