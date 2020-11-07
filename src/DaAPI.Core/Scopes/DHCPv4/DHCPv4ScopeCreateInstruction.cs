using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv4
{
    public class DHCPv4ScopeCreateInstruction : IDataTransferObject
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }
        public DHCPv4CreateScopeResolverInformation ResolverInformations { get; set; }
        public DHCPv4ScopeAddressProperties AddressProperties { get; set; }
        public DHCPv4ScopeProperties Properties { get; set; }

    }
}
