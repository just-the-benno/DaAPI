using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Services
{
    public interface ISerializer
    {
        T Deserialze<T>(String input);
        String Seralize<T>(T data);
    }
}
