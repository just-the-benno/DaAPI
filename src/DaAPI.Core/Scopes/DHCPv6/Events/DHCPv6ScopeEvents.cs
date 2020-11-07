using DaAPI.Core.Common;
using DaAPI.Core.Scopes.DHCPv6;
using DaAPI.Core.Scopes.DHCPv6.ScopeProperties;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv6
{
    public static class DHCPv6ScopeEvents
    {
        public class DHCPv6ScopeAddedEvent : DomainEvent
        {
            #region Properties

            public DHCPv6ScopeCreateInstruction Instructions { get;  set; }

            #endregion

            #region Constructor

            public DHCPv6ScopeAddedEvent()
            {

            }

            public DHCPv6ScopeAddedEvent(DHCPv6ScopeCreateInstruction instructions)
            {
                Instructions = instructions;
            }

            #endregion
        }

        public class DHCPv6ScopePropertiesUpdatedEvent : EntityBasedDomainEvent
        {
            public DHCPv6ScopeProperties Properties { get; set; }

            public DHCPv6ScopePropertiesUpdatedEvent()
            {

            }

            public DHCPv6ScopePropertiesUpdatedEvent(Guid scopeId, DHCPv6ScopeProperties properites) : base(scopeId)
            {
                Properties = properites;
            }
        }

        public class DHCPv6ScopeNameUpdatedEvent : EntityBasedDomainEvent
        {
            public String Name { get; set; }

            public DHCPv6ScopeNameUpdatedEvent()
            {

            }

            public DHCPv6ScopeNameUpdatedEvent(Guid scopeId, ScopeName name) : base(scopeId)
            {
                Name = name;
            }
        }

        public class DHCPv6ScopeDescriptionUpdatedEvent : EntityBasedDomainEvent
        {
            public String Description { get; set; }

            public DHCPv6ScopeDescriptionUpdatedEvent()
            {

            }

            public DHCPv6ScopeDescriptionUpdatedEvent(Guid scopeId, ScopeDescription description) : base(scopeId)
            {
                Description = description;
            }
        }

        public class DHCPv6ScopeResolverUpdatedEvent : EntityBasedDomainEvent
        {
            public CreateScopeResolverInformation ResolverInformationen { get; set; }

            public DHCPv6ScopeResolverUpdatedEvent()
            {

            }

            public DHCPv6ScopeResolverUpdatedEvent(Guid scopeId, CreateScopeResolverInformation information) : base(scopeId)
            {
                ResolverInformationen = information;
            }
        }

        public class DHCPv6ScopeAddressPropertiesUpdatedEvent : EntityBasedDomainEvent
        {
            public DHCPv6ScopeAddressProperties AddressProperties { get; set; }

            public DHCPv6ScopeAddressPropertiesUpdatedEvent()
            {

            }

            public DHCPv6ScopeAddressPropertiesUpdatedEvent(Guid scopeId, DHCPv6ScopeAddressProperties addressProperties) : base(scopeId)
            {
                AddressProperties = addressProperties;
            }
        }

        public class DHCPv6ScopeParentUpdatedEvent : EntityBasedDomainEvent
        {
            public Guid? ParentId { get; set; }

            public DHCPv6ScopeParentUpdatedEvent()
            {

            }

            public DHCPv6ScopeParentUpdatedEvent(Guid scopeId, Guid? parentId) : base(scopeId)
            {
                ParentId = parentId;
            }
        }

        public class DHCPv6ScopeDeletedEvent : EntityBasedDomainEvent
        {
            public bool IncludeChildren { get; set; }

            public DHCPv6ScopeDeletedEvent()
            {

            }

            public DHCPv6ScopeDeletedEvent(Guid scopeId, bool includeChildren) : base(scopeId)
            {
                IncludeChildren = includeChildren;
            }
        }

        public class DHCPv6ScopeAddressesAreExhaustedEvent : EntityBasedDomainEvent
        {
            public DHCPv6ScopeAddressesAreExhaustedEvent() : base()
            {

            }

            public DHCPv6ScopeAddressesAreExhaustedEvent(Guid id) : base(id)
            {

            }
        }

        public class DHCPv6ScopeSuspendedEvent : EntityBasedDomainEvent
        {
            public DHCPv6ScopeSuspendedEvent() : base()
            {

            }

            public DHCPv6ScopeSuspendedEvent(Guid id) : base(id)
            {

            }
        }

        public class DHCPv6ScopeReactivedEvent : EntityBasedDomainEvent
        {
            public DHCPv6ScopeReactivedEvent() : base()
            {

            }

            public DHCPv6ScopeReactivedEvent(Guid id) : base(id)
            {

            }
        }


    }
}
