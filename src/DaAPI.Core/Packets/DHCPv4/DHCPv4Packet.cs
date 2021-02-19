using DaAPI.Core.Common;
using DaAPI.Core.Scopes.DHCPv4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4ScopeProperty;

namespace DaAPI.Core.Packets.DHCPv4
{
    public class DHCPv4Packet : DHCPPacket<DHCPv4Packet, IPv4Address>
    {
        public enum DHCPv4PacketOperationCodes : byte
        {
            Unknown = 0,
            BootRequest = 1,
            BootReply = 2,

        }
        [Flags]
        public enum DHCPv4PacketFlags : ushort
        {
            Unicast = 0,
            Broadcast = 1 << 15,
        }

        public enum DHCPv4PacketHardwareAddressTypes : byte
        {
            Unknown = 0,
            Ethernet = 1,
        }

        public enum DHCPv4PacketRequestType
        {
            AnswerToOffer = 2,
            /// <summary>
            /// a unicast from the client to extent its lease
            /// </summary>
            Renewing = 3,
            /// <summary>
            /// an broadcast from the client to extent its lease
            /// </summary>
            Rebinding = 4,
            /// <summary>
            /// used after a restart of a client, if it remember its last configuration
            /// </summary>
            Initializing = 5,
        }

        public static DHCPv4Packet FromByteArray(byte[] stream, IPv4HeaderInformation header) => FromByteArray(stream, header.Source, header.Destionation);

        #region Fields

        private static readonly Byte[] _magicCookie = new byte[] { 99, 130, 83, 99 };
        private static readonly Encoding _textEncoding = new ASCIIEncoding();

        private readonly List<DHCPv4PacketOption> _options;

        private DHCPv4ClientIdentifier _clientIdenfier;

        #endregion

        #region Properties

        private Boolean _isValid = false;

        public override Boolean IsValid => _isValid;

        public DHCPv4PacketOperationCodes OpCode { get; private set; }
        public DHCPv4PacketHardwareAddressTypes HardwareType { get; private set; }
        public Byte HardwareAddressLength { get; private set; }
        public Byte Hops { get; private set; }
        public UInt32 TransactionId { get; private set; }
        public UInt16 SecondsElapsed { get; private set; }
        public DHCPv4PacketFlags Flags { get; private set; }
        /// <summary>
        /// ciaddr
        /// </summary>
        public IPv4Address ClientIPAdress { get; private set; }
        /// <summary>
        /// yiaddr
        /// </summary>
        public IPv4Address YourIPAdress { get; private set; }
        /// <summary>
        /// siaddr
        /// </summary>
        public IPv4Address ServerIPAdress { get; private set; }
        /// <summary>
        /// giaddress
        /// </summary>
        public IPv4Address GatewayIPAdress { get; private set; }
        /// <summary>
        /// chaddr
        /// </summary>
        public Byte[] ClientHardwareAddress { get; private set; }
        /// <summary>
        /// sname
        /// </summary>
        public String ServerHostname { get; private set; }
        public String FileName { get; private set; }

        public IReadOnlyList<DHCPv4PacketOption> Options => _options.AsReadOnly();

        public DHCPv4MessagesTypes MessageType { get; private set; }

        #endregion

        #region Constructor

        private DHCPv4Packet()
        {
            _options = new List<DHCPv4PacketOption>();
            ClientIPAdress = IPv4Address.Empty;
            YourIPAdress = IPv4Address.Empty;
            ServerIPAdress = IPv4Address.Empty;
            GatewayIPAdress = IPv4Address.Empty;
            ClientHardwareAddress = Array.Empty<Byte>();

            OpCode = DHCPv4PacketOperationCodes.BootReply;
            HardwareType = DHCPv4PacketHardwareAddressTypes.Ethernet;
            Hops = 0;
            SecondsElapsed = 0;
            Flags = DHCPv4PacketFlags.Unicast;

            ServerHostname = String.Empty;
            FileName = String.Empty;

            Header = IPv4HeaderInformation.Default;

            _isValid = false;
        }

        public DHCPv4Packet(
           IPv4HeaderInformation ipv4Header,
           Byte[] hwAddress,
           UInt32 transactionId,
           IPv4Address yourIPAddress,
           IPv4Address gwIPAddress,
           IPv4Address clientIPAddress,
           params DHCPv4PacketOption[] options
           ) : this(ipv4Header, hwAddress, transactionId, yourIPAddress, gwIPAddress, clientIPAddress, options.ToList())
        {
        }

