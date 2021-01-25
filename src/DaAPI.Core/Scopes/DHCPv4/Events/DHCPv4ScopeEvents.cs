using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv4
{
    public static class DHCPv4ScopeEvents
    {
        public class DHCPv4ScopeAddedEvent : DomainEvent
        {
            #region Properties

            public DHCPv4ScopeCreateInstruction Instructions { get; private set; }

            #endregion

            #region Constructor

            public DHCPv4ScopeAddedEvent(DHCPv4ScopeCreateInstruction instructions)
            {
                Instructions = instructions;
            }

            #endregion
        }

        public class DHCPv4ScopePropertiesUpdatedEvent : EntityBasedDomainEvent
        {
            public DHCPv4ScopeProperties Properties { get; set; }

            public DHCPv4ScopePropertiesUpdatedEvent()
            {

            }

            public DHCPv4ScopePropertiesUpdatedEvent(Guid scopeId, DHCPv4ScopeProperties properites) : base(scopeId)
            {
                Properties = properites;
            }
        }

        public class DHCPv4ScopeNameUpdatedEvent : EntityBasedDomainEvent
        {
            public string Name { get; set; }

            public DHCPv4ScopeNameUpdatedEvent()
            {

            }

            public DHCPv4ScopeNameUpdatedEvent(Guid scopeId, ScopeName name) : base(scopeId)
            {
                Name = name;
            }
        }

        public class DHCPv4ScopeDescriptionUpdatedEvent : EntityBasedDomainEvent
        {
            public string Description { get; set; }

            public DHCPv4ScopeDescriptionUpdatedEvent()
            {

            }

            public DHCPv4ScopeDescriptionUpdatedEvent(Guid scopeId, ScopeDescription description) : base(scopeId)
            {
                Description = description;
            }
        }

        public class DHCPv4ScopeResolverUpdatedEvent : EntityBasedDomainEvent
        {
            public CreateScopeResolverInformation ResolverInformationen { get; set; }

            public DHCPv4ScopeResolverUpdatedEvent()
            {

            }

            public DHCPv4ScopeResolverUpdatedEvent(Guid scopeId, CreateScopeResolverInformation information) : base(scopeId)
            {
                ResolverInformationen = information;
            }
        }

        public class DHCPv4ScopeAddressPropertiesUpdatedEvent : EntityBasedDomainEvent
        {
            public DHCPv4ScopeAddressProperties AddressProperties { get; set; }

            public DHCPv4ScopeAddressPropertiesUpdatedEvent()
            {

            }

            public DHCPv4ScopeAddressPropertiesUpdatedEvent(Guid scopeId, DHCPv4ScopeAddressProperties addressProperties) : base(scopeId)
            {
                AddressProperties = addressProperties;
            }
        }

        public class DHCPv4ScopeParentUpdatedEvent : EntityBasedDomainEvent
        {
            public Guid? ParentId { get; set; }

            public DHCPv4ScopeParentUpdatedEvent()
            {

            }

            public DHCPv4ScopeParentUpdatedEvent(Guid scopeId, Guid? parentId) : base(scopeId)
            {
                ParentId = parentId;
            }
        }

        public class DHCPv4ScopeDeletedEvent : EntityBasedDomainEvent
        {
            public bool IncludeChildren { get; set; }

            public DHCPv4ScopeDeletedEvent()
            {

            }

            public DHCPv4ScopeDeletedEvent(Guid scopeId, bool includeChildren) : base(scopeId)
            {
                IncludeChildren = includeChildren;
            }
        }

        public class DHCPv4ScopeAddressesAreExhaustedEvent : EntityBasedDomainEvent
        {
            public DHCPv4ScopeAddressesAreExhaustedEvent() : base()
            {

            }

            public DHCPv4ScopeAddressesAreExhaustedEvent(Guid id) : base(id)
            {

            }
        }

        public class DHCPv4ScopeSuspendedEvent : EntityBasedDomainEvent
        {
            public DHCPv4ScopeSuspendedEvent() : base()
            {

            }

            public DHCPv4ScopeSuspendedEvent(Guid id) : base(id)
            {

            }
        }

        public class DHCPv4ScopeReactivedEvent : EntityBasedDomainEvent
        {
            public DHCPv4ScopeReactivedEvent() : base()
            {

            }

            public DHCPv4ScopeReactivedEvent(Guid id) : base(id)
            {

            }
        }


    }
}
