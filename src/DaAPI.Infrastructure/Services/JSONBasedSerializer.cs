using DaAPI.Core.Services;
using DaAPI.Infrastructure.Services.Helper;
using DaAPI.Infrastructure.Services.JsonConverters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Infrastructure.Services
{
    public class JSONBasedSerializer : ISerializer
    {
        #region Fields

        private readonly JsonSerializerSettings _settings;

        #endregion

        #region Constructor

        public JSONBasedSerializer()
        {
            _settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> {
                                new IPv4AddressJSONConverter(),
                                new IPv4SubnetJSONConverter(),
                                new DHCPv4ScopePropertyJSONConverter(),
                                new IPv6AddressAsStringJsonConverter(),
                                new IPv6SubnetMaskAsStringJsonConverter(),

                               },
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                ContractResolver = new BaseSpecifiedConcreteClassConverter()
            };
        }

        #endregion

        #region Methods

        public T Deserialze<T>(String input)
        {
            T result = JsonConvert.DeserializeObject<T>(input, _settings);

            return result;
        }

        public string Seralize<T>(T data)
        {
            return JsonConvert.SerializeObject(data, _settings);
        }

        #endregion
    }
}