        public DHCPv4Packet(
        IPv4HeaderInformation ipv4Header,
        Byte[] hwAddress,
        UInt32 transactionId,
        IPv4Address yourIPAddress,
        IPv4Address gwIPAddress,
        IPv4Address clientIPAddress,
        IEnumerable<DHCPv4PacketOption> options
        ) : this()
        {

            TransactionId = transactionId;
            ClientHardwareAddress = ByteHelper.CopyData(hwAddress);
            HardwareAddressLength = (Byte)ClientHardwareAddress.Length;
            YourIPAdress = IPv4Address.FromByteArray(yourIPAddress.GetBytes());
            ClientIPAdress = IPv4Address.FromByteArray(clientIPAddress.GetBytes());
            GatewayIPAdress = IPv4Address.FromByteArray(gwIPAddress.GetBytes());
            Header = ipv4Header;

            _options = new List<DHCPv4PacketOption>();

            foreach (var item in options)
            {
                AddOption(item);
            }

            SetClientIdentifier();
            _isValid = true;
        }

        private static DHCPv4Packet Invalid => new DHCPv4Packet() { _isValid = false };

        public static DHCPv4Packet FromByteArray(Byte[] rawData, IPv4Address source, IPv4Address destionation)
        {
            try
            {
                DHCPv4Packet packet = new DHCPv4Packet
                {
                    OpCode = (DHCPv4PacketOperationCodes)rawData[0],
                    HardwareType = (DHCPv4PacketHardwareAddressTypes)rawData[1],
                    HardwareAddressLength = rawData[2],
                    Hops = rawData[3],
                    TransactionId = ByteHelper.ConvertToUInt32FromByte(rawData, 4),
                    SecondsElapsed = ByteHelper.ConvertToUInt16FromByte(rawData, 8),
                    Flags = (DHCPv4PacketFlags)ByteHelper.ConvertToUInt16FromByte(rawData, 10),
                    ClientIPAdress = IPv4Address.FromByteArray(rawData, 12),
                    YourIPAdress = IPv4Address.FromByteArray(rawData, 16),
                    ServerIPAdress = IPv4Address.FromByteArray(rawData, 20),
                    GatewayIPAdress = IPv4Address.FromByteArray(rawData, 24)
                };

                packet.Header = new IPv4HeaderInformation(source, destionation);

                packet.ClientHardwareAddress = ByteHelper.CopyData(rawData, 28, packet.HardwareAddressLength);
                Boolean serverhostnameFound = false;
                for (int i = 40; i < 40 + 64; i++)
                {
                    if (rawData[i] != 0)
                    {
                        serverhostnameFound = true;
                        break;
                    }
                }
                if (serverhostnameFound == true)
                {
                    packet.ServerHostname = new String(_textEncoding.GetChars(rawData, 40, 64));
                }
                else
                {
                    packet.ServerHostname = String.Empty;
                }

                Boolean fileNameFound = false;
                for (int i = 104; i < 104 + 128; i++)
                {
                    if (rawData[i] != 0)
                    {
                        fileNameFound = true;
                        break;
                    }
                }
                if (fileNameFound == true)
                {
                    packet.FileName = new String(_textEncoding.GetChars(rawData, 104, 128));
                }
                else
                {
                    packet.FileName = String.Empty;
                }

                Byte[] actualCookie = new Byte[] { rawData[236], rawData[237], rawData[238], rawData[239] };
                if (ByteHelper.AreEqual(_magicCookie, actualCookie) == false)
                {
                    throw new ArgumentException();
                }

                Int32 index = 240;
                while (index < rawData.Length)
                {
                    Byte optionCode = rawData[index];
                    if (optionCode == 255) { break; }
                    Byte length = rawData[index + 1];

                    Byte[] optionData = ByteHelper.CopyData(rawData, index, length + 2);
                    DHCPv4PacketOption option = DHCPv4PacketOptionFactory.GetOption(optionCode, optionData);

                    packet.AddOption(option);


                    index += 2 + length;
                }

                packet._isValid = true;

                packet.SetClientIdentifier();

                packet._byteRepresentation = rawData;

                return packet;
            }
            catch (Exception)
            {
                return DHCPv4Packet.Invalid;
            }
        }

