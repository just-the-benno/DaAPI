using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes
{
    public abstract class ScopeCreateInstruction<TAddressProperties, TAddress, TScopeProperties, TScopeProperty, TOption, TValueType> : IDataTransferObject
        where TAddressProperties : ScopeAddressProperties<TAddressProperties, TAddress>
        where TAddress : IPAddress<TAddress>
        where TScopeProperties : ScopeProperties<TScopeProperty, TOption, TValueType>, new()
        where TScopeProperty : ScopeProperty<TOption, TValueType>
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }
        public CreateScopeResolverInformation ResolverInformation { get; set; }
        public TAddressProperties AddressProperties { get; set; }
        public TScopeProperties ScopeProperties { get; set; }

        internal virtual Boolean IsValid() => AddressProperties != null && ResolverInformation != null;
    }
}
