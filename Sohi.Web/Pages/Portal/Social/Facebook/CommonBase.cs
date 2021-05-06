using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Sohi.Models;

namespace Sohi.Web.Pages.Portal.Social.Facebook
{
    public class CommonBase : LayoutComponentBase
    {
        private bool collapseNavMenu = true;

        protected string NavMenuCssClass => collapseNavMenu ? "collapse" : null;

        protected void ToggleNavMenu()
        {
            collapseNavMenu = !collapseNavMenu;
        }


        public IEnumerable<Lead> Leads { get; set; }
    }
}