        private void SetIPHeader(DHCPv4Packet request)
        {
            IPv4Address source = IPv4Address.Empty;
            IPv4Address destionation;
            if (request.Header.Source == IPv4Address.Empty)
            {
                if ((request.Flags & DHCPv4PacketFlags.Broadcast) == DHCPv4PacketFlags.Broadcast)
                {
                    destionation = IPv4Address.Broadcast;
                }
                else
                {
                    destionation = YourIPAdress;
                }
            }
            else
            {
                destionation = IPv4Address.FromAddress(request.Header.Source);
                source = IPv4Address.FromAddress(request.Header.Destionation);
                Flags = DHCPv4PacketFlags.Unicast;
            }

            if (MessageType == DHCPv4MessagesTypes.NotAcknowledge && request.GatewayIPAdress == IPv4Address.Empty)
            {
                Flags = DHCPv4PacketFlags.Unicast;
                destionation = IPv4Address.Broadcast;
            }

            Header = new IPv4HeaderInformation(source, destionation);
        }

        private void SetServerIdentifier(IPv4Address serverAddress)
        {
            AddOption(new DHCPv4PacketAddressOption(DHCPv4OptionTypes.ServerIdentifier, serverAddress));
        }

        private void AddOptions(
            IEnumerable<DHCPv4ScopeProperty> scopeProperties)
        {
            AddOptions(null, scopeProperties);
        }

        private void AddOptions(
        DHCPv4ScopeAddressProperties addressProperties,
        IEnumerable<DHCPv4ScopeProperty> scopeProperties)
        {
            List<DHCPv4ScopeProperty> propertiesToInsert = new List<DHCPv4ScopeProperty>(scopeProperties);
            
            if(addressProperties != null)
            {
                propertiesToInsert.Add(new DHCPv4AddressScopeProperty(DHCPv4OptionTypes.SubnetMask, IPv4Address.FromByteArray(addressProperties.Mask.GetBytes())));
            }

            Dictionary<DHCPv4OptionTypes, TimeSpan?> timeRelatedOptions = new Dictionary<DHCPv4OptionTypes, TimeSpan?>();
            if (addressProperties != null)
            {
                timeRelatedOptions.Add(DHCPv4OptionTypes.IPAddressLeaseTime, addressProperties.LeaseTime);
                timeRelatedOptions.Add(DHCPv4OptionTypes.RenewalTimeValue, addressProperties.RenewalTime);
                timeRelatedOptions.Add(DHCPv4OptionTypes.RebindingTimeValue, addressProperties.PreferredLifetime);
            }

            foreach (var item in timeRelatedOptions)
            {
                if (item.Value.HasValue == false) { continue; }

                propertiesToInsert.Insert(0, new DHCPv4TimeScopeProperty(item.Key, false, item.Value.Value));
            }

            foreach (var item in propertiesToInsert)
            {
                switch (item.ValueType)
                {

                    case DHCPv4ScopePropertyType.Address:
                    case DHCPv4ScopePropertyType.Subnet:
                        AddOption(new DHCPv4PacketAddressOption(item.OptionIdentifier, ((DHCPv4AddressScopeProperty)item).Address));
                        break;

                    case DHCPv4ScopePropertyType.AddressList:
                        AddOption(new DHCPv4PacketAddressListOption(item.OptionIdentifier, ((DHCPv4AddressListScopeProperty)item).Addresses));
                        break;

                    case DHCPv4ScopePropertyType.Time:
                        AddOption(new DHCPv4PacketTimeSpanOption(item.OptionIdentifier, ((DHCPv4TimeScopeProperty)item).Value, false));
                        break;

                    case DHCPv4ScopePropertyType.TimeOffset:
                        AddOption(new DHCPv4PacketTimeSpanOption(item.OptionIdentifier, ((DHCPv4TimeScopeProperty)item).Value, true));
                        break;

                    case DHCPv4ScopePropertyType.Byte:
                        AddOption(new DHCPv4PacketByteOption(item.OptionIdentifier, (Byte)(((DHCPv4NumericValueScopeProperty)item).Value)));
                        break;

                    case DHCPv4ScopePropertyType.UInt16:
                        AddOption(new DHCPv4PacketUInt16Option(item.OptionIdentifier, (UInt16)(((DHCPv4NumericValueScopeProperty)item).Value)));
                        break;

                    case DHCPv4ScopePropertyType.UInt32:
                        AddOption(new DHCPv4PacketUInt32Option(item.OptionIdentifier, (UInt32)(((DHCPv4NumericValueScopeProperty)item).Value)));
                        break;

                    case DHCPv4ScopePropertyType.Boolean:
                        AddOption(new DHCPv4PacketBooleanOption(item.OptionIdentifier, ((DHCPv4BooleanScopeProperty)item).Value));
                        break;

                    case DHCPv4ScopePropertyType.AddressAndMask:
                        throw new NotImplementedException();

                    case DHCPv4ScopePropertyType.Text:
                        AddOption(new DHCPv4PacketTextOption(item.OptionIdentifier, ((DHCPv4TextScopeProperty)item).Value));
                        break;

                    case DHCPv4ScopePropertyType.ByteArray:
                        //AddOption(new DHCPv4PacketRawByteOption(item.OptionIdentifier, ((DHCPv4RawByteScopeProperty)item).Value));
                        throw new NotImplementedException();
                    default:
                        break;
                }
            }
        }


