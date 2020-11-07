using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Infrastructure.AggregateStore
{
    public interface ITypeProvider
    {
        String GetIdentifierForType(Type type);
        Type GetTypeForIdentifier(String identifier);
    }
}
