using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.App.Services
{
    public class SignOutService
    {
        private readonly NavigationManager _navigationManager;
        private readonly SignOutSessionStateManager _signOutManager;

        public SignOutService(NavigationManager navigationManager, SignOutSessionStateManager signOutManager)
        {
            this._navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
            this._signOutManager = signOutManager ?? throw new ArgumentNullException(nameof(signOutManager));
        }

        public async Task BeginSignOut()
        {
            await _signOutManager.SetSignOutState();
            _navigationManager.NavigateTo("authentication/logout");
        }

    }
}
