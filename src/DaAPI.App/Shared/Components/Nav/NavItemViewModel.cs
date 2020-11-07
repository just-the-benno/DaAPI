using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.App.Shared.Components.Nav
{
    public class NavItemViewModel
    {
        public String Link { get; set; }
        public String IconClass { get; set; }
        public String Caption { get; set; }
        public Int32 NotifictionAmount { get; set; }

        public IEnumerable<NavItemViewModel> SubItems { get; set; }

        public NavItemViewModel()
        {
            SubItems = Array.Empty<NavItemViewModel>();
        }
    }
}
