using Businesscards.Models;
using Businesscards.Views;
using System;
using Xamarin.Forms;

namespace Businesscards
{
    public partial class AppShell : Shell
    {
        private User user = User.InstanceUser;

        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(BusinesscardEntryPage), typeof(BusinesscardEntryPage));
            Routing.RegisterRoute(nameof(BusinesscardsPage), typeof(BusinesscardsPage));

            user.OriginUser = user.getOriginWithTxt();
        }

        private async void OnOriginMenuItemClicked(object sender, EventArgs e)
        {
            string email;

            if (user.OriginUser == null)
            {
                email = "origin@jandenul.be";
            }
            else
            {
                email = user.OriginUser;
            }

            string originPrompt = await DisplayPromptAsync("Login", "Give your email", placeholder: email, keyboard: Keyboard.Email);

            if (originPrompt != "" && originPrompt != null)
            {
                if(originPrompt == "Remove 1234")
                {
                    user.deleteOriginTxt();
                    user.OriginUser = null;
                }
                else
                {
                    user.OriginUser = originPrompt;
                    user.setOriginWithTxt();
                }
            }
            

            Shell.Current.FlyoutIsPresented = false;
        }

        private async void OnInfoMenuItemClicked(object sender, EventArgs e)
        {
            string text = "With this application you can have the text on business cards read and that information can be sent to the database of Jan De Nul Group." +
                            "\nOn the main screen you can see the business cards that still need to be sent. At the bottom you have 3 buttons. With the left button you can enter the data manually. With the middle button you can select an image from a business card out from gallery of your smartphone. With the right button you can immediately take a photo of a card and have it processed by the application." +
                            "\nThen you come to the entry page where the fields were filled in as accurately as possible according to the data on the business card. If a value is not in the correct field, the contents of 2 fields can be swapped using the swap buttons next to each field." +
                            "\nIf you agree with the content of all fields, you can click on the save button and the content will be sent to the database. If not, click on the delete button and this card will be deleted.";

            await DisplayAlert("Info", text, "OK");

            Shell.Current.FlyoutIsPresented = false;
        }
    }
}