        internal IPv4Address GetIPAddressFromOption(DHCPv4OptionTypes optionType)
        {
            IPv4Address result = IPv4Address.Empty;

            DHCPv4PacketAddressOption option = Options
                .Where(x => x.OptionType == (Byte)optionType)
                .Cast<DHCPv4PacketAddressOption>()
                .FirstOrDefault();
            if (option != null)
            {
                result = IPv4Address.FromAddress(option.Address);
            }

            return result;
        }

        internal IPv4Address GetRequestedAddressFromRequestedOption()
        {
            return GetIPAddressFromOption(DHCPv4OptionTypes.RequestedIPAddress);
        }

        internal IPv4Address GetServerIdentifierFromOption()
        {
            return GetIPAddressFromOption(DHCPv4OptionTypes.ServerIdentifier);
        }

        private DHCPv4Packet(DHCPv4MessagesTypes responseType, DHCPv4Packet request) : this()
        {
            MessageType = responseType;
            ClientHardwareAddress = ByteHelper.CopyData(request.ClientHardwareAddress);
            HardwareAddressLength = request.HardwareAddressLength;
            HardwareType = request.HardwareType;
            TransactionId = request.TransactionId;
            OpCode = DHCPv4PacketOperationCodes.BootReply;
            Flags = request.Flags;

            if (request.GatewayIPAdress != IPv4Address.Empty)
            {
                GatewayIPAdress = IPv4Address.FromAddress(request.GatewayIPAdress);
            }

            AddOption(new DHCPv4PacketMessageTypeOption(responseType));
        }

        public static DHCPv4Packet AsDiscoverResponse(
            DHCPv4Packet request,
            IPv4Address leaseAddress,
            DHCPv4ScopeAddressProperties addressProperties,
            IEnumerable<DHCPv4ScopeProperty> scopeProperties)
        {
            DHCPv4Packet packet = new DHCPv4Packet(DHCPv4MessagesTypes.Offer, request)
            {
                YourIPAdress = leaseAddress,
                _isValid = true,
            };

            packet.SetIPHeader(request);
            packet.AddOption(new DHCPv4PacketAddressOption(DHCPv4OptionTypes.ServerIdentifier, packet.Header.Source));
            packet.AddOptions(addressProperties, scopeProperties);

            return packet;
        }

        public static DHCPv4Packet AsRequestResponse(
            DHCPv4Packet request,
            IPv4Address leaseAddress,
            DHCPv4ScopeAddressProperties addressProperties,
            IEnumerable<DHCPv4ScopeProperty> scopeProperties)
        {
            DHCPv4Packet packet = new DHCPv4Packet(DHCPv4MessagesTypes.Acknowledge, request)
            {
                YourIPAdress = leaseAddress,
                _isValid = true,
            };

            packet.SetIPHeader(request);
            packet.AddOptions(addressProperties, scopeProperties);

            return packet;
        }

        public static DHCPv4Packet AsNonAcknowledgeResponse(DHCPv4Packet request, String message)
        {
            DHCPv4Packet packet = new DHCPv4Packet(DHCPv4MessagesTypes.NotAcknowledge, request)
            {
                _isValid = true
            };

            packet.AddOption(new DHCPv4PacketTextOption(DHCPv4OptionTypes.Message, message));
            packet.SetIPHeader(request);

            return packet;
        }

        public static DHCPv4Packet AsInformResponse(DHCPv4Packet request,
            IEnumerable<DHCPv4ScopeProperty> scopeProperties)
        {
            DHCPv4Packet packet = new DHCPv4Packet(DHCPv4MessagesTypes.Acknowledge, request)
            {
                _isValid = true,
                ClientIPAdress = request.ClientIPAdress
            };

            packet.SetIPHeader(request);
            packet.AddOptions(scopeProperties);
            packet.SetServerIdentifier(request.Header.Destionation);

            return packet;
        }

