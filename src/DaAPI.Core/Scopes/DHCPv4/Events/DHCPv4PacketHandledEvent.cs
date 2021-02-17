using DaAPI.Core.Common;
using DaAPI.Core.Packets.DHCPv4;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv4
{
    public static class DHCPv4PacketHandledEvents
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
        public abstract class DHCPv4PacketHandledEvent : DomainEvent
        {
            #region Properties

            public DHCPv4Packet Request { get; set; }
            public DHCPv4Packet Response { get; set; }
            public Boolean WasSuccessfullHandled { get; set; }
            public Guid? ScopeId { get; set; }
            public abstract Int32 ErrorCode { get; }

            #endregion

            #region Constructor

            protected DHCPv4PacketHandledEvent()
            {

            }

            protected DHCPv4PacketHandledEvent(DHCPv4Packet request, DHCPv4Packet response, Boolean wasSuccessfullHandled)
            {
                Request = request;
                Response = response;
                WasSuccessfullHandled = wasSuccessfullHandled;
            }

            protected DHCPv4PacketHandledEvent(Guid scopeId, DHCPv4Packet request, DHCPv4Packet response, Boolean wasSuccessfullHandled) : this(request, response, wasSuccessfullHandled)
            {
                ScopeId = scopeId;
            }

            #endregion


        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
        public class DHCPv4DeclineHandledEvent : DHCPv4PacketHandledEvent
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

            public DHCPv4DeclineHandledEvent()
            {

            }

            public DHCPv4DeclineHandledEvent(Guid scopeId, DHCPv4Packet request) : this(scopeId, request, DeclineErros.NoError)
            {

            }

            public DHCPv4DeclineHandledEvent(Guid scopeId, DHCPv4Packet request, DeclineErros error) : base(scopeId, request, DHCPv4Packet.Empty, error == DeclineErros.NoError)
            {
                Error = error;
            }

            public DHCPv4DeclineHandledEvent(DHCPv4Packet request) : this(request, DeclineErros.ScopeNotFound)
            {

            }

            public DHCPv4DeclineHandledEvent(DHCPv4Packet request, DeclineErros error) : base(request, DHCPv4Packet.Empty, false)
            {
                if (error != DeclineErros.ScopeNotFound)
                {
                    throw new ArgumentException("a scope id has to be specified if an error different from 'ScopeNotFound' is used");
                }

                Error = error;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
        public class DHCPv4DiscoverHandledEvent : DHCPv4PacketHandledEvent
        {
            public enum DisoverErros
            {
                NoError = 0,
                ScopeNotFound = 1,
                NoAddressesLeft = 2,
            }

            public DisoverErros Error { get; set; }
            public override int ErrorCode => (Int32)Error;

            public DHCPv4DiscoverHandledEvent()
            {

            }

            public DHCPv4DiscoverHandledEvent(Guid scopeId, DHCPv4Packet request, DHCPv4Packet response) : this(scopeId, request, response, DisoverErros.NoError)
            {

            }

            public DHCPv4DiscoverHandledEvent(Guid scopeId, DHCPv4Packet request, DisoverErros error) : this(scopeId, request, DHCPv4Packet.Empty, error)
            {
                if (error == DisoverErros.NoError)
                {
                    throw new ArgumentException("if a request has no error, a response packet is needed");
                }
            }

            public DHCPv4DiscoverHandledEvent(DHCPv4Packet request) : this(request, DHCPv4Packet.Empty, DisoverErros.ScopeNotFound)
            {

            }

            public DHCPv4DiscoverHandledEvent(DHCPv4Packet request, DisoverErros error) : this(request, DHCPv4Packet.Empty, error)
            {

            }

            public DHCPv4DiscoverHandledEvent(Guid scopeId, DHCPv4Packet request, DHCPv4Packet response, DisoverErros error) : base(scopeId, request, response, error == DisoverErros.NoError)
            {
                Error = error;
            }

            public DHCPv4DiscoverHandledEvent(DHCPv4Packet request, DHCPv4Packet response, DisoverErros error) : base(request, response, error == DisoverErros.NoError)
            {
                if (error != DisoverErros.ScopeNotFound)
                {
                    throw new ArgumentException("a scope id has to be specified if an error different from 'ScopeNotFound' is used");
                }

                Error = error;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]

        public class DHCPv4InformHandledEvent : DHCPv4PacketHandledEvent
        {
            public enum InformErros
            {
                NoError = 0,
                ScopeNotFound = 1,
                InformsNotAllowed = 2,
            }

            public InformErros Error { get; set; }
            public override int ErrorCode => (Int32)Error;

            public DHCPv4InformHandledEvent()
            {

            }

            public DHCPv4InformHandledEvent(Guid scopeId, DHCPv4Packet request, DHCPv4Packet response) : this(scopeId, request, response, InformErros.NoError)
            {

            }

            public DHCPv4InformHandledEvent(Guid scopeId, DHCPv4Packet request, InformErros error) : this(scopeId, request, DHCPv4Packet.Empty, error)
            {
                if (error == InformErros.NoError)
                {
                    throw new ArgumentException("if a request has no error, a response packet is needed");
                }
            }

            public DHCPv4InformHandledEvent(DHCPv4Packet request) : this(request, DHCPv4Packet.Empty, InformErros.ScopeNotFound)
            {

            }

            public DHCPv4InformHandledEvent(DHCPv4Packet request, InformErros error) : this(request, DHCPv4Packet.Empty, error)
            {

            }

            public DHCPv4InformHandledEvent(Guid scopeId, DHCPv4Packet request, DHCPv4Packet response, InformErros error) : base(scopeId, request, response, error == InformErros.NoError)
            {
                Error = error;
            }

            public DHCPv4InformHandledEvent(DHCPv4Packet request, DHCPv4Packet response, InformErros error) : base(request, response, error == InformErros.NoError)
            {
                if (error != InformErros.ScopeNotFound)
                {
                    throw new ArgumentException("a scope id has to be specified if an error different from 'ScopeNotFound' is used");
                }

                Error = error;
            }


        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
        public class DHCPv4ReleaseHandledEvent : DHCPv4PacketHandledEvent
        {
            public enum ReleaseError
            {
                NoError = 0,
                NoLeaseFound = 1,
                LeaseNotActive = 2,
            }

            public ReleaseError Error { get; set; }
            public override int ErrorCode => (Int32)Error;

            public DHCPv4ReleaseHandledEvent()
            {

            }

            public DHCPv4ReleaseHandledEvent(Guid scopeId, DHCPv4Packet request) : this(scopeId, request, ReleaseError.NoError)
            {

            }

            public DHCPv4ReleaseHandledEvent(DHCPv4Packet request) : base(request, DHCPv4Packet.Empty, false)
            {
                Error = ReleaseError.NoLeaseFound;
            }

            public DHCPv4ReleaseHandledEvent(Guid scopeId, DHCPv4Packet request, ReleaseError error) : base(scopeId, request, DHCPv4Packet.Empty, error == ReleaseError.NoError)
            {
                Error = error;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
        public class DHCPv4RequestHandledEvent : DHCPv4PacketHandledEvent
        {
            public enum RequestErros
            {
                NoError = 0,
                ScopeNotFound = 1,
                LeaseNotFound = 3,
                LeaseNotPending = 4,
                RenewingNotAllowed = 5,
                NoAddressAvaiable = 6,
                LeaseNotActive = 7,
            }

            public RequestErros Error { get; set; }
            public override int ErrorCode => (Int32)Error;

            public DHCPv4RequestHandledEvent()
            {

            }

            public DHCPv4RequestHandledEvent(Guid scopeId, DHCPv4Packet request, DHCPv4Packet response) : this(scopeId, request, response, RequestErros.NoError)
            {

            }

            public DHCPv4RequestHandledEvent(Guid scopeId, DHCPv4Packet request, RequestErros error) : this(scopeId, request, DHCPv4Packet.Empty, error)
            {
                if (error == RequestErros.NoError)
                {
                    throw new ArgumentException("if a request has no error, a response packet is needed");
                }
            }

            public DHCPv4RequestHandledEvent(DHCPv4Packet request) : this(request, DHCPv4Packet.Empty, RequestErros.ScopeNotFound)
            {

            }

            public DHCPv4RequestHandledEvent(DHCPv4Packet request, RequestErros error) : this(request, DHCPv4Packet.Empty, error)
            {

            }

            public DHCPv4RequestHandledEvent(Guid scopeId, DHCPv4Packet request, DHCPv4Packet response, RequestErros error) : base(scopeId, request, response, error == RequestErros.NoError)
            {
                Error = error;
            }

            public DHCPv4RequestHandledEvent(DHCPv4Packet request, DHCPv4Packet response, RequestErros error) : base(request, response, error == RequestErros.NoError)
            {
                if (error != RequestErros.ScopeNotFound)
                {
                    throw new ArgumentException("a scope id has to be specified if an error different from 'ScopeNotFound' is used");
                }

                Error = error;
            }
        }


    }

}
