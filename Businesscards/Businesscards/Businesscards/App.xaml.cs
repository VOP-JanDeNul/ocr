using Businesscards.Views;
using System;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Businesscards.Data;

namespace Businesscards
{
    public partial class App : Application
    {
        static BusinesscardDatabase database;

        // Create the database connection as a singleton.
        public static BusinesscardDatabase Database
        {
            get
            {
                if (database == null)
                {
                    database = new BusinesscardDatabase(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Businesscards.db3"));
                }
                return database;
            }
        }


        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
