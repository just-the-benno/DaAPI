using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.App.Helper
{
    public class DHCPv4PacketOptionCodeToNameConverter
    {
        private readonly IStringLocalizer<DHCPv4PacketOptionCodeToNameConverter> _localizer;

        public DHCPv4PacketOptionCodeToNameConverter(IStringLocalizer<DHCPv4PacketOptionCodeToNameConverter> localizer)
        {
            this._localizer = localizer;
        }

        public String GetName(UInt16 code) => _localizer[code.ToString()];
    }
}
