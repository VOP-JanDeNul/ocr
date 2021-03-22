using Businesscards.Models;
using Businesscards.OCR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Businesscards.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BusinesscardsPage : ContentPage
    {
        public object PhotoPath { get; private set; }

        public BusinesscardsPage()
        {
            InitializeComponent();
            PrepareFiles();
        }

        // Reading the data and placing it in internal storage
        // Read it using the xamarin essentials openAppPackageFileAsync method
        async void PrepareFiles()
        {
            Console.WriteLine("CARDSPAGE: preparing files");
            // create the tessdata directory 
            string dataPath = Path.Combine(FileSystem.CacheDirectory, "tessdata");
            Directory.CreateDirectory(dataPath);
            using (var stream = await FileSystem.OpenAppPackageFileAsync("eng.traineddata"))
            {
                using (var reader = new StreamReader(stream))
                {
                    string fileContents = await reader.ReadToEndAsync();
                    string localPath = Path.Combine(FileSystem.CacheDirectory,"tessdata","eng.traineddata");
                    File.WriteAllText(localPath, fileContents);
                }
            }
            using (var stream = await FileSystem.OpenAppPackageFileAsync("pdf.ttf"))
            {
                using (var reader = new StreamReader(stream))
                {
                    string fileContents = await reader.ReadToEndAsync();
                    string localPath = Path.Combine(FileSystem.CacheDirectory,"tessdata","pdf.ttf");
                    File.WriteAllText(localPath, fileContents);
                }
            }
            Console.WriteLine("CARDSPAGE: files prepared");
        }

        async void OnAddClicked(object sender, EventArgs e)
        {
            try
            {
                var card = new Businesscard();

                var businesscardEntryPage = new BusinesscardEntryPage();
                businesscardEntryPage.BindingContext = card;
                await Navigation.PushModalAsync(businesscardEntryPage);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in onAddClicked" + ex.ToString());
            }
        }

        async void OnGaleryClicked(object sender, EventArgs e)
        {
            try
            {
                await GaleryPhotoAsync();
                await Ok_Button_Clicked();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in onAddClicked" + ex.ToString());
            }
        }

        async void OnTakePictureClicked(object sender, EventArgs e)
        {
            try
            {
                await TakePhotoAsync();
                await Ok_Button_Clicked();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in onTakePictureClicked" + ex.ToString());
            }
        }


        async Task GaleryPhotoAsync()
        {
            try
            {
                var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Please pick a photo"
                });

                if (result != null)
                {
                    //var stream = await result.OpenReadAsync();
                    await LoadPhotoAsync(result);
                    // resultImage.Source = ImageSource.FromStream(() => stream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GaleryPhotoAsync" + ex.ToString());
            }
        }

        async Task LoadPhotoAsync(FileResult photo)
        {
            try
            {
                if (photo == null)
                {
                    PhotoPath = null;
                    return;
                }

                // save the file into local storage
                var newFile = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
                using (var stream = await photo.OpenReadAsync())
                using (var newStream = File.OpenWrite(newFile))
                    await stream.CopyToAsync(newStream);

                var streamImage = await photo.OpenReadAsync();
                // resultImage.Source = ImageSource.FromStream(() => streamImage);

                PhotoPath = newFile;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in LoadPhotoAsync" + e.ToString());
            }
        }

        async Task Ok_Button_Clicked()
        {
            var card = new Businesscard();
            card.FileName = PhotoPath.ToString();

            Dictionary<string, List<string>> results = BusinessCardOCR.getText(card.FileName);
            var businesscardEntryPage = new BusinesscardEntryPage(results);
            businesscardEntryPage.BindingContext = card;
            await Navigation.PushModalAsync(businesscardEntryPage);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Retrieve all the businesscards from the database, 
            // and set them as the data source for the CollectionView.
            collectionView.ItemsSource = await App.Database.GetBusinesscardsAsync();
        }

        async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection != null)
            {
                // Navigate to the BusinesscardEntryPage, passing the Id  as a query parameter.
                Businesscard card = (Businesscard)e.CurrentSelection.FirstOrDefault();
                //await Shell.Current.GoToAsync($"{nameof(BusinesscardEntryPage)}?{nameof(BusinesscardEntryPage.ItemId)}={card.Id.ToString()}");
                var businesscardEntryPage = new BusinesscardEntryPage();
                businesscardEntryPage.BindingContext = card;
                await Navigation.PushModalAsync(businesscardEntryPage);
            }
        }



        async Task TakePhotoAsync()
        {
            if (MediaPicker.IsCaptureSupported)
            {
                Console.WriteLine("Capture is supported");
            }
            else
            {
                Console.WriteLine("Capture is not supported!");
            }
            try
            {
                var photo = await MediaPicker.CapturePhotoAsync();
                await LoadPhotoAsync(photo);
                Console.WriteLine($"CapturePhotoAsync COMPLETED: {PhotoPath}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"CapturePhotoAsync THREW: {e.Message}");
            }
        }


    }
}


