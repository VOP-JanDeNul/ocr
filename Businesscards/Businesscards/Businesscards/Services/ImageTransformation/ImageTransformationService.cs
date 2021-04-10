using System;
using System.IO;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Businesscards.Services.ImageTransformation
{
    public class ImageTransformationService : IImageTransformationService
    {
        public ImageTransformationService()
        {

        }

        // Help function used to convert an image to base64 notation
        public string ConvertImageToBase64(string path)
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(path);
                string base64String = Convert.ToBase64String(bytes);
                return base64String;
            }
            catch(Exception ex)
            {
                throw new ImageTransformationException(ex.ToString());
            }
        }

        // Help function used to save image locally from base64 format - returns path
        public string GetImagePath(string base64string)
        {
            try
            {
                if (string.IsNullOrEmpty(base64string)){
                    return "";
                }
                byte[] fileBytes = Convert.FromBase64String(base64string);
                string localPath = Path.Combine(FileSystem.CacheDirectory, "image");
                ImageSource image = ImageSource.FromStream(() => new MemoryStream(fileBytes));
                File.WriteAllBytes(localPath, fileBytes);
                return localPath;
            }
            catch (Exception ex)
            {
                throw new ImageTransformationException(ex.ToString());
            }
        }
    }
}
