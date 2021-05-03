using Businesscards.Models;
using Businesscards.Services.Toast;
using Businesscards.ViewModels;
using Businesscards.Views.Controls;
using System;
using Xamarin.Forms;

namespace Businesscards.Views
{
    //[QueryProperty(nameof(ItemId), nameof(ItemId))]
    public partial class BusinesscardEntryPage : ContentPage
    {
        //private Dictionary<string, List<string>> ocrResults;

        private int swapper = 0;

        // Initialize page and set binding context to BusinesscardsEntryViewModel
        public BusinesscardEntryPage()
        {
            InitializeComponent();
            BindingContext = new BusinesscardEntryViewModel(Navigation);


            MessagingCenter.Subscribe<BusinesscardEntryViewModel>(this, "endpoint", (sender) =>
            {
                save.IsEnabled = false;
                delete.IsEnabled = false;

                // enable the indicator
                activityindicator2.IsEnabled = true;
                activityindicator2.IsRunning = true;
                activityindicator2.IsVisible = true;
            });

            MessagingCenter.Subscribe<BusinesscardEntryViewModel>(this, "endpoint_done", (sender) =>
            {
                save.IsEnabled = true;
                delete.IsEnabled = true;

                // disable the indicator
                activityindicator2.IsEnabled = false;
                activityindicator2.IsRunning = false;
                activityindicator2.IsVisible = false;
            });
        }

        // Initialize page and set binding context to BusinesscardsEntryViewModel using an existing businesscard
        public BusinesscardEntryPage(Businesscard card)
        {
            InitializeComponent();
            BindingContext = new BusinesscardEntryViewModel(Navigation, card);

            MessagingCenter.Subscribe<BusinesscardEntryViewModel>(this, "endpoint", (sender) =>
            {
                save.IsEnabled = false;
                delete.IsEnabled = false;
            });

            MessagingCenter.Subscribe<BusinesscardEntryViewModel>(this, "endpoint_done", (sender) =>
            {
                save.IsEnabled = true;
                delete.IsEnabled = true;
            });
        }

        //async void LoadBusinesscard(string itemId)
        //{
        //    try
        //    {
        //        int id = Convert.ToInt32(itemId);
        //        // Retrieve the businesscard and set it as the BindingContext of the page.
        //        Businesscard card = await App.Database.GetBusinesscardAsync(id);
        //        BindingContext = card;
        //    }
        //    catch (Exception)
        //    {
        //        Console.WriteLine("Failed to load Businesscard.");
        //    }
        //}



        // Code for swap functionality
        string namefirst, namesecond;
        Button firstButton, secondButton;

        private void save_Clicked(object sender, EventArgs e)
        {
            if (companyValidation.IsVisible || nameValidation.IsVisible || emailValidation.IsVisible)
            {
                scrollView.ScrollToAsync(0, 0, true);
            }
        }


        // Always needed 2 different buttons to swap the content of the Entry fields between them.
        // If 2 times the same button is clicked, nothing changed.
        private void OnSwapClicked(object sender, EventArgs e)
        {

            if (swapper == 0)       // First button is clicked
            {
                firstButton = (Button)sender;
                string idfirst = firstButton.ClassId;
                namefirst = idfirst.Substring(0, idfirst.Length - 4);

                firstButton.BackgroundColor = Color.FromHex("2D2A29");      // black JDN
                swapper++;
            }
            else        // Second button is clicked
            {
                secondButton = (Button)sender;
                string idsecond = secondButton.ClassId;
                namesecond = idsecond.Substring(0, idsecond.Length - 4);

                swap(namefirst, namesecond);

                firstButton.BackgroundColor = Color.FromHex("BA0C2F");      // normal JDN color
                swapper = 0;
            }

        }

        // Code for swap functionality
        private void swap(string first, string second)
        {
            // Gets the necessary controls 
            FloatingLabelInput fliFirst = (FloatingLabelInput)FindByName(first + "Entry");
            FloatingLabelInput fliSecond = (FloatingLabelInput)FindByName(second + "Entry");

            // Gets the text of the controls
            string a = fliFirst.Text;
            string b = fliSecond.Text;

            // Changes the text => SWAP
            fliFirst.Text = b;
            fliSecond.Text = a;
        }

    }
}

