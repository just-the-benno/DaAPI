using DaAPI.Core.Common;
using DaAPI.Core.Packets.DHCPv6;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv6
{
    public static class DHCPv6PacketHandledEvents
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
        public abstract class DHCPv6PacketHandledEvent : DomainEvent
        {
            #region Properties

            public DHCPv6Packet Request { get; set; }
            public DHCPv6Packet Response { get; set; }
            public Boolean WasSuccessfullHandled { get; set; }
            public Guid? ScopeId { get; set; }
            public abstract Int32 ErrorCode { get; }

            #endregion

            #region Constructor

            protected DHCPv6PacketHandledEvent()
            {

            }

            protected DHCPv6PacketHandledEvent(DHCPv6Packet request, DHCPv6Packet response, Boolean wasSuccessfullHandled)
            {
                Request = request;
                Response = response;
                WasSuccessfullHandled = wasSuccessfullHandled;
            }

            protected DHCPv6PacketHandledEvent(Guid scopeId, DHCPv6Packet request, DHCPv6Packet response, Boolean wasSuccessfullHandled) : this(request, response, wasSuccessfullHandled)
            {
                ScopeId = scopeId;
            }

            #endregion
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
        public class DHCPv6DeclineHandledEvent : DHCPv6PacketHandledEvent
        {
            public enum DeclineErros
            {
                NoError = 0,
                ScopeNotFound = 1,
                DeclineNotAllowed = 2,
                LeaseNotFound = 3,
                LeaseInInvalidState = 4,
                AddressAlreadySuspended = 5,
                IPAddressNotFound = 6,
            }

            public DeclineErros Error { get; set; }
            public override int ErrorCode => (Int32)Error;

            public DHCPv6DeclineHandledEvent()
            {

            }

            public DHCPv6DeclineHandledEvent(Guid scopeId, DHCPv6Packet request) : this(scopeId, request, DeclineErros.NoError)
            {

            }

            public DHCPv6DeclineHandledEvent(Guid scopeId, DHCPv6Packet request, DeclineErros error) : base(scopeId, request, DHCPv6Packet.Empty, error == DeclineErros.NoError)
            {
                Error = error;
            }

            public DHCPv6DeclineHandledEvent(DHCPv6Packet request) : this(request, DeclineErros.ScopeNotFound)
            {

            }

            public DHCPv6DeclineHandledEvent(DHCPv6Packet request, DeclineErros error) : base(request, DHCPv6Packet.Empty, false)
            {
                if (error != DeclineErros.ScopeNotFound)
                {
                    throw new ArgumentException("a scope id has to be specified if an error different from 'ScopeNotFound' is used");
                }

                Error = error;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
        public class DHCPv6SolicitHandledEvent : DHCPv6PacketHandledEvent
        {
            public enum SolicitErros
            {
                NoError = 0,
                ScopeNotFound = 1,
                NoAddressesLeft = 2,
                LeaseNotFound = 3,
                LeaseNotActive = 4,
                PrefixDelegationNotAvailable = 5,
            }

            public SolicitErros Error { get; set; }
            public Boolean IsRapitCommit { get; set; }
            public override Int32 ErrorCode => (Int32)Error;

            public DHCPv6SolicitHandledEvent()
            {

            }

            public DHCPv6SolicitHandledEvent(Guid scopeId, DHCPv6Packet request, DHCPv6Packet response, Boolean isRapitCommit) : this(scopeId, request, response, SolicitErros.NoError)
            {
                IsRapitCommit = isRapitCommit;
            }

            public DHCPv6SolicitHandledEvent(Guid scopeId, DHCPv6Packet request, SolicitErros error) : this(scopeId, request, DHCPv6Packet.Empty, error)
            {
                if (error == SolicitErros.NoError)
                {
                    throw new ArgumentException("if a request has no error, a response packet is needed");
                }
            }

            public DHCPv6SolicitHandledEvent(DHCPv6Packet request) : this(request, DHCPv6Packet.Empty, SolicitErros.ScopeNotFound)
            {

            }

            public DHCPv6SolicitHandledEvent(DHCPv6Packet request, SolicitErros error) : this(request, DHCPv6Packet.Empty, error)
            {

            }

            public DHCPv6SolicitHandledEvent(Guid scopeId, DHCPv6Packet request, DHCPv6Packet response, SolicitErros error) : base(scopeId, request, response, error == SolicitErros.NoError)
            {
                Error = error;
            }

            public DHCPv6SolicitHandledEvent(DHCPv6Packet request, DHCPv6Packet response, SolicitErros error) : base(request, response, error == SolicitErros.NoError)
            {
                if (error != SolicitErros.ScopeNotFound)
                {
                    throw new ArgumentException("a scope id has to be specified if an error different from 'ScopeNotFound' is used");
                }

                Error = error;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
        public class DHCPv6InformRequestHandledEvent : DHCPv6PacketHandledEvent
        {
            public enum InformRequestErros
            {
                NoError = 0,
                ScopeNotFound = 1,
                InformsNotAllowed = 2,
            }

            public InformRequestErros Error { get; set; }
            public override Int32 ErrorCode => (Int32)Error;

            public DHCPv6InformRequestHandledEvent()
            {

            }

            public DHCPv6InformRequestHandledEvent(Guid scopeId, DHCPv6Packet request, DHCPv6Packet response) : this(scopeId, request, response, InformRequestErros.NoError)
            {

            }

            public DHCPv6InformRequestHandledEvent(Guid scopeId, DHCPv6Packet request, InformRequestErros error) : this(scopeId, request, DHCPv6Packet.Empty, error)
            {
                if (error == InformRequestErros.NoError)
                {
                    throw new ArgumentException("if a request has no error, a response packet is needed");
                }
            }

            public DHCPv6InformRequestHandledEvent(DHCPv6Packet request) : this(request, DHCPv6Packet.Empty, InformRequestErros.ScopeNotFound)
            {

            }

            public DHCPv6InformRequestHandledEvent(DHCPv6Packet request, InformRequestErros error) : this(request, DHCPv6Packet.Empty, error)
            {

            }

            public DHCPv6InformRequestHandledEvent(Guid scopeId, DHCPv6Packet request, DHCPv6Packet response, InformRequestErros error) : base(scopeId, request, response, error == InformRequestErros.NoError)
            {
                Error = error;
            }

            public DHCPv6InformRequestHandledEvent(DHCPv6Packet request, DHCPv6Packet response, InformRequestErros error) : base(request, response, error == InformRequestErros.NoError)
            {
                if (error != InformRequestErros.ScopeNotFound)
                {
                    throw new ArgumentException("a scope id has to be specified if an error different from 'ScopeNotFound' is used");
                }

                Error = error;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
        public class DHCPv6ReleaseHandledEvent : DHCPv6PacketHandledEvent
        {
            public enum ReleaseError
            {
                NoError = 0,
                NoLeaseFound = 1,
                LeaseNotActive = 2,
                ScopeNotFound = 3,
            }

            public ReleaseError Error { get; set; }
            public override Int32 ErrorCode => (Int32)Error;

            public DHCPv6ReleaseHandledEvent()
            {

            }

            public DHCPv6ReleaseHandledEvent(Guid scopeId, DHCPv6Packet request) : this(scopeId, request, null, ReleaseError.NoError)
            {

            }

            public DHCPv6ReleaseHandledEvent(DHCPv6Packet request) : base(request, DHCPv6Packet.Empty, false)
            {
                Error = ReleaseError.ScopeNotFound;
            }

            public DHCPv6ReleaseHandledEvent(Guid scopeId, DHCPv6Packet request, DHCPv6Packet response, ReleaseError error) : base(scopeId, request, response, error == ReleaseError.NoError)
            {
                Error = error;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
        public class DHCPv6RequestHandledEvent : DHCPv6PacketHandledEvent
        {
            public enum RequestErrors
            {
                NoError = 0,
                ScopeNotFound = 1,
                LeaseNotFound = 3,
                LeaseNotInCorrectState = 8,
                LeasePendingButOnlyPrefixRequested = 9,
            }

            public RequestErrors Error { get; set; }
            public override Int32 ErrorCode => (Int32)Error;

            public DHCPv6RequestHandledEvent()
            {

            }

            public DHCPv6RequestHandledEvent(Guid scopeId, DHCPv6Packet request, DHCPv6Packet response) : this(scopeId, request, response, RequestErrors.NoError)
            {

            }

            public DHCPv6RequestHandledEvent(Guid scopeId, DHCPv6Packet request, RequestErrors error) : this(scopeId, request, DHCPv6Packet.Empty, error)
            {
                if (error == RequestErrors.NoError)
                {
                    throw new ArgumentException("if a request has no error, a response packet is needed");
                }
            }

            public DHCPv6RequestHandledEvent(DHCPv6Packet request) : this(request, DHCPv6Packet.Empty, RequestErrors.ScopeNotFound)
            {

            }

            public DHCPv6RequestHandledEvent(DHCPv6Packet request, RequestErrors error) : this(request, DHCPv6Packet.Empty, error)
            {

            }

            public DHCPv6RequestHandledEvent(Guid scopeId, DHCPv6Packet request, DHCPv6Packet response, RequestErrors error) : base(scopeId, request, response, error == RequestErrors.NoError)
            {
                Error = error;
            }

            public DHCPv6RequestHandledEvent(DHCPv6Packet request, DHCPv6Packet response, RequestErrors error) : base(request, response, error == RequestErrors.NoError)
            {
                if (error != RequestErrors.ScopeNotFound)
                {
                    throw new ArgumentException("a scope id has to be specified if an error different from 'ScopeNotFound' is used");
                }

                Error = error;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
        public class DHCPv6RenewHandledEvent : DHCPv6PacketHandledEvent
        {
            public enum RenewErrors
            {
                NoError = 0,
                ScopeNotFound = 1,
                LeaseNotFound = 3,
                NoAddressesAvaiable = 4,
                OnlyPrefixIsNotAllowed = 5,
            }

            public RenewErrors Error { get; set; }
            public override Int32 ErrorCode => (Int32)Error;

            public DHCPv6RenewHandledEvent()
            {

            }

            public DHCPv6RenewHandledEvent(Guid scopeId, DHCPv6Packet request, DHCPv6Packet response) : this(scopeId, request, response, RenewErrors.NoError)
            {

            }

            public DHCPv6RenewHandledEvent(Guid scopeId, DHCPv6Packet request, RenewErrors error) : this(scopeId, request, DHCPv6Packet.Empty, error)
            {
                if (error == RenewErrors.NoError)
                {
                    throw new ArgumentException("if a request has no error, a response packet is needed");
                }
            }

            public DHCPv6RenewHandledEvent(DHCPv6Packet request) : this(request, DHCPv6Packet.Empty, RenewErrors.ScopeNotFound)
            {

            }

            public DHCPv6RenewHandledEvent(DHCPv6Packet request, RenewErrors error) : this(request, DHCPv6Packet.Empty, error)
            {

            }

            public DHCPv6RenewHandledEvent(Guid scopeId, DHCPv6Packet request, DHCPv6Packet response, RenewErrors error) : base(scopeId, request, response, error == RenewErrors.NoError)
            {
                Error = error;
            }

            public DHCPv6RenewHandledEvent(DHCPv6Packet request, DHCPv6Packet response, RenewErrors error) : base(request, response, error == RenewErrors.NoError)
            {
                if (error != RenewErrors.ScopeNotFound)
                {
                    throw new ArgumentException("a scope id has to be specified if an error different from 'ScopeNotFound' is used");
                }

                Error = error;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
        public class DHCPv6RebindHandledEvent : DHCPv6PacketHandledEvent
        {
            public enum RebindErrors
            {
                NoError = 0,
                ScopeNotFound = 1,
                LeaseNotFound = 3,
                NoAddressesAvaiable = 4,
                OnlyPrefixIsNotAllowed = 5,
            }

            public RebindErrors Error { get; set; }
            public override Int32 ErrorCode => (Int32)Error;

            public DHCPv6RebindHandledEvent()
            {

            }

            public DHCPv6RebindHandledEvent(Guid scopeId, DHCPv6Packet request, DHCPv6Packet response) : this(scopeId, request, response, RebindErrors.NoError)
            {

            }

            public DHCPv6RebindHandledEvent(Guid scopeId, DHCPv6Packet request, RebindErrors error) : this(scopeId, request, DHCPv6Packet.Empty, error)
            {
                if (error == RebindErrors.NoError)
                {
                    throw new ArgumentException("if a request has no error, a response packet is needed");
                }
            }

            public DHCPv6RebindHandledEvent(DHCPv6Packet request) : this(request, DHCPv6Packet.Empty, RebindErrors.ScopeNotFound)
            {

            }

            public DHCPv6RebindHandledEvent(DHCPv6Packet request, RebindErrors error) : this(request, DHCPv6Packet.Empty, error)
            {

            }

            public DHCPv6RebindHandledEvent(Guid scopeId, DHCPv6Packet request, DHCPv6Packet response, RebindErrors error) : base(scopeId, request, response, error == RebindErrors.NoError)
            {
                Error = error;
            }

            public DHCPv6RebindHandledEvent(DHCPv6Packet request, DHCPv6Packet response, RebindErrors error) : base(request, response, error == RebindErrors.NoError)
            {
                if (error != RebindErrors.ScopeNotFound)
                {
                    throw new ArgumentException("a scope id has to be specified if an error different from 'ScopeNotFound' is used");
                }

                Error = error;
            }
        }

        public class DHCPv6ConfirmHandledEvent : DHCPv6PacketHandledEvent
        {
            public enum ConfirmErrors
            {
                NoError = 0,
                ScopeNotFound = 1,
                LeaseNotFound = 3,
                LeaseNotActive = 4,
                AddressMismtach = 5,
            }

            public ConfirmErrors Error { get; set; }
            public override Int32 ErrorCode => (Int32)Error;

            public DHCPv6ConfirmHandledEvent()
            {

            }

            public DHCPv6ConfirmHandledEvent(Guid scopeId, DHCPv6Packet request, DHCPv6Packet response) : this(scopeId, request, response, ConfirmErrors.NoError)
            {

            }

            public DHCPv6ConfirmHandledEvent(Guid scopeId, DHCPv6Packet request, ConfirmErrors error) : this(scopeId, request, DHCPv6Packet.Empty, error)
            {
                if (error == ConfirmErrors.NoError)
                {
                    throw new ArgumentException("if a request has no error, a response packet is needed");
                }
            }

            public DHCPv6ConfirmHandledEvent(DHCPv6Packet request) : this(request, DHCPv6Packet.Empty, ConfirmErrors.ScopeNotFound)
            {

            }

            public DHCPv6ConfirmHandledEvent(DHCPv6Packet request, ConfirmErrors error) : this(request, DHCPv6Packet.Empty, error)
            {

            }

            public DHCPv6ConfirmHandledEvent(Guid scopeId, DHCPv6Packet request, DHCPv6Packet response, ConfirmErrors error) : base(scopeId, request, response, error == ConfirmErrors.NoError)
            {
                Error = error;
            }

            public DHCPv6ConfirmHandledEvent(DHCPv6Packet request, DHCPv6Packet response, ConfirmErrors error) : base(request, response, error == ConfirmErrors.NoError)
            {
                if (error != ConfirmErrors.ScopeNotFound)
                {
                    throw new ArgumentException("a scope id has to be specified if an error different from 'ScopeNotFound' is used");
                }

                Error = error;
            }
        }
    }
}
