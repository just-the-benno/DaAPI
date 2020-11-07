using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv4;
using DaAPI.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Infrastructure.Services
{
    public class DHCPv4ScopeResolverManager : IDHCPv4ScopeResolverManager
    {
        #region Fields

        private readonly Dictionary<String, Func<IDHCPv4ScopeResolver>> _dhcpv4ScopeMapper = new Dictionary<String, Func<IDHCPv4ScopeResolver>>();

        #endregion

        #region Properties


        #endregion

        #region Constructor

        public DHCPv4ScopeResolverManager()
        {
        }

        #endregion

        #region Methods

        private String GetNormalizedMapperName(String input) => input.Trim().ToLower();

        public void AddOrUpdateDHCPv4ScopeResolver(String name, Func<IDHCPv4ScopeResolver> activator)
        {
            if (activator == null)
            {
                throw new ArgumentNullException(nameof(activator));
            }

            String normalizeNamed = GetNormalizedMapperName(name);
            if (_dhcpv4ScopeMapper.ContainsKey(normalizeNamed) == false)
            {
                _dhcpv4ScopeMapper.Add(normalizeNamed, activator);
            }
            else
            {
                _dhcpv4ScopeMapper[normalizeNamed] = activator;
            }
        }

        public Boolean RemoveDHCPv4ScopeResolver(String name)
        {
            return _dhcpv4ScopeMapper.Remove(name);
        }

        private IDHCPv4ScopeResolver GetResolverFromCreateModel(DHCPv4CreateScopeResolverInformation resolverCreateModel, Boolean applyValues)
        {
            String normalizeNamed = GetNormalizedMapperName(resolverCreateModel.Typename);
            if (_dhcpv4ScopeMapper.ContainsKey(normalizeNamed) == false) { throw new Exception(); }

            IDHCPv4ScopeResolver resolver = _dhcpv4ScopeMapper[normalizeNamed].Invoke();
            if (applyValues == true)
            {
                resolver.ApplyValues(resolverCreateModel.PropertiesAndValues);
            }

            return resolver;
        }

        private void GenerateResolverTree(IEnumerable<DHCPv4CreateScopeResolverInformation> resolvers, IDHCPv4ScopeResolverContainingOtherResolvers parent)
        {
            foreach (DHCPv4CreateScopeResolverInformation item in resolvers)
            {
                IDHCPv4ScopeResolver resolver = GetResolverFromCreateModel(item, true);
                parent.AddResolver(resolver);

                if (resolver is IDHCPv4ScopeResolverContainingOtherResolvers == true)
                {
                    IDHCPv4ScopeResolverContainingOtherResolvers child = (IDHCPv4ScopeResolverContainingOtherResolvers)resolver;
                    IEnumerable<DHCPv4CreateScopeResolverInformation> innenrResolvers = child.ExtractResolverCreateModels(item);
                    GenerateResolverTree(innenrResolvers, child);
                }
            }
        }

        public IDHCPv4ScopeResolver InitializeResolver(DHCPv4CreateScopeResolverInformation resolverCreateModel)
        {
            IDHCPv4ScopeResolver resolver = GetResolverFromCreateModel(resolverCreateModel, true);

            if (resolver is IDHCPv4ScopeResolverContainingOtherResolvers == true)
            {
                IDHCPv4ScopeResolverContainingOtherResolvers resolverContainingOthers = (IDHCPv4ScopeResolverContainingOtherResolvers)resolver;
                IEnumerable<DHCPv4CreateScopeResolverInformation> innenrResolvers = resolverContainingOthers.ExtractResolverCreateModels(resolverCreateModel);
                GenerateResolverTree(innenrResolvers, resolverContainingOthers);
            }

            return resolver;
        }

        public Boolean ValidateDHCPv4Resolver(DHCPv4CreateScopeResolverInformation resolverCreateModel)
        {
            if (resolverCreateModel == null || String.IsNullOrEmpty(resolverCreateModel.Typename) == true)
            {
                return false;
            }

            String normalizeNamed = GetNormalizedMapperName(resolverCreateModel.Typename);
            if (_dhcpv4ScopeMapper.ContainsKey(normalizeNamed) == false) { return false; }

            IDHCPv4ScopeResolver resolver = _dhcpv4ScopeMapper[normalizeNamed].Invoke();
            Boolean result = resolver.ArePropertiesAndValuesValid(resolverCreateModel.PropertiesAndValues);

            if (result == true && resolver is IDHCPv4ScopeResolverContainingOtherResolvers resolvers)
            {
                IDHCPv4ScopeResolverContainingOtherResolvers resolverContainingOthers = resolvers;
                IEnumerable<DHCPv4CreateScopeResolverInformation> innenrResolvers = resolverContainingOthers.ExtractResolverCreateModels(resolverCreateModel);

                foreach (DHCPv4CreateScopeResolverInformation item in innenrResolvers)
                {
                    Boolean childResult = ValidateDHCPv4Resolver(item);
                    if (childResult == false)
                    {
                        result = false;
                        break;
                    }
                }
            }

            return result;

            //Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            //foreach (Assembly assembly in assemblies)
            //{
            //    Type[] types = assembly.GetTypes();

            //    foreach (Type type in types)
            //    {
            //        Type interfaceType = type.GetInterface(nameof(IDHCPv4ScopeResolver));
            //        if (interfaceType == null) { continue; }

            //        ConstructorInfo constructorInfo = type.GetConstructor(new Type[] { });
            //        if(constructorInfo == null) { continue; }

            //        Object resolver = constructorInfo.Invoke(new object[0]);

            //    }
            //}
        }

        public IEnumerable<ScopeResolverDescription> GetRegisterResolverDescription()
        {
            List<ScopeResolverDescription> result = new List<ScopeResolverDescription>();

            foreach (var item in _dhcpv4ScopeMapper)
            {
                IDHCPv4ScopeResolver resolver = item.Value();
                ScopeResolverDescription resolverDescription = resolver.GetDescription();

                result.Add(resolverDescription);
            }

            return result;
        }

        #endregion
    }
}