        //public static DHCPv4Packet FromRequest(DHCPv4Packet request, 
        //    DHCPv4MessagesTypes responseType,
        //    IEnumerable<DHCPv4PacketOption> options)
        //{
        //    DHCPv4Packet result = new DHCPv4Packet();

        //    result._options = new List<DHCPv4PacketOption>(options);
        //    result._options.Insert(0, new DHCPv4PacketMessageTypeOption(responseType));
        //    result.MessageType = responseType;

        //    result.ClientHardwareAddress = ByteHelper.CopyData(request.ClientHardwareAddress);
        //    result.HardwareAddressLength = request.HardwareAddressLength;
        //    result.HardwareType = request.HardwareType;
        //    result.TransactionId = request.TransactionId;
        //    result.OpCode = DHCPv4PacketOperationCodes.BootReply;
        //    result.Flags = request.Flags;

        //    IPv4Address destionationAddress = null;

        //    if (request.IPv4Header.Source == IPv4Address.Empty)
        //    {
        //        if ((request.Flags & DHCPv4PacketFlags.Broadcast) == DHCPv4PacketFlags.Broadcast)
        //        {
        //            destionationAddress = IPv4Address.Broadcast;
        //        }
        //        else
        //        {
        //            destionationAddress = response.YourIPAdress;
        //        }
        //    }
        //    else
        //    {
        //        destionationAddress = IPv4Address.FromByteArray(request.IPv4Header.Source.GetBytes());
        //    }

        //    if (request.GatewayIPAdress != IPv4Address.Empty)
        //    {
        //        result.GatewayIPAdress = IPv4Address.FromByteArray(request.GatewayIPAdress.GetBytes());
        //    }

        //    IPv4Address sourceAddress = null;

        //    sourceAddress = IPv4Address.FromByteArray(request.IPv4Header.Destionation.GetBytes());


        //    //response.Options.Add(new DHCPv4ServerOption(DHCPv4OptionTypes.ServerIdentifier, response.SourceAddress));

        //    DHCPv4PacketOption clientIdentifer = request._options.FirstOrDefault(x => x.OptionType == (Byte)DHCPv4OptionTypes.ClientIdentifier);
        //    if (clientIdentifer != null)
        //    {
        //        result._options.Add(clientIdentifer);
        //    }

        //    result.IPv4Header = new IPv4HeaderInformation(sourceAddress, destionationAddress);

        //    return result;
        //}

        #endregion

        #region Methods

        private void SetClientIdentifier()
        {
            DHCPv4PacketClientIdentifierOption clientIdentifierOption = _options.OfType<DHCPv4PacketClientIdentifierOption>().FirstOrDefault();
            if (clientIdentifierOption != null)
            {
                if(clientIdentifierOption.Identifier.HasHardwareAddress() == false)
                {
                    _clientIdenfier = clientIdentifierOption.Identifier.AddHardwareAddress(ClientHardwareAddress);
                }
            }
            else
            {
                _clientIdenfier = DHCPv4ClientIdentifier.FromHwAddress(ClientHardwareAddress);
            }
        }

        private void AddOption(DHCPv4PacketOption option)
        {
            _options.Add(option);

            if (option is DHCPv4PacketMessageTypeOption option1)
            {
                MessageType = option1.Value;
            }
        }

        public DHCPv4ClientIdentifier GetClientIdentifier() => _clientIdenfier;

        private Byte[] _byteRepresentation = null;

        public override Byte[] GetAsStream()
        {
            PrepareStream();
            return _byteRepresentation;
        }

        private void PrepareStream()
        {
            if(_byteRepresentation == null)
            {
                Byte[] packetAsByteStream = new byte[1500];
                Int32 written = GetAsStream(packetAsByteStream);

                Byte[] result = ByteHelper.CopyData(packetAsByteStream, 0, written);

                _byteRepresentation = result;
            }
        }

