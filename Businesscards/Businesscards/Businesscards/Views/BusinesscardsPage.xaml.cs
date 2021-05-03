using Businesscards.Models;
using Businesscards.Services.ImageTransformation;
using Businesscards.Services.Settings;
using Businesscards.Services.Toast;
using Businesscards.ViewModels;
using System.Diagnostics;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Businesscards.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BusinesscardsPage : ContentPage
    {
        private BusinesscardsViewModel viewModel;

        // Initialize page and set binding context to BusinesscardsViewModel
        public BusinesscardsPage()
        {
            InitializeComponent();
            viewModel = new BusinesscardsViewModel(Navigation);

            User user = User.InstanceUser;

            if (SettingsService.FirstRun)
            {
                FirstTimeRunned();
                SettingsService.FirstRun = false;
            }

            MessagingCenter.Subscribe<BusinesscardsViewModel>(this, "emptyOrigin", async (sender) =>
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

                string originPrompt = await DisplayPromptAsync("Unknown user!", "Before adding a card, fill in your email address.", placeholder: email, keyboard: Keyboard.Email);
                
                if (originPrompt != "" && originPrompt != null)
                {
                    user.OriginUser = originPrompt;
                    user.setOriginWithTxt();
                }
            });

            MessagingCenter.Subscribe<BusinesscardsViewModel>(this, "BadRequest", (sender) =>
            {
                //await DisplayAlert("Alert", "The picture wasn't clear enough, consider holding it in the air and a bit further away.", "OK");
                DependencyService.Get<IToast>().LongAlert("The picture wasn't clear enough.\nConsider holding it in the air and a bit further away.");
            });

            MessagingCenter.Subscribe<BusinesscardsViewModel>(this, "GeneralError", (sender) =>
            {
                //await DisplayAlert("Alert", "Something went wrong. Check your internet connection. If this keeps happening restart the app.", "OK");
                DependencyService.Get<IToast>().LongAlert("Something went wrong.\nCheck your internet connection.\nIf this keeps happening restart the app.");
            });

            MessagingCenter.Subscribe<BusinesscardsViewModel>(this, "TimeoutException", (sender) =>
            {
                //await DisplayAlert("Alert", "Scanning took too long. Check your internet connection. If this keeps happening restart the app.", "OK");
                DependencyService.Get<IToast>().LongAlert("Scanning took too long.\nCheck your internet connection.\nIf this keeps happening restart the app.");
            });

            MessagingCenter.Subscribe<BusinesscardsViewModel>(this, "Sending", (sender) =>
            {
                // disable the buttons and collectionview
                manualButton.IsEnabled = false;
                galeryButton.IsEnabled = false;
                cameraButton.IsEnabled = false;
                collectionView.SelectionMode = SelectionMode.None;

                // enable the indicator
                activityindicator.IsEnabled = true;
                activityindicator.IsRunning = true;
                activityindicator.IsVisible = true;
            });

            MessagingCenter.Subscribe<BusinesscardsViewModel>(this, "Done", (sender) =>
            {
                // enable the buttons and collectionview
                manualButton.IsEnabled = true;
                galeryButton.IsEnabled = true;
                cameraButton.IsEnabled = true;
                collectionView.SelectionMode = SelectionMode.Single;

                // disable the indicator
                activityindicator.IsEnabled = false;
                activityindicator.IsRunning = false;
                activityindicator.IsVisible = false;
            });
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            viewModel.OnAppearingTest();
        }

        // Eventhandler for the selection of card in the collectionview
        private void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // if an item was selected (and put in the SelectedItem property) then pass the call through to the viewmodel and deselect the item.
            if (((CollectionView)sender).SelectedItem != null && e.CurrentSelection.FirstOrDefault() != null)
            {
                Debug.WriteLine("MainPage - a card was selected: " + e.CurrentSelection.FirstOrDefault());
                viewModel.SelectionChanged();
                ((CollectionView)sender).SelectedItem = null;
            }
        }

        private async void FirstTimeRunned()
        {
            string text = "\nYou have three buttons: \n - Left button: manual input.\n - Middle button: scan card from gallery. \n - Right button: scan card with camera." +
                            "\n\nFor more information, click on the hamburger menu at the top left corner.";

            await DisplayAlert("Welcome!", text, "OK");
        } 
    }
}


