using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Helper;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv6;
using DaAPI.Core.Scopes.DHCPv6.ScopeProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace DaAPI.Core.Packets.DHCPv6
{
    public class DHCPv6Packet : DHCPPacket<DHCPv6Packet, IPv6Address>
    {
        #region  Fields

        private readonly List<DHCPv6PacketOption> _options;

        #endregion

        #region Properties

        public UInt32 TransactionId { get; private set; }
        public DHCPv6PacketTypes PacketType { get; private set; }

        public IReadOnlyList<DHCPv6PacketOption> Options => _options.AsReadOnly();

        public override Boolean IsValid => PacketType != DHCPv6PacketTypes.Invalid;

        #endregion

        #region Constructor

        public DHCPv6Packet(IPv6HeaderInformation header, UInt32 transactionId, DHCPv6PacketTypes packetType, IEnumerable<DHCPv6PacketOption> options)
        {
            Header = header;
            TransactionId = transactionId;
            PacketType = packetType;
            _options = new List<DHCPv6PacketOption>(options);
        }

        public static DHCPv6Packet AsOuter(IPv6HeaderInformation header, UInt32 transactionId, DHCPv6PacketTypes packetType, IEnumerable<DHCPv6PacketOption> options) =>
           new DHCPv6Packet(header, transactionId, packetType, options);

        public static DHCPv6Packet AsInner(UInt32 transactionId, DHCPv6PacketTypes packetType, IEnumerable<DHCPv6PacketOption> options) =>
            new DHCPv6Packet(null, transactionId, packetType, options);

        protected DHCPv6Packet(DHCPv6PacketTypes type, IEnumerable<DHCPv6PacketOption> options)
        {
            PacketType = type;
            _options = new List<DHCPv6PacketOption>(options ?? Array.Empty<DHCPv6PacketOption>());
        }


        private DHCPv6Packet(DHCPv6PacketTypes type, UInt32 transactionId, IEnumerable<DHCPv6PacketOption> options)
        {
            PacketType = type;
            TransactionId = transactionId;
            _options = new List<DHCPv6PacketOption>(options);
        }

        #endregion

        #region Methods

        #region Queries

        public virtual DHCPv6Packet GetInnerPacket() => this;

        public DUID GetIdentifier(DHCPv6PacketOptionTypes optionType)
        {
            var option = _options.FirstOrDefault(x => x.Code == (UInt16)optionType);
            if (option == null || option is DHCPv6PacketIdentifierOption == false)
            {
                return DUID.Empty;
            }

            var casedOption = (DHCPv6PacketIdentifierOption)option;
            return casedOption.DUID;
        }

        private UInt32? GetIdentityAssocationId(DHCPv6PacketOptionTypes optionType)
        {
            var option = _options.Where(x => x.Code == (UInt16)optionType)
               .Cast<DHCPv6PacketIdentityAsociationOption>().FirstOrDefault();

            if (option == null) { return null; }

            return option.Id;
        }

        public UInt32? GetNonTemporaryIdentityAssocationId() => GetIdentityAssocationId(DHCPv6PacketOptionTypes.IdentityAssociation_NonTemporary);
        public UInt32? GetPrefixDelegationIdentityAssocationId() => GetIdentityAssocationId(DHCPv6PacketOptionTypes.IdentityAssociation_PrefixDelegation);

        public bool IsUnicast() =>
            PacketType != DHCPv6PacketTypes.RELAY_FORW && PacketType != DHCPv6PacketTypes.RELAY_REPL &&
            Header.Source.IsUnicast() == true;

        public T GetOption<T>(DHCPv6PacketOptionTypes optionType) where T : DHCPv6PacketOption
            => GetOption<T>((UInt16)optionType);

        public T GetOption<T>(UInt16 optionType) where T : DHCPv6PacketOption
        {
            if (HasOption(optionType) == false) { return null; }
            DHCPv6PacketOption option = _options.First(x => x.Code == optionType);
            if (option is T == false) { return null; }

            return (T)option;
        }

        public Boolean HasOption(UInt16 type) => _options.FirstOrDefault(x => x.Code == type) != null;

        public Boolean HasOption(DHCPv6PacketOptionTypes type) => HasOption((UInt16)type);

        public Boolean HasRapitCommitOption() => HasOption(DHCPv6PacketOptionTypes.RapitCommit);

        public DUID GetServerIdentifer() => GetIdentifier(DHCPv6PacketOptionTypes.ServerIdentifer);
        public DUID GetClientIdentifer() => GetIdentifier(DHCPv6PacketOptionTypes.ClientIdentifier);

        public Boolean ShouldHaveDuid() =>
                PacketType == DHCPv6PacketTypes.REQUEST ||
                PacketType == DHCPv6PacketTypes.RENEW ||
                PacketType == DHCPv6PacketTypes.DECLINE ||
                PacketType == DHCPv6PacketTypes.RELEASE;

        public Boolean IsClientRequest() =>
                PacketType == DHCPv6PacketTypes.Solicit ||
                PacketType == DHCPv6PacketTypes.REQUEST ||
                PacketType == DHCPv6PacketTypes.RENEW ||
                PacketType == DHCPv6PacketTypes.REBIND ||
                PacketType == DHCPv6PacketTypes.DECLINE ||
                PacketType == DHCPv6PacketTypes.RELEASE ||
                PacketType == DHCPv6PacketTypes.CONFIRM ||
                PacketType == DHCPv6PacketTypes.INFORMATION_REQUEST ||
                PacketType == DHCPv6PacketTypes.RELAY_FORW;

        private Boolean ShouldHaveClientButNotServerIdentifier(DUID clientIdentifier, DUID serverIdentiifer)
    => clientIdentifier != DUID.Empty && serverIdentiifer == DUID.Empty;

        private Boolean ShouldHaveClientAndServerIdentifier(DUID clientIdentifier, DUID serverIdentiifer)
            => clientIdentifier != DUID.Empty && serverIdentiifer != DUID.Empty;

        public Boolean IsConsistent()
        {
            DUID clientIdentifier = GetClientIdentifer();
            DUID serverIdentiifer = GetServerIdentifer();

            if (
                PacketType == DHCPv6PacketTypes.Solicit ||
                PacketType == DHCPv6PacketTypes.CONFIRM ||
                PacketType == DHCPv6PacketTypes.REBIND
                )
            {
                return ShouldHaveClientButNotServerIdentifier(clientIdentifier, serverIdentiifer);
            }
            else if (
                PacketType == DHCPv6PacketTypes.REQUEST ||
                PacketType == DHCPv6PacketTypes.RENEW ||
                PacketType == DHCPv6PacketTypes.DECLINE ||
                PacketType == DHCPv6PacketTypes.RELEASE
                )
            {
                return ShouldHaveClientAndServerIdentifier(clientIdentifier, serverIdentiifer);
            }
            else if (PacketType == DHCPv6PacketTypes.INFORMATION_REQUEST)
            {
                return clientIdentifier != DUID.Empty;
            }
            else if (
                PacketType == DHCPv6PacketTypes.Invalid ||
                PacketType == DHCPv6PacketTypes.Unkown)
            {
                return false;
            }

            return true;
        }

        private T GetIdentiyAssocation<T>(DHCPv6PacketOptionTypes optionType, UInt32 identityAssocationId) where T : DHCPv6PacketIdentityAsociationOption
        {
            var result =
                _options.Where(x => x.Code == (UInt16)optionType)
                .Cast<T>()
                .Where(x => x.Id == identityAssocationId).FirstOrDefault();

            return result;
        }

        public DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption GetNonTemporaryIdentiyAssocation(UInt32 identityAssocationId) =>
            GetIdentiyAssocation<DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption>(DHCPv6PacketOptionTypes.IdentityAssociation_NonTemporary, identityAssocationId);

        public DHCPv6PacketIdentityAssociationPrefixDelegationOption GetPrefixDelegationIdentiyAssocation(UInt32 identityAssocationId) =>
              GetIdentiyAssocation<DHCPv6PacketIdentityAssociationPrefixDelegationOption>(DHCPv6PacketOptionTypes.IdentityAssociation_PrefixDelegation, identityAssocationId);

        public static DHCPv6Packet ConstructPacket(DHCPv6Packet receivedPacket, DHCPv6Packet innerResponse)
        {
            if (receivedPacket.PacketType == DHCPv6PacketTypes.RELAY_FORW)
            {
                return DHCPv6RelayPacket.ConstructRelayPacket((DHCPv6RelayPacket)receivedPacket, innerResponse);
            }
            else
            {
                return innerResponse;
            }
        }

        public Boolean CouldHaveDuid() => PacketType == DHCPv6PacketTypes.INFORMATION_REQUEST;

        #endregion

        #region as responses

        private static void AddOptions(ICollection<DHCPv6PacketOption> packetOptions, DHCPv6Packet requestPacket, DUID serverDuid, DHCPv6ScopeAddressProperties addressProperties, DHCPv6ScopeProperties properties, Boolean onlyMinimialOptions)
        {
            DHCPv6Packet innerReceivedPacket = requestPacket.GetInnerPacket();

            packetOptions.Add(new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ServerIdentifer, serverDuid));
            packetOptions.Add(new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier, innerReceivedPacket.GetClientIdentifer()));
            if (onlyMinimialOptions == true)
            {
                return;
            }

            if (addressProperties?.SupportDirectUnicast == true)
            {
                packetOptions.Add(new DHCPv6PacketIPAddressOption(DHCPv6PacketOptionTypes.ServerUnicast, requestPacket.Header.ListenerAddress));
            }

            foreach (var item in properties?.Properties)
            {
                switch (item)
                {
                    case DHCPv6AddressListScopeProperty property:
                        packetOptions.Add(new DHCPv6PacketIPAddressListOption(property.OptionIdentifier, property.Addresses));
                        break;
                    case DHCPv6NumericValueScopeProperty property:
                        switch (property.NumericType)
                        {
                            case NumericScopePropertiesValueTypes.Byte:
                                packetOptions.Add(new DHCPv6PacketByteOption(property.OptionIdentifier, (Byte)property.Value));
                                break;
                            case NumericScopePropertiesValueTypes.UInt16:
                                packetOptions.Add(new DHCPv6PacketUInt16Option(property.OptionIdentifier, (UInt16)property.Value));
                                break;
                            case NumericScopePropertiesValueTypes.UInt32:
                                packetOptions.Add(new DHCPv6PacketUInt32Option(property.OptionIdentifier, (UInt32)property.Value));
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private static DHCPv6Packet ConstructPacketWithHeader(DHCPv6Packet requestPacket, DHCPv6Packet innerResponse)
        {
            DHCPv6Packet response = ConstructPacket(requestPacket, innerResponse);
            response.Header = IPv6HeaderInformation.AsResponse(requestPacket.Header);
            return response;
        }

        public static DHCPv6Packet AsAdvertise(
            DHCPv6Packet requestPacket,
            IPv6Address leaseAddress,
            DHCPv6PrefixDelegation prefixDelegation,
            DHCPv6ScopeAddressProperties addressProperties,
            DHCPv6ScopeProperties properties,
            DUID serverDuid
            )
        {
            DHCPv6Packet innerReceivedPacket = requestPacket.GetInnerPacket();

            TimeSpan T1 = addressProperties.T1.Value * addressProperties.PreferredLeaseTime.Value;
            TimeSpan T2 = addressProperties.T2.Value * addressProperties.PreferredLeaseTime.Value;

            TimeSpan preferredLifetime = addressProperties.PreferredLeaseTime.Value; ;
            TimeSpan validLifetime = addressProperties.ValidLeaseTime.Value;

            List<DHCPv6PacketOption> packetOptions = new List<DHCPv6PacketOption>
            {
                DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption.AsSuccess(innerReceivedPacket.GetNonTemporaryIdentityAssocationId().Value,T1,T2,leaseAddress,preferredLifetime,validLifetime)
            };

            if (innerReceivedPacket.HasOption(DHCPv6PacketOptionTypes.IdentityAssociation_PrefixDelegation) == true)
            {
                if (prefixDelegation == DHCPv6PrefixDelegation.None)
                {
                    packetOptions.Add(DHCPv6PacketIdentityAssociationPrefixDelegationOption.NotAvailable(
                    requestPacket.GetOption<DHCPv6PacketIdentityAssociationPrefixDelegationOption>(DHCPv6PacketOptionTypes.IdentityAssociation_PrefixDelegation)));
                }
                else
                {
                    packetOptions.Add(DHCPv6PacketIdentityAssociationPrefixDelegationOption.AsSuccess(prefixDelegation.IdentityAssociation, T1, T2, prefixDelegation.Mask.Identifier, prefixDelegation.NetworkAddress, preferredLifetime, validLifetime));
                }
            }

            AddOptions(packetOptions, requestPacket, serverDuid, addressProperties, properties, false);

            DHCPv6Packet innerResponse = new DHCPv6Packet(DHCPv6PacketTypes.ADVERTISE,
                innerReceivedPacket.TransactionId,
                packetOptions);

            DHCPv6Packet response = ConstructPacketWithHeader(requestPacket, innerResponse);
            return response;
        }

        private static void AddRapitCommitOption(DHCPv6Packet response)
        {
            var innerPacket = response.GetInnerPacket();
            innerPacket.PacketType = DHCPv6PacketTypes.REPLY;
            innerPacket._options.Add(new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit));
        }

        public static DHCPv6Packet AsPrefixAdvertise(DHCPv6Packet requestPacket, DHCPv6ScopeAddressProperties addressProperties, DHCPv6ScopeProperties properties, DHCPv6Lease lease, DUID serverDuid)
        {
            DHCPv6Packet innerReceivedPacket = requestPacket.GetInnerPacket();

            AdjustTimeValues(addressProperties, lease, out TimeSpan preferredLifetime, out TimeSpan validLifetime);

            TimeSpan T1 = preferredLifetime * addressProperties.T1.Value;
            TimeSpan T2 = preferredLifetime * addressProperties.T2.Value;

            List<DHCPv6PacketOption> packetOptions = new List<DHCPv6PacketOption>
            {
                DHCPv6PacketIdentityAssociationPrefixDelegationOption.AsSuccess(lease.PrefixDelegation.IdentityAssociation,T1,T2,lease.PrefixDelegation.Mask.Identifier,lease.PrefixDelegation.NetworkAddress,preferredLifetime,validLifetime)
            };

            AddOptions(packetOptions, requestPacket, serverDuid, addressProperties, properties, false);

            DHCPv6Packet innerResponse = new DHCPv6Packet(DHCPv6PacketTypes.ADVERTISE,
                innerReceivedPacket.TransactionId,
                packetOptions);

            DHCPv6Packet response = ConstructPacketWithHeader(requestPacket, innerResponse);

            return response;
        }

        public static DHCPv6Packet AsPrefixReplyWithRapitCommit(DHCPv6Packet requestPacket, DHCPv6ScopeAddressProperties addressProperties, DHCPv6ScopeProperties properties, DHCPv6Lease lease, DUID serverDuid)
        {
            DHCPv6Packet response = AsPrefixAdvertise(requestPacket, addressProperties, properties, lease, serverDuid);
            AddRapitCommitOption(response);

            return response;
        }

        public static DHCPv6Packet AsReply(DHCPv6Packet requestPacket, DHCPv6ScopeAddressProperties addressProperties, DHCPv6ScopeProperties properties, DHCPv6Lease lease, Boolean adjustTimers, DUID serverDuid, Boolean isRapitCommit)
        {
            DHCPv6Packet innerReceivedPacket = requestPacket.GetInnerPacket();

            TimeSpan preferredLifetime = addressProperties.PreferredLeaseTime.Value;
            TimeSpan validLifetime = addressProperties.ValidLeaseTime.Value;
            if (adjustTimers == true)
            {
                AdjustTimeValues(addressProperties, lease, out preferredLifetime, out validLifetime);
            }

            TimeSpan T1 = addressProperties.T1.Value * preferredLifetime;
            TimeSpan T2 = addressProperties.T2.Value * preferredLifetime;

            List<DHCPv6PacketOption> packetOptions = new List<DHCPv6PacketOption>();

            if (innerReceivedPacket.HasOption(DHCPv6PacketOptionTypes.IdentityAssociation_NonTemporary) == true)
            {
                packetOptions.Add(DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption.AsSuccess(
                    lease.IdentityAssocicationId, T1, T2, lease.Address, preferredLifetime, validLifetime));
            }

            if (innerReceivedPacket.HasOption(DHCPv6PacketOptionTypes.IdentityAssociation_PrefixDelegation) == true)
            {
                if (lease.PrefixDelegation == DHCPv6PrefixDelegation.None)
                {
                    packetOptions.Add(DHCPv6PacketIdentityAssociationPrefixDelegationOption.NotAvailable(
                    innerReceivedPacket.GetOption<DHCPv6PacketIdentityAssociationPrefixDelegationOption>(DHCPv6PacketOptionTypes.IdentityAssociation_PrefixDelegation)));
                }
                else
                {
                    packetOptions.Add(DHCPv6PacketIdentityAssociationPrefixDelegationOption.AsSuccess(lease.PrefixDelegation.IdentityAssociation, T1, T2, lease.PrefixDelegation.Mask.Identifier, lease.PrefixDelegation.NetworkAddress, preferredLifetime, validLifetime));
                }
            }

            if (isRapitCommit == true)
            {
                packetOptions.Add(new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit));
            }

            AddOptions(packetOptions, requestPacket, serverDuid, addressProperties, properties, false);

            DHCPv6Packet innerResponse = new DHCPv6Packet(DHCPv6PacketTypes.REPLY,
                innerReceivedPacket.TransactionId,
                packetOptions);

            DHCPv6Packet response = ConstructPacketWithHeader(requestPacket, innerResponse);

            return response;
        }

        public static DHCPv6Packet AsError(DHCPv6Packet requestPacket, DHCPv6StatusCodes errorCode, DUID serverDuid)
        {
            DHCPv6Packet innerReceivedPacket = requestPacket.GetInnerPacket();

            List<DHCPv6PacketOption> packetOptions = new List<DHCPv6PacketOption>();
            {
                foreach (var item in innerReceivedPacket.Options)
                {
                    switch (item)
                    {
                        case DHCPv6PacketIdentityAssociationPrefixDelegationOption option:
                            if (errorCode == DHCPv6StatusCodes.NoAddrsAvail)
                            {
                                packetOptions.Add(DHCPv6PacketIdentityAssociationPrefixDelegationOption.NotAvailable(option));
                            }
                            else
                            {
                                packetOptions.Add(DHCPv6PacketIdentityAssociationPrefixDelegationOption.Error(option, errorCode));
                            }
                            break;
                        case DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption option:
                            packetOptions.Add(DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption.Error(option, errorCode));
                            break;
                        default:
                            break;
                    }
                }
            }

            AddOptions(packetOptions, requestPacket, serverDuid, null, null, true);

            DHCPv6Packet innerResponse = new DHCPv6Packet(DHCPv6PacketTypes.REPLY,
                innerReceivedPacket.TransactionId,
                packetOptions);

            DHCPv6Packet response = ConstructPacketWithHeader(requestPacket, innerResponse);
            return response;
        }

        public static DHCPv6Packet AsReleaseResponse(DHCPv6Packet requestPacket, UInt32 identityAssocicationId, UInt32 prefixIdentityAssociation, DUID serverDuid)
        {
            DHCPv6Packet innerReceivedPacket = requestPacket.GetInnerPacket();

            List<DHCPv6PacketOption> packetOptions = new List<DHCPv6PacketOption>();
            {
                foreach (var item in innerReceivedPacket.Options)
                {
                    switch (item)
                    {
                        case DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption option:
                            packetOptions.Add(DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption.Error(option, option.Id == identityAssocicationId ? DHCPv6StatusCodes.Success : DHCPv6StatusCodes.NoBinding));
                            break;
                        case DHCPv6PacketIdentityAssociationPrefixDelegationOption option:
                            packetOptions.Add(DHCPv6PacketIdentityAssociationPrefixDelegationOption.Error(option, option.Id == prefixIdentityAssociation ? DHCPv6StatusCodes.Success : DHCPv6StatusCodes.NoBinding));
                            break;
                        default:
                            break;
                    }
                }
            }

            AddOptions(packetOptions, requestPacket, serverDuid, null, null, true);

            DHCPv6Packet innerResponse = new DHCPv6Packet(DHCPv6PacketTypes.REPLY,
                innerReceivedPacket.TransactionId,
                packetOptions);

            DHCPv6Packet response = ConstructPacketWithHeader(requestPacket, innerResponse);
            return response;
        }

        private static void AdjustTimeValues(DHCPv6ScopeAddressProperties addressProperties, DHCPv6Lease lease, out TimeSpan preferredLifetime, out TimeSpan validLifetime)
        {
            TimeSpan delta = DateTime.UtcNow - lease.Start;

            preferredLifetime = addressProperties.PreferredLeaseTime.Value - delta;
            validLifetime = addressProperties.ValidLeaseTime.Value - delta;

            if (validLifetime.TotalMinutes < 0)
            {
                preferredLifetime = TimeSpan.FromMinutes(1);
            }
        }

        #endregion

        #region To and from byte arrays

        public static DHCPv6Packet FromByteArray(Byte[] rawData, IPv6HeaderInformation ipv6Header)
        {
            try
            {
                var result = FromByteArray(rawData);
                result.Header = ipv6Header;
                result._byteRepresentation = rawData;
                return result;
            }
            catch (Exception)
            {
                return new DHCPV6UnparsablePacket(ipv6Header, rawData);
            }
        }

        protected static DHCPv6Packet FromByteArray(Byte[] rawData)
        {
            DHCPv6PacketTypes type = (DHCPv6PacketTypes)rawData[0];

            DHCPv6Packet result;
            if (type == DHCPv6PacketTypes.RELAY_REPL || type == DHCPv6PacketTypes.RELAY_FORW)
            {
                result = DHCPv6RelayPacket.FromByteArray(rawData, 0);
            }
            else
            {
                UInt32 transactionId = (UInt32)((rawData[1] << 16) | (rawData[2] << 8) | rawData[3]);

                result = new DHCPv6Packet(
                    type, transactionId,
                    DHCPv6PacketOptionFactory.GetOptions(rawData, 4));
            }

            return result;
        }

        private Byte[] _byteRepresentation = null;

        private void PrepareStream()
        {
            if (_byteRepresentation == null)
            {
                Byte[] stream = new byte[1800];
                Int32 length = GetAsStream(stream);
                Byte[] result = ByteHelper.CopyData(stream, 0, length);

                _byteRepresentation = result;
            }
        }

        public UInt16 GetSize()
        {
            PrepareStream();
            return (UInt16)_byteRepresentation.Length;
        }

        public override Byte[] GetAsStream()
        {
            PrepareStream();
            return _byteRepresentation;
        }

        public Int32 GetAsStream(Byte[] packetAsByteStream) => GetAsStream(packetAsByteStream, 0);

        public virtual Int32 GetAsStream(Byte[] packetAsByteStream, Int32 offset)
        {
            packetAsByteStream[offset + 0] = (Byte)PacketType;
            Byte[] transactionId = ByteHelper.GetBytes(TransactionId);

            packetAsByteStream[offset + 1] = transactionId[1];
            packetAsByteStream[offset + 2] = transactionId[2];
            packetAsByteStream[offset + 3] = transactionId[3];

            offset += 4;

            foreach (DHCPv6PacketOption option in _options)
            {
                offset += option.AppendToStream(packetAsByteStream, offset);
            }

            return offset;
        }

        #endregion

        #endregion
    }
}
