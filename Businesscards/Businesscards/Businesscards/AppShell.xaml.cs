using Businesscards.Views;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Businesscards
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(BusinesscardEntryPage), typeof(BusinesscardEntryPage));
            Routing.RegisterRoute(nameof(BusinesscardsPage), typeof(BusinesscardsPage));
            Shell.SetTabBarIsVisible(this, false);
        }

    }
}
