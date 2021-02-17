using DaAPI.Core.Packets.DHCPv4;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace DaAPI.App.Helper
{
    public class DHCPv4PacketResponseCodeHelper
    {
        private readonly IStringLocalizer<DHCPv4PacketResponseCodeHelper> _localizer;

        public DHCPv4PacketResponseCodeHelper(IStringLocalizer<DHCPv4PacketResponseCodeHelper> localizer)
        {
            this._localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        }

        private static Dictionary<DHCPv4MessagesTypes, Dictionary<Int32, (String Name, String Color)>> _responseCodesMapper;

        private void FillMapper()
        {
            if (_responseCodesMapper != null) { return; }

            _responseCodesMapper = new Dictionary<DHCPv4MessagesTypes, Dictionary<Int32, (String Name, String Color)>>
{
                { DHCPv4MessagesTypes.Discover, new Dictionary<Int32, (String Name, String Color)> {
                        { 0, (_localizer["Discover_0"],"#28a745") },
                        { 1, (_localizer["Discover_1"],"#ffc107") },
                        { 2, (_localizer["Discover_2"],"#dc3545") },
                    }
                },
                { DHCPv4MessagesTypes.Request, new Dictionary<Int32, (String Name, String Color)> {
                        { 0, (_localizer["Request_0"],"#28a745") },
                        { 1, (_localizer["Request_1"],"#ffc107") },
                        { 3, (_localizer["Request_3"],"#d81b60") },
                        { 4, (_localizer["Request_4"],"#f012be") },
                        { 5, (_localizer["Request_5"],"#6f42c1") },
                        { 6, (_localizer["Request_5"],"#dc3545") },
                        { 7, (_localizer["Request_5"],"#001f3f") },
                    }
                },
                { DHCPv4MessagesTypes.Release, new Dictionary<Int32, (String Name, String Color)> {
                        { 0, (_localizer["Release_0"],"#28a745") },
                        { 1, (_localizer["Release_1"],"#d81b60") },
                        { 2, (_localizer["Release_2"],"#f012be") },
                    }
                },
                { DHCPv4MessagesTypes.Inform, new Dictionary<Int32, (String Name, String Color)> {
                        { 0, (_localizer["Inform_0"],"#28a745") },
                        { 1, (_localizer["Inform_1"],"#d81b60") },
                        { 2, (_localizer["Inform_2"],"#f012be") },
                    }
                },
                { DHCPv4MessagesTypes.Decline, new Dictionary<Int32, (String Name, String Color)> {
                        { 0, (_localizer["Decline_0"],"#28a745") },
                        { 1, (_localizer["Decline_1"],"#ffc107") },
                        { 2, (_localizer["Decline_2"],"#d81b60") },
                        { 3, (_localizer["Decline_3"],"#f012be") },
                        { 4, (_localizer["Decline_4"],"#6f42c1") },
                        { 5, (_localizer["Decline_5"],"#dc3545") },
                        { 6, (_localizer["Decline_6"],"#001f3f") },
                    }
                },
            };
        }

        public Dictionary<DHCPv4MessagesTypes, Dictionary<Int32, (String Name, String Color)>> GetResponseCodesMapper()
        {
            FillMapper();
            return _responseCodesMapper;
        }

        public String GetErrorName(DHCPv4MessagesTypes request, Int32 errorCode)
        {
            FillMapper();

            return _responseCodesMapper.ContainsKey(request) == true ?
            (_responseCodesMapper[request].ContainsKey(errorCode) == true ? _responseCodesMapper[request][errorCode].Name : errorCode.ToString()) : errorCode.ToString();
        }
    }
}
