using Businesscards.Models;
using Businesscards.Services.Camera;
using Businesscards.Services.ImageTransformation;
using Businesscards.Services.OCR;
using Businesscards.Services.Rest;
using Businesscards.Services.Toast;
using Businesscards.Views;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Businesscards.ViewModels
{
    public class BusinesscardsViewModel : BaseViewModel
    {
        private ICameraService cameraService;
        private IRestService restService;
        private AOcrService ocrService;
        private IImageTransformationService imageService;
        private NetworkAccess network;
        private User user;
        private Businesscard _selectedCard;

        public ObservableCollection<Businesscard> Businesscards { get; set; } = new ObservableCollection<Businesscard>();

        // This attribute is changed by the collectionview if a card was selected.
        public Businesscard SelectedCard
        {
            get
            {
                return _selectedCard;
            }
            set
            {
                if (_selectedCard != value)
                {
                    _selectedCard = value;
                }
            }
        }

        public INavigation Navigation { get; set; }

        // Initialize ViewModel, navigation and commands
        public BusinesscardsViewModel(INavigation navigation)
        {
            Navigation = navigation;
            cameraService = new CameraService();
            restService = new RestService();
            imageService = new ImageTransformationService();
            user = User.InstanceUser;

            // If you want to change which OCR Service you want to use: this is the only place you need to change something (except for Tesseract -> network code is different)
            ocrService = new AzureFormRecognizer();
            //ocrService = new AzureVisionOCR();
            //ocrService = new TesseractOCR();

            LoadBusinessCards = new Command(async () => await onLoadBusinessCards());
            AddBusinessCardManually = new Command(async () => await onAddManually());
            AddBusinessCardGallery = new Command(async () => await onAddGallery());
            AddBusinessCardCamera = new Command(async () => await onAddCamera());
        }

        private async Task GetBusinessCards()
        {
            Businesscards.Clear();
            var businesscards = await App.Database.GetBusinesscardsAsync();
            var sortedcards = businesscards.OrderByDescending(i => i.Date);
            foreach (var card in sortedcards)
            {
                //Uncomment this code if the endpoint is set up
                try
                {
                    if (card.Extra != "UNSCANNEDCARD")
                    {
                        // Try to send the card to the endpoint
                        await restService.SendCardsAsync(card);
                        // if succesfull delete is from the database
                        await App.Database.DeleteBusinesscardAsync(card);
                        DependencyService.Get<IToast>().ShortAlert("Card was sent succesfully.");
                    }
                    else
                    {
                        Businesscards.Add(card);
                    }
                }
                catch (RestException ex)
                {
                    // if not succesfull add to the cards list
                    Businesscards.Add(card);
                    DependencyService.Get<IToast>().LongAlert("Something went wrong sending the card.\nPlease check your internet connection and try again.");
                    Debug.WriteLine(ex.ToString());
                }
            }
        }

        public async void OnAppearingTest()
        {
            await onLoadBusinessCards();
        }

        // Command for loading businesscards from local sqlite database
        public ICommand LoadBusinessCards { get; }

        private async Task onLoadBusinessCards()
        {
            IsBusy = true;
            try
            {
                Businesscards.Clear();
                await GetBusinessCards();
            }
            catch (Exception)
            {
                Debug.WriteLine("BusinessCardsViewModel - Failed to load businesscards");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Command for adding a businesscard manually
        public ICommand AddBusinessCardManually { get; }

        private async Task onAddManually()
        {
            try
            {
                if (user.OriginUser == null)
                {
                    MessagingCenter.Send(this, "emptyOrigin");
                }
                else
                {
                    var businesscardEntryPage = new BusinesscardEntryPage();
                    await Navigation.PushAsync(businesscardEntryPage);
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("BusinessCardsViewModel - Failed to add card");
            }
        }

        // Command for adding a businesscard by selecting a photo from gallery
        public ICommand AddBusinessCardGallery { get; }

        private async Task onAddGallery()
        {
            try
            {
                if (user.OriginUser == null)
                {
                    MessagingCenter.Send(this, "emptyOrigin");
                }
                else
                {
                    await cameraService.GaleryPhotoAsync();
                    await Ok_Button_Clicked();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("BusinessCardsViewModel - Error in onAddGallery" + ex.ToString());
            }
        }

        // Command for adding a businesscard by taking a photo with native camera app
        public ICommand AddBusinessCardCamera { get; }

        private async Task onAddCamera()
        {
            try
            {

                if (user.OriginUser == null)
                {
                    MessagingCenter.Send(this, "emptyOrigin");
                }
                else
                {
                    await cameraService.TakePhotoAsync();
                    await Ok_Button_Clicked();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("BusinessCardsViewModel - Error in onAddCamera" + ex.ToString());
            }
        }

        // Method called to pass selected/taken photo to businesscardEntryPage view
        async Task Ok_Button_Clicked()
        {
            // If there is no connectivity store the card locally and then show it in the overview page
            network = Connectivity.NetworkAccess;
            if (network != NetworkAccess.Internet)
            {
                //Store locally
                await StoreLocally();
            }
            else
            {
                //Send to OCR
                string path = CameraService.PhotoPath.ToString();
                await SendToOCR(path);
            }
        }

        // Function to store the image locally as an unscanned card for later use
        private async Task StoreLocally()
        {
            // Create a new card and fill in the information 
            Businesscard card = new Businesscard();
            string filename = CameraService.PhotoPath.ToString();
            card.Base64 = imageService.ConvertImageToBase64(filename);
            card.Date = DateTime.Now;
            card.Company = "Tap here to scan the card";
            card.Name = "Unscanned card";
            card.Extra = "UNSCANNEDCARD"; // Note: this value is very important since it is used to identify which card is unscanned and which is not (DO NOT CHANGE)

            // save the card locally
            await App.Database.SaveBusinesscardAsync(card);

            // refresh the page to show it on the mainpage
            OnAppearingTest();
            Debug.WriteLine("Stored the card locally because the OCR Service isn't available");
            DependencyService.Get<IToast>().LongAlert("Stored the card.\nTry to rescan when connected to the internet.");
        }

        public async Task SendToOCR(string path)
        {
            // id is overwritten by the database if it is 0
            await SendToOCR(path, 0);
        }

        // Method which analyses the photo using the ocr service and then redirecting to the entrypage
        public async Task SendToOCR(string path, int id)
        {
            try
            {
                network = Connectivity.NetworkAccess;
                if (network == NetworkAccess.Internet)
                {
                    // Send a message to the view to start the loading indicator and to disable the buttons
                    MessagingCenter.Send(this, "Sending");
                    DependencyService.Get<IToast>().LongAlert("Analyzing businesscard.\nThis may take a while.");

                    //Reset the OCR Service
                    ocrService.resetOCR();

                    // Define a new card before trying to analyze the image
                    Businesscard card;

                    // We want the analyzing to stop if it doesn't complete within a certain timeframe
                    int timeout = 20000;
                    Task<Businesscard> getCardTask = ocrService.getCard(path);
                    using (var timeoutCancellationTokenSource = new CancellationTokenSource())
                    {
                        // Check which task finishes first: the delay or the scanning
                        if (await Task.WhenAny(getCardTask, Task.Delay(timeout, timeoutCancellationTokenSource.Token)) == getCardTask)
                        {
                            // Task completed within timeout.
                            // Consider that the task may have faulted or been canceled.
                            // We re-await the task so that any exceptions/cancellation is rethrown.
                            card = await getCardTask;
                        }
                        else
                        {
                            // Task didn't complete within timeout.
                            // Throw timeexception to show relevant toast
                            throw new TimeoutException("OCR took to long.");
                        }
                    }

                    // Save the base64 version of the image in the object to later send to the endpoint
                    card.Base64 = imageService.ConvertImageToBase64(path);
                    card.Id = id;

                    var businesscardEntryPage = new BusinesscardEntryPage(card);
                    await Navigation.PushAsync(businesscardEntryPage);

                    MessagingCenter.Send(this, "Done");
                }
                else
                {
                    Debug.WriteLine("Won't try to scan this image since there is no internet connection");
                    DependencyService.Get<IToast>().LongAlert("Can't scan the businesscard.\nThere is no internet connection.");
                }
            }
            catch (BadRequestException ex)
            {
                Debug.WriteLine("BADREQUESTEXCEPTION for OCR: " + ex.ToString());
                // Functionality to display a pop up - sending a message to the view that a bad request was sent
                MessagingCenter.Send(this, "BadRequest");
                MessagingCenter.Send(this, "Done");
            }
            catch (FormRecognizerException ex)
            {
                Debug.WriteLine("FORMRECOGNIZEREXCEPTION for OCR: " + ex.ToString());
                // Functionality to display a pop up - sending a message to the view that a bad request was sent
                MessagingCenter.Send(this, "GeneralError");
                MessagingCenter.Send(this, "Done");
            }
            catch (TimeoutException ex)
            {
                Debug.WriteLine("TIMEOUTEXCEPTION for OCR: " + ex.ToString());
                MessagingCenter.Send(this, "TimeoutException");
                MessagingCenter.Send(this, "Done");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GENERALEXCEPTION for OCR: " + ex.ToString());
                // Functionality to display a pop up - sending a message to the view that a bad request was sent
                MessagingCenter.Send(this, "GeneralError");
                MessagingCenter.Send(this, "Done");
            }
        }

        //async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        async public void SelectionChanged()
        {
            if (SelectedCard.Extra == "UNSCANNEDCARD")
            {
                Debug.WriteLine("The selected card is an unscanned card, which is stored locally");
                // Now the user has selected a card, which hasn't been scanned yet, so we save the base64 image locally and pass it on to the ocrservice
                string path = imageService.GetImagePath(SelectedCard.Base64);
                await SendToOCR(path, SelectedCard.Id);
            }
        }
    }
}
