using DaAPI.Core.Common;
using DaAPI.Core.Packets;
using DaAPI.Core.Scopes;
using DaAPI.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Infrastructure.Services
{
    public abstract class ScopeResolverManagerBase<TPacket, TAddress> : IScopeResolverManager<TPacket, TAddress>
        where TPacket : DHCPPacket<TPacket, TAddress>
        where TAddress : IPAddress<TAddress>
    {

        #region Fields

        private readonly Dictionary<String, Func<IScopeResolver<TPacket, TAddress>>> _resolverMapper =
            new Dictionary<String, Func<IScopeResolver<TPacket, TAddress>>>();

        protected readonly ILogger _logger;
        private readonly ISerializer _serializer;

        #endregion

        #region Properties


        #endregion

        #region Constructor

        protected ScopeResolverManagerBase(ISerializer serializer, ILogger logger)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Methods

        private static String GetNormalizedMapperName(String input) => input.Trim().ToLower();

        public void AddOrUpdateScopeResolver(string name, Func<IScopeResolver<TPacket, TAddress>> activator)
        {
            if (activator == null)
            {
                throw new ArgumentNullException(nameof(activator));
            }

            String normalizeNamed = GetNormalizedMapperName(name);
            if (_resolverMapper.ContainsKey(normalizeNamed) == false)
            {
                _resolverMapper.Add(normalizeNamed, activator);
            }
            else
            {
                _resolverMapper[normalizeNamed] = activator;
            }
        }

        public Boolean RemoveResolver(String name)
        {
            String normalizeNamed = GetNormalizedMapperName(name);
            return _resolverMapper.Remove(normalizeNamed);
        }

        private IScopeResolver<TPacket, TAddress> GetResolverFromCreateModel(CreateScopeResolverInformation resolverCreateModel, Boolean applyValues)
        {
            String normalizeNamed = GetNormalizedMapperName(resolverCreateModel.Typename);
            if (_resolverMapper.ContainsKey(normalizeNamed) == false) { throw new Exception(); }

            IScopeResolver<TPacket, TAddress> resolver = _resolverMapper[normalizeNamed].Invoke();
            if (applyValues == true)
            {
                resolver.ApplyValues(resolverCreateModel.PropertiesAndValues, _serializer);
            }

            return resolver;
        }

        public IScopeResolver<TPacket, TAddress> InitializeResolver(CreateScopeResolverInformation resolverCreateModel)
        {
            IScopeResolver<TPacket, TAddress> resolver = GetResolverFromCreateModel(resolverCreateModel, true);

            if (resolver is IScopeResolverContainingOtherResolvers<TPacket, TAddress> == true)
            {
                var resolverContainingOthers = (IScopeResolverContainingOtherResolvers<TPacket, TAddress>)resolver;
                IEnumerable<CreateScopeResolverInformation> innenrResolvers = resolverContainingOthers.ExtractResolverCreateModels(resolverCreateModel, _serializer);
                GenerateResolverTree(innenrResolvers, resolverContainingOthers);
            }

            return resolver;
        }

        public bool IsResolverInformationValid(CreateScopeResolverInformation resolverCreateModel)
        {
            if (resolverCreateModel == null || String.IsNullOrEmpty(resolverCreateModel.Typename) == true)
            {
                return false;
            }

            String normalizeNamed = GetNormalizedMapperName(resolverCreateModel.Typename);
            if (_resolverMapper.ContainsKey(normalizeNamed) == false) { return false; }

            IScopeResolver<TPacket, TAddress> resolver = _resolverMapper[normalizeNamed].Invoke();

            Boolean result = resolver.ArePropertiesAndValuesValid(resolverCreateModel.PropertiesAndValues, _serializer);

            if (result == true && resolver is IScopeResolverContainingOtherResolvers<TPacket, TAddress> resolvers)
            {
                var resolverContainingOthers = resolvers;
                IEnumerable<CreateScopeResolverInformation> innenrResolvers = resolverContainingOthers.ExtractResolverCreateModels(resolverCreateModel, _serializer);

                foreach (CreateScopeResolverInformation item in innenrResolvers)
                {
                    Boolean childResult = IsResolverInformationValid(item);
                    if (childResult == false)
                    {
                        result = false;
                        break;
                    }
                }
            }

            return result;
        }

        private void GenerateResolverTree(IEnumerable<CreateScopeResolverInformation> resolvers, IScopeResolverContainingOtherResolvers<TPacket, TAddress> parent)
        {
            foreach (CreateScopeResolverInformation item in resolvers)
            {
                var resolver = GetResolverFromCreateModel(item, true);
                parent.AddResolver(resolver);

                if (resolver is IScopeResolverContainingOtherResolvers<TPacket, TAddress> == true)
                {
                    var child = (IScopeResolverContainingOtherResolvers<TPacket, TAddress>)resolver;
                    IEnumerable<CreateScopeResolverInformation> innenrResolvers = child.ExtractResolverCreateModels(item, _serializer);
                    GenerateResolverTree(innenrResolvers, child);
                }
            }
        }

        public IEnumerable<ScopeResolverDescription> GetRegisterResolverDescription()
        {
            List<ScopeResolverDescription> result = new List<ScopeResolverDescription>();

            foreach (var item in _resolverMapper)
            {
                IScopeResolver<TPacket, TAddress> resolver = item.Value();
                ScopeResolverDescription resolverDescription = resolver.GetDescription();

                result.Add(resolverDescription);
            }

            return result;
        }

        #endregion
    }
}
