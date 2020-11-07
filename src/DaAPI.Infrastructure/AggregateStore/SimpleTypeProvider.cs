using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Infrastructure.AggregateStore
{
    //https://github.com/Eveneum/Eveneum/blob/master/Eveneum/Serialization/PlatformTypeProvider.cs/ 

    public class SimpleTypeProvider : ITypeProvider
    {
        private readonly ConcurrentDictionary<String, Type> Cache = new ConcurrentDictionary<string, Type>();

        public string GetIdentifierForType(Type type) => type.AssemblyQualifiedName;

        public Type GetTypeForIdentifier(String identifier) => Cache.GetOrAdd(identifier, t => Type.GetType(t));
    }
}
