using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.App.Helper
{
    public class DHCPv6PacketOptionCodeToNameConverter
    {
        private readonly IStringLocalizer<DHCPv6PacketOptionCodeToNameConverter> _localizer;

        public DHCPv6PacketOptionCodeToNameConverter(IStringLocalizer<DHCPv6PacketOptionCodeToNameConverter> localizer)
        {
            this._localizer = localizer;
        }

        public String GetName(UInt16 code) => _localizer[code.ToString()];
    }
}
