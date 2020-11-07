using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.TestHelper
{
    public static class InputHelper
    {
        public static IEnumerable<String> GetEmptyStringInputs() =>
            new List<string> { String.Empty, null, " ", "\n" };
    }
}