        public Int32 GetAsStream(Byte[] stream)
        {
            Int32 writtenBytes = 0;
            stream[writtenBytes++] = (Byte)OpCode;
            stream[writtenBytes++] = (Byte)HardwareType;
            stream[writtenBytes++] = HardwareAddressLength;
            stream[writtenBytes++] = Hops;

            Byte[] transcationid = ByteHelper.GetBytes(TransactionId);
            stream[writtenBytes++] = transcationid[0];
            stream[writtenBytes++] = transcationid[1];
            stream[writtenBytes++] = transcationid[2];
            stream[writtenBytes++] = transcationid[3];

            Byte[] secondAsId = ByteHelper.GetBytes(SecondsElapsed);
            stream[writtenBytes++] = secondAsId[0];
            stream[writtenBytes++] = secondAsId[1];

            Byte[] flag = ByteHelper.GetBytes((UInt16)Flags);
            stream[writtenBytes++] = flag[0];
            stream[writtenBytes++] = flag[1];

            List<IPv4Address> addresses = new List<IPv4Address> { ClientIPAdress, YourIPAdress, ServerIPAdress, GatewayIPAdress };
            foreach (IPv4Address item in addresses)
            {
                Byte[] addressBytes = item.GetBytes();

                stream[writtenBytes++] = addressBytes[0];
                stream[writtenBytes++] = addressBytes[1];
                stream[writtenBytes++] = addressBytes[2];
                stream[writtenBytes++] = addressBytes[3];
            }

            for (int i = 0; i < HardwareAddressLength; i++)
            {
                stream[writtenBytes++] = ClientHardwareAddress[i];
            }

            for (int i = HardwareAddressLength; i < 16; i++)
            {
                stream[writtenBytes++] = 0;
            }

            Byte[] serverName = _textEncoding.GetBytes(ServerHostname);
            for (int i = 0; i < serverName.Length; i++)
            {
                stream[writtenBytes++] = serverName[i];
            }

            for (int i = serverName.Length; i < 64; i++)
            {
                stream[writtenBytes++] = 0;
            }

            Byte[] fileName = _textEncoding.GetBytes(FileName);
            for (int i = 0; i < fileName.Length; i++)
            {
                stream[writtenBytes++] = fileName[i];
            }

            for (int i = fileName.Length; i < 128; i++)
            {
                stream[writtenBytes++] = 0;
            }

            stream[writtenBytes++] = _magicCookie[0];
            stream[writtenBytes++] = _magicCookie[1];
            stream[writtenBytes++] = _magicCookie[2];
            stream[writtenBytes++] = _magicCookie[3];

            foreach (DHCPv4PacketOption option in _options)
            {
                Int32 writtenOptionBytes = option.AppendToStream(stream, writtenBytes);
                writtenBytes += writtenOptionBytes;
            }

            // add "end" option
            stream[writtenBytes++] = 255;
            stream[writtenBytes++] = 0;

            return writtenBytes;
        }

        public DHCPv4PacketRequestType GetRequestType()
        {
            if (MessageType != DHCPv4MessagesTypes.Request)
            {
                throw new InvalidOperationException("packet is not a request");
            }

            IPv4Address requestedIPAddress = GetRequestedAddressFromRequestedOption();
            IPv4Address serverIdentifier = GetServerIdentifierFromOption();
            if (serverIdentifier == IPv4Address.Empty &&
                requestedIPAddress == IPv4Address.Empty &&
                ClientIPAdress != IPv4Address.Empty)
            {
                if (Header.Source != IPv4Address.Broadcast)
                {
                    return DHCPv4PacketRequestType.Renewing;
                }
                else
                {
                    return DHCPv4PacketRequestType.Rebinding;
                }
            }

            if (requestedIPAddress != IPv4Address.Empty &&
                serverIdentifier != IPv4Address.Empty &&
                ClientIPAdress == IPv4Address.Empty
               )
            {
                return DHCPv4PacketRequestType.AnswerToOffer;
            }

            if (serverIdentifier == IPv4Address.Empty &&
               requestedIPAddress != IPv4Address.Empty &&
               ClientIPAdress == IPv4Address.Empty
               )
            {
                return DHCPv4PacketRequestType.Initializing;
            }

            throw new InvalidOperationException("unable to determinate the request type");
        }

        public DHCPv4PacketOption GetOptionByIdentifier(DHCPv4OptionTypes optionIdentifier) =>
            GetOptionByIdentifier((Byte)optionIdentifier);

        public DHCPv4PacketOption GetOptionByIdentifier(byte optionIdentifier)
        {
            DHCPv4PacketOption option = _options.FirstOrDefault(x => x.OptionType == optionIdentifier);
            if (option == null) { return DHCPv4PacketOption.NotPresented; }

            return option;
        }

        public UInt16 GetSize()
        {
            if (_byteRepresentation == null)
            {
                Byte[] stream = new byte[1800];
                Int32 length = GetAsStream(stream);
                Byte[] result = ByteHelper.CopyData(stream, 0, length);

                _byteRepresentation = result;
            }

            return (UInt16)_byteRepresentation.Length;
        }

        #endregion
    }
}
