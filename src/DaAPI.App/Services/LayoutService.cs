using DaAPI.App.Shared.Components.Breadcrumb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.App.Services
{
    public class LayoutService
    {
        public String PageTitle { get; private set; }
        public IEnumerable<BreadcrumbViewModel> Breadcrumbs { get; private set; }

        public event EventHandler<EventArgs> PageTitleChanged;
        public event EventHandler<EventArgs> BreadcrumbsChanged;

        public LayoutService()
        {
            Breadcrumbs = Array.Empty<BreadcrumbViewModel>();
        }

        public void UpdatePageTitle(String title)
        {
            PageTitle = title;
            PageTitleChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateBreadcrumbs(IEnumerable<BreadcrumbViewModel> breadcrumbs)
        {
            Breadcrumbs = new List<BreadcrumbViewModel>(breadcrumbs);
            BreadcrumbsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
