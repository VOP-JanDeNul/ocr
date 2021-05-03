using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;

namespace Businesscards.Services.Settings
{
    public static class SettingsService
    {
        public static bool FirstRun
        {
            get => Preferences.Get(nameof(FirstRun), true);
            set => Preferences.Set(nameof(FirstRun), value);
        }
    }
}
