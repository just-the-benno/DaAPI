using DaAPI.Core.Common;
using DaAPI.Core.Packets;
using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DaAPI.Core.Scopes
{
    public abstract class ScopeResolverContainingOtherResolvers<TPacket, TAddress> : IScopeResolverContainingOtherResolvers<TPacket, TAddress>
        where TPacket : DHCPPacket<TPacket, TAddress>
        where TAddress : IPAddress<TAddress>
    {
        #region Fields and Constants

        private const String _innerResolverName = "InnerResolvers";

        #endregion

        #region Properties

        protected ICollection<IScopeResolver<TPacket, TAddress>> InnerResolvers { get; private set; } = new List<IScopeResolver<TPacket, TAddress>>();

        public IEnumerable<IScopeResolver<TPacket, TAddress>> GetScopeResolvers() => InnerResolvers.AsEnumerable();

        public virtual Boolean ForceReuseOfAddress => false;
        public virtual Boolean HasUniqueIdentifier => false;

        #endregion

        #region Constructor

        public ScopeResolverContainingOtherResolvers()
        {
        }

        public virtual Boolean AddResolver(IScopeResolver<TPacket, TAddress> resolver)
        {
            if (InnerResolvers.Contains(resolver) == true) { return false; }

            InnerResolvers.Add(resolver);
            return true;
        }

        public IEnumerable<CreateScopeResolverInformation> ExtractResolverCreateModels(CreateScopeResolverInformation item, ISerializer serializer)
        {
            String rawvalue = item.PropertiesAndValues[_innerResolverName];
            IEnumerable<CreateScopeResolverInformation> result = serializer.Deserialze<IEnumerable<CreateScopeResolverInformation>>(rawvalue);
            return result;
        }

        public abstract bool PacketMeetsCondition(TPacket packet);

        public virtual byte[] GetUniqueIdentifier(TPacket packet) => throw new InvalidOperationException();

        public virtual Boolean ArePropertiesAndValuesValid(IDictionary<String, String> propertiesAndValues, ISerializer serializer)
        {
            if (propertiesAndValues == null) { return false; }

            if (propertiesAndValues.ContainsKey(_innerResolverName) == false)
            {
                return false;
            }

            return true;
        }

        public virtual void ApplyValues(IDictionary<String, String> propertiesAndValue, ISerializer serializers)
        {
            return;
        }


        public virtual IEnumerable<ScopeResolverPropertyDescription> GetPropertyDescriptions() => new List<ScopeResolverPropertyDescription>();

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

        public IDictionary<string, string> GetValues() => throw new NotImplementedException();

        #endregion
    }
}
