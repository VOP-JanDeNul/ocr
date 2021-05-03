using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Businesscards.Services.Camera
{
    // class for using native camera app 
    public class CameraService : ICameraService
    {
        public CameraService()
        {

        }

        public static object PhotoPath { get; private set; }

        // Select an image from the galery
        public async Task GaleryPhotoAsync()
        {
            try
            {
                PhotoPath = null;
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
                throw new CameraException(ex.ToString());
            }
        }

        // Take a picture with the camera
        public async Task TakePhotoAsync()
        {
            PhotoPath = null;
            if (MediaPicker.IsCaptureSupported)
            {
                Debug.WriteLine("Capture is supported");
            }
            else
            {
                Debug.WriteLine("Capture is not supported!");
            }
            try
            {
                var photo = await MediaPicker.CapturePhotoAsync();
                await LoadPhotoAsync(photo);
                Debug.WriteLine($"CapturePhotoAsync COMPLETED: {PhotoPath}");
            }
            catch (Exception ex)
            {
                throw new CameraException(ex.ToString());
            }
        }

        // Load the image as a stream
        public async Task LoadPhotoAsync(FileResult photo)
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
            catch (Exception ex)
            {
                throw new CameraException(ex.ToString());
            }
        }
    }
}
