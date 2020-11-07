using DaAPI.Core.Notifications.Triggers;
using DaAPI.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DaAPI.Core.Notifications.Actors
{
    public class NxOsStaticRouteUpdaterNotificationActor : NotificationActor
    {
        #region Fields

        private readonly INxOsDeviceConfigurationService _nxosDeviceSerive;
        private readonly ILogger<NxOsStaticRouteUpdaterNotificationActor> _logger;

        #endregion

        #region Properties

        public String Url { get; private set; }
        public String Username { get; private set; }
        public String Password { get; private set; }

        #endregion

        public NxOsStaticRouteUpdaterNotificationActor(
            INxOsDeviceConfigurationService nxosDeviceSerive,
            ILogger<NxOsStaticRouteUpdaterNotificationActor> logger
            )
        {
            this._nxosDeviceSerive = nxosDeviceSerive ?? throw new ArgumentNullException(nameof(nxosDeviceSerive));
            this._logger = logger;
        }

        protected internal override async Task<Boolean> Handle(NotifcationTrigger trigger)
        {
            if (trigger is PrefixEdgeRouterBindingUpdatedTrigger == false)
            {
                _logger.LogError("notification actor {name} has an invalid trigger. expected trigger type is {expectedType} actual is {type}",
                    nameof(NxOsStaticRouteUpdaterNotificationActor), typeof(PrefixEdgeRouterBindingUpdatedTrigger), trigger.GetType());

                return false;
            }

            _logger.LogDebug("connection to nx os device {address}", Url);

            Boolean isReachabled = await _nxosDeviceSerive.Connect(Url, Username, Password);
            if (isReachabled == false)
            {
                _logger.LogDebug("unable to connect to device {address}", Url);
                return false;
            }

            var castedTrigger = (PrefixEdgeRouterBindingUpdatedTrigger)trigger;
            _logger.LogDebug("connection to device {address} established", Url);
            if (castedTrigger.OldBinding != null)
            {
                Boolean removeResult = await _nxosDeviceSerive.RemoveIPv6StaticRoute(
                    castedTrigger.OldBinding.Prefix, castedTrigger.OldBinding.Mask.Identifier, castedTrigger.OldBinding.Host);
                if (removeResult == false)
                {
                    _logger.LogError("unable to remve old route form device {address}. Cancel actor", Url);
                    return false;
                }

                _logger.LogDebug("static route {prefix}/{mask} via {host} has been removed from {device}",
                     castedTrigger.OldBinding.Prefix, castedTrigger.OldBinding.Mask.Identifier, castedTrigger.OldBinding.Host, Url);
            }

            if (castedTrigger.NewBinding != null)
            {
                Boolean addResult = await _nxosDeviceSerive.AddIPv6StaticRoute(
                 castedTrigger.NewBinding.Prefix, castedTrigger.NewBinding.Mask.Identifier, castedTrigger.NewBinding.Host);
                if (addResult == false)
                {
                    _logger.LogError("unable to add a static route to device {address}. Cancel actor", Url);
                    return false;
                }

                _logger.LogDebug("static route {prefix}/{mask} via {host} has been added from {device}",
                     castedTrigger.NewBinding.Prefix, castedTrigger.NewBinding.Mask.Identifier, castedTrigger.NewBinding.Host, Url);
            }

            _logger.LogDebug("actor {name} successfully finished", nameof(NxOsStaticRouteUpdaterNotificationActor));
            return true;

        }

        public override NotificationActorCreateModel ToCreateModel() => new NotificationActorCreateModel
        {
            Typename = nameof(NxOsStaticRouteUpdaterNotificationActor),
            PropertiesAndValues = new Dictionary<String, String>
            {
                {nameof(Url), GetQuotedString(Url)  },
                {nameof(Username), GetQuotedString(Username)  },
                {nameof(Password), GetQuotedString(Password)  },
            }
        };

        public override Boolean ApplyValues(IDictionary<String, String> propertiesAndValues)
        {
            try
            {
                var url = GetValueWithoutQuota(propertiesAndValues[nameof(Url)]);
                var uri = new Uri(url);

                if (uri.Scheme != "http" && uri.Scheme != "https")
                {
                    return false;
                }

                Url = url;
      
                Username = GetValueWithoutQuota(propertiesAndValues[nameof(Username)]);
                Password = GetValueWithoutQuota(propertiesAndValues[nameof(Password)]);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
