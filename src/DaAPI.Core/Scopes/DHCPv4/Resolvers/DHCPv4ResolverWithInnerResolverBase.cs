using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DaAPI.Core.Scopes.DHCPv4
{
    public abstract class DHCPv4ResolverWithInnerResolverBase : IDHCPv4ScopeResolverContainingOtherResolvers
    {
        #region Fields and Constants

        private const String _innerResolverName = "InnerResolvers";

        #endregion

        #region Properties

        protected ICollection<IDHCPv4ScopeResolver> InnerResolvers { get; private set; } = new List<IDHCPv4ScopeResolver>();
        protected ISerializer Serializer {  get; }

        public virtual Boolean ForceReuseOfAddress => false;
        public virtual Boolean HasUniqueIdentifier => false;

        #endregion

        #region Constructor

        public DHCPv4ResolverWithInnerResolverBase(ISerializer serializer)
        {
            Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        public virtual Boolean AddResolver(IDHCPv4ScopeResolver resolver)
        {
            if (InnerResolvers.Contains(resolver) == true) { return false; }

            InnerResolvers.Add(resolver);
            return true;
        }

        public IEnumerable<DHCPv4CreateScopeResolverInformation> ExtractResolverCreateModels(DHCPv4CreateScopeResolverInformation item)
        {
            String rawvalue = item.PropertiesAndValues[_innerResolverName];
            IEnumerable<DHCPv4CreateScopeResolverInformation> result = Serializer.Deserialze<IEnumerable<DHCPv4CreateScopeResolverInformation>>(rawvalue);
            return result;
        }

        public abstract bool PacketMeetsCondition(DHCPv4Packet packet);

        public virtual byte[] GetUniqueIdentifier(DHCPv4Packet packet)
        {
            throw new NotImplementedException();
        }

        public virtual Boolean ArePropertiesAndValuesValid(IDictionary<String, String> propertiesAndValues)
        {
            if(propertiesAndValues == null) { return false; }

            if(propertiesAndValues.ContainsKey(_innerResolverName) == false)
            {
                return false;
            }

            return true;
        }

        public virtual void ApplyValues(IDictionary<String, String> propertiesAndValues)
        {
           return;
        }


        public virtual IEnumerable<ScopeResolverPropertyDescription> GetPropertyDescriptions()
        {
            return new List<ScopeResolverPropertyDescription>();
        }

        public ScopeResolverDescription GetDescription()
        {
            ScopeResolverDescription description = new ScopeResolverDescription(
                base.GetType().Name,
                (new List<ScopeResolverPropertyDescription>
                {
                    new ScopeResolverPropertyDescription( _innerResolverName, ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.Resolvers ),
                }).Union(GetPropertyDescriptions()));

            return description;
        }

        #endregion
    }
}
