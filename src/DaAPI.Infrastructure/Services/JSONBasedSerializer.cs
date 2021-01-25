using DaAPI.Core.Services;
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
                                new IPv6AddressAsStringJsonConverter(),
                                new IPv6SubnetMaskAsStringJsonConverter(),
                                new IPv4AddressAsStringJsonConverter(),
                                new IPv4SubnetMaskAsStringJsonConverter(),
                               },

                ObjectCreationHandling = ObjectCreationHandling.Replace,
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
