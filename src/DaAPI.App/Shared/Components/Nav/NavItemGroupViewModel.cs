using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.App.Shared.Components.Nav
{
    public class NavItemGroupViewModel
    {
        public String Caption { get; set; }
        public Boolean DisplayCaption { get; set; }
        public IEnumerable<NavItemViewModel> NavItems { get; set; }
    }
}
