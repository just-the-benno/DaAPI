using DaAPI.Infrastructure.ServiceBus;
using DaAPI.Infrastructure.ServiceBus.Messages;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace DaAPI.UnitTests.Infrastructure.ServiceBus
{
    public class MediaRBasedServiceBusTester
    {
        private readonly ITestOutputHelper testConsole;

        public class Pinged : IMessage
        {
            public List<String> Pongs { get; private set; } = new List<String>();

            public void SetPong(String pong) => Pongs.Add(pong);
        }

        public class CascadePing : Pinged
        {
            public Int32 CascadeDeep { get; private set; }

            public CascadePing(Int32 deep)
            {
                CascadeDeep = deep;
            }

            public void SetAsCasded() => SetPong($"{CascadeDeep--}");
        }

        public class AsyncPingedHandler : INotificationHandler<Pinged>
        {
            private readonly ITestOutputHelper _testConsole;
            private readonly string _name;
            private readonly bool _shouldThrowError;

            public AsyncPingedHandler(ITestOutputHelper testConsole, string name, Boolean throwError)
            {
                this._testConsole = testConsole;
                this._name = name;
                this._shouldThrowError = throwError;
            }

            public async Task Handle(Pinged notification, CancellationToken cancellationToken)
            {
                _testConsole.WriteLine($"[AsyncPingedHandler {_name}] {DateTime.Now:HH:mm:ss.fff} : Pinged");
                await Task.Delay(100).ConfigureAwait(false);
                if (_shouldThrowError == true)
                {
                    _testConsole.WriteLine($"[AsyncPingedHandler {_name}] {DateTime.Now:HH:mm:ss.fff} : After pinged, with exception");
                    throw new InvalidOperationException("something");
                }

                notification.SetPong(_name);
                _testConsole.WriteLine($"[AsyncPingedHandler {_name}] {DateTime.Now:HH:mm:ss.fff} : After pinged");
            }
        }


        public class SyncPingedHandler : INotificationHandler<Pinged>
        {
            private readonly ITestOutputHelper _testConsole;
            private readonly string _name;
            private readonly bool _shouldThrowError;

            public SyncPingedHandler(ITestOutputHelper testConsole, string name, Boolean shouldThrowError)
            {
                this._testConsole = testConsole;
                this._name = name;
                this._shouldThrowError = shouldThrowError;
            }

            public Task Handle(Pinged notification, CancellationToken cancellationToken)
            {
                _testConsole.WriteLine($"[SyncPingedHandler {_name}] {DateTime.Now:HH:mm:ss.fff} : Pinged");
                Thread.Sleep(100);
                if (_shouldThrowError == true)
                {
                    _testConsole.WriteLine($"[SyncPingedHandler {_name}] {DateTime.Now:HH:mm:ss.fff} : After pinged with error");
                    throw new InvalidOperationException("something");
                }

                notification.SetPong(_name);
                _testConsole.WriteLine($"[SyncPingedHandler {_name}] {DateTime.Now:HH:mm:ss.fff} : After pinged");
                return Task.CompletedTask;
            }
        }

        public class CascadingAsyncPingedHandler : INotificationHandler<CascadePing>
        {
            private readonly ITestOutputHelper _testConsole;
            private readonly IServiceBus _serviceBus;

            public CascadingAsyncPingedHandler(ITestOutputHelper testConsole, IServiceBus serviceBus)
            {
                this._testConsole = testConsole;
                this._serviceBus = serviceBus;
            }

            public async Task Handle(CascadePing notification, CancellationToken cancellationToken)
            {
                await Task.Delay(100);
                _testConsole.WriteLine($"[CascadingAsyncPingedHandler ({notification.CascadeDeep}) ] {DateTime.Now:HH:mm:ss.fff} : Pinged");
                notification.SetAsCasded();

                if (notification.CascadeDeep > 0)
                {
                    await _serviceBus.Publish(notification);
                }
            }
        }

        public MediaRBasedServiceBusTester(ITestOutputHelper testConsole)
        {
            this.testConsole = testConsole;
        }

        [Fact]
        public async Task SendEvents()
        {
            var services = new ServiceCollection();
            services.AddScoped<ServiceFactory>(p => p.GetService);

            services.AddSingleton<IServiceBus, MediaRBasedServiceBus>();

            services.AddTransient<INotificationHandler<Pinged>>(sp => new SyncPingedHandler(testConsole, "Sync no error", false));
            services.AddTransient<INotificationHandler<Pinged>>(sp => new AsyncPingedHandler(testConsole, "async no error", false));
            services.AddTransient<INotificationHandler<Pinged>>(sp => new AsyncPingedHandler(testConsole, "async with error", true));
            services.AddTransient<INotificationHandler<Pinged>>(sp => new SyncPingedHandler(testConsole, "sync with error", true));

            var provider = services.BuildServiceProvider();

            var publisher = provider.GetRequiredService<IServiceBus>();

            var pinged = new Pinged();

            await publisher.Publish(pinged);
            Assert.Empty(pinged.Pongs);

            await Task.Delay(300);

            Assert.Equal(2, pinged.Pongs.Count);
        }


        [Fact]
        public async Task Send_CascadingEvents()
        {
            var services = new ServiceCollection();
            services.AddScoped<ServiceFactory>(p => p.GetService);

            services.AddSingleton<IServiceBus, MediaRBasedServiceBus>();

            services.AddTransient<INotificationHandler<CascadePing>>(sp => new CascadingAsyncPingedHandler(testConsole, sp.GetRequiredService<IServiceBus>()));

            var provider = services.BuildServiceProvider();

            var publisher = provider.GetRequiredService<IServiceBus>();

            var pinged = new CascadePing(4);

            await publisher.Publish(pinged);
            Assert.Empty(pinged.Pongs);

            await Task.Delay(600);

            Assert.Equal(4, pinged.Pongs.Count);
        }
    }
}
