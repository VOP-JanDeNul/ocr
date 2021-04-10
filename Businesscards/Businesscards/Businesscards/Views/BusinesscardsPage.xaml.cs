using Businesscards.Models;
using Businesscards.ViewModels;
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

            MessagingCenter.Subscribe<BusinesscardsViewModel>(this, "emptyOrigin", async (sender) =>
            {
                await DisplayAlert("Unknown user!", "Before adding a card, go to settings->Origin and fill in your email address", "OK");
            });

            MessagingCenter.Subscribe<BusinesscardsViewModel>(this, "BadRequest", async (sender) =>
            {
                await DisplayAlert("Alert", "The picture wasn't clear enough, consider holding it in the air and a bit further away", "OK");
            });

            MessagingCenter.Subscribe<BusinesscardsViewModel>(this, "Sending", (sender) =>
            {
                // disable the buttons
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
                // enable the buttons
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

        // Called when a businesscard is selected from list
        // !! Verwijderen in sprint 3 !!!
        async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection != null)
            {
                // Navigate to the BusinesscardEntryPage, passing the Id  as a query parameter.
                Businesscard card = (Businesscard)e.CurrentSelection.FirstOrDefault();
                // await Shell.Current.GoToAsync($"{nameof(BusinesscardEntryPage)}?{nameof(BusinesscardEntryPage.ItemId)}={card.Id.ToString()}");
                var businesscardEntryPage = new BusinesscardEntryPage(card);
                // businesscardEntryPage.BindingContext = card;
                await Navigation.PushAsync(businesscardEntryPage);
            }
        }


    }
}


