using DaAPI.Core.Packets.DHCPv6;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace DaAPI.App.Helper
{
    public class DHCPv6PacketResponseCodeHelper
    {
        private readonly IStringLocalizer<DHCPv6PacketResponseCodeHelper> _localizer;

        public DHCPv6PacketResponseCodeHelper(IStringLocalizer<DHCPv6PacketResponseCodeHelper> localizer)
        {
            this._localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        }

        private static Dictionary<DHCPv6PacketTypes, Dictionary<Int32, (String Name, String Color)>> _responseCodesMapper;

        private void FillMapper()
        {
            if (_responseCodesMapper != null) { return; }

            _responseCodesMapper = new Dictionary<DHCPv6PacketTypes, Dictionary<Int32, (String Name, String Color)>>
{
                { DHCPv6PacketTypes.Solicit, new Dictionary<Int32, (String Name, String Color)> {
                        { 0, (_localizer["Solicit_0"],"#28a745") },
                        { 1, (_localizer["Solicit_1"],"#ffc107") },
                        { 2, (_localizer["Solicit_2"],"#dc3545") },
                        { 3, (_localizer["Solicit_3"],"#d81b60") },
                        { 4, (_localizer["Solicit_4"],"#f012be") },
                        { 5, (_localizer["Solicit_5"],"#6f42c1") },
                    }
                },
                { DHCPv6PacketTypes.REQUEST, new Dictionary<Int32, (String Name, String Color)> {
                        { 0, (_localizer["Request_0"],"#28a745") },
                        { 1, (_localizer["Request_1"],"#ffc107") },
                        { 3, (_localizer["Request_3"],"#d81b60") },
                        { 8, (_localizer["Request_8"],"#f012be") },
                        { 9, (_localizer["Request_9"],"#6f42c1") },
                    }
                },
                { DHCPv6PacketTypes.RENEW, new Dictionary<Int32, (String Name, String Color)> {
                        { 0, (_localizer["Renew_0"],"#28a745") },
                        { 1, (_localizer["Renew_1"],"#ffc107") },
                        { 3, (_localizer["Renew_3"],"#d81b60") },
                        { 4, (_localizer["Renew_4"],"#dc3545") },
                        { 5, (_localizer["Renew_5"],"#6f42c1") },
                    }
                },
                { DHCPv6PacketTypes.REBIND, new Dictionary<Int32, (String Name, String Color)> {
                        { 0, (_localizer["Rebind_0"],"#28a745") },
                        { 1, (_localizer["Rebind_1"],"#ffc107") },
                        { 3, (_localizer["Rebind_3"],"#d81b60") },
                        { 4, (_localizer["Rebind_4"],"#dc3545") },
                        { 5, (_localizer["Rebind_5"],"#6f42c1") },
                    }
                },
                { DHCPv6PacketTypes.DECLINE, new Dictionary<Int32, (String Name, String Color)> {
                        { 0, (_localizer["Decline_0"],"#28a745") },
                        { 1, (_localizer["Decline_1"],"#ffc107") },
                        { 2, (_localizer["Decline_2"],"#6f42c1") },
                        { 3, (_localizer["Decline_3"],"#d81b60") },
                        { 4, (_localizer["Decline_4"],"#f012be") },
                        { 5, (_localizer["Decline_5"],"#dc3545") },
                        { 6, (_localizer["Decline_6"],"#001f3f") },
                    }
                },
                { DHCPv6PacketTypes.RELEASE, new Dictionary<Int32, (String Name, String Color)> {
                        { 0, (_localizer["Release_0"],"#28a745") },
                        { 1, (_localizer["Release_1"],"#d81b60") },
                        { 2, (_localizer["Release_2"],"#f012be") },
                        { 3, (_localizer["Release_3"],"#ffc107") },
                    }
                },
                { DHCPv6PacketTypes.CONFIRM, new Dictionary<Int32, (String Name, String Color)> {
                        { 0, (_localizer["Confirm_0"],"#28a745") },
                        { 1, (_localizer["Confirm_1"],"#ffc107") },
                        { 3, (_localizer["Confirm_3"],"#d81b60") },
                        { 4, (_localizer["Confirm_4"],"#f012be") },
                        { 5, (_localizer["Confirm_5"],"#fd7e14") },
                    }
                }
            };
        }

        public Dictionary<DHCPv6PacketTypes, Dictionary<Int32, (String Name, String Color)>> GetResponseCodesMapper()
        {
            FillMapper();
            return _responseCodesMapper;
        }

        public String GetErrorName(DHCPv6PacketTypes request, Int32 errorCode)
        {
            FillMapper();

            return _responseCodesMapper.ContainsKey(request) == true ?
            (_responseCodesMapper[request].ContainsKey(errorCode) == true ? _responseCodesMapper[request][errorCode].Name : errorCode.ToString()) : errorCode.ToString();
        }
    }
}
