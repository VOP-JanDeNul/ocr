using Businesscards.Models;
using Businesscards.Services.Camera;
using Businesscards.Services.ImageTransformation;
using Businesscards.Services.OCR;
using Businesscards.Services.Rest;
using Businesscards.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Businesscards.ViewModels
{
    public class BusinesscardsViewModel : BaseViewModel
    {
        private ICameraService cameraService;
        private IRestService restService;
        private AOcrService ocrService;
        private IImageTransformationService imageService;
        private User user;

        public ObservableCollection<Businesscard> Businesscards { get; set; } = new ObservableCollection<Businesscard>();
        public INavigation Navigation { get; set; }

        // Initialize ViewModel, navigation and commands
        public BusinesscardsViewModel(INavigation navigation)
        {
            Navigation = navigation;
            this.cameraService = new CameraService();
            this.restService = new RestService();
            this.imageService = new ImageTransformationService();
            this.user = User.InstanceUser;

            //only change OCR here
            this.ocrService = new AzureFormRecognizer();
            //this.ocrService = new AzureVisionOCR();
            //this.ocrService = new TesseractOCR();

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
                    // Try to send the card to the endpoint
                    await restService.SendCardsAsync(card);
                    // if succesfull delete is from the database
                    await App.Database.DeleteBusinesscardAsync(card);
                }
                catch (RestException)
                {
                    // if not succesfull add to the cards list
                    Businesscards.Add(card);
                }
                //Businesscards.Add(card);
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
                Console.WriteLine("Failed to load businesscards");
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
                Console.WriteLine("Failed to add card");
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
                Console.WriteLine("Error in onAddGallery" + ex.ToString());
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
                Console.WriteLine("Error in onAddCamera" + ex.ToString());
            }
        }

        // Method called to pass selected/taken photo to businesscardEntryPage view
        async Task Ok_Button_Clicked()
        {
            ocrService.resetOCR();
            string filename = CameraService.PhotoPath.ToString();
            Console.WriteLine("Ok-button clicked: sending to ocrService");
            // ocrService is called to get the information of the text
            try
            {
                // Send a message to the view to start the loading indicator and to disable the buttons
                MessagingCenter.Send(this, "Sending");
                Businesscard card = await ocrService.getCard(filename);
                // Send a message to the view to stop the loading indicator and to enable the buttons
                MessagingCenter.Send(this, "Done");

                // Save the base64 version of the image in the object to later send to the endpoint
                card.Base64 = imageService.ConvertImageToBase64(filename);
                var businesscardEntryPage = new BusinesscardEntryPage(card);
                await Navigation.PushAsync(businesscardEntryPage);
            }
            catch (BadRequestException)
            {
                Console.WriteLine("Received a bad request from the OCR Service");
                // Functionality to display a pop up - sending a message to the view that a bad request was sent
                MessagingCenter.Send(this, "Done");
                MessagingCenter.Send(this, "BadRequest");
            }
        }
    }
}
