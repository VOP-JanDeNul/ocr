using Businesscards.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Businesscards.Services.ImageTransformation
{
    public class ImageTransformationService : IImageTransformationService
    {
        // Used for random filenames
        private Random rd;
        public ImageTransformationService()
        {
            rd = new Random();
        }

        // Help function used to convert an image to base64 notation
        public string ConvertImageToBase64(string path)
        {
            Debug.WriteLine("ImageService: Converting image into a base64 string");
            try
            {
                byte[] bytes = File.ReadAllBytes(path);
                string base64String = Convert.ToBase64String(bytes);
                return base64String;
            }
            catch (Exception ex)
            {
                throw new ImageTransformationException(ex.ToString());
            }
        }

        // Help function used to save image locally from base64 format - returns path
        public string GetImagePath(string base64string)
        {
            Debug.WriteLine("ImageService: converting base64 string into an image and saving it in cache");
            try
            {
                if (string.IsNullOrEmpty(base64string))
                {
                    return "";
                }
                byte[] fileBytes = Convert.FromBase64String(base64string);
                // we put the image in the cache and give it the name 'image' with a random number so that multiple images all have a different name
                string localPath = Path.Combine(FileSystem.CacheDirectory, "image"+rd.Next(1000000));
                ImageSource image = ImageSource.FromStream(() => new MemoryStream(fileBytes));
                // Write the file
                File.WriteAllBytes(localPath, fileBytes);
                return localPath;
            }
            catch (Exception ex)
            {
                throw new ImageTransformationException(ex.ToString());
            }
        }

        // Easy function used to print the card
        public void PrintCard(Businesscard card)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Company: " + card.Company);
            sb.AppendLine("Name: " + card.Name);
            sb.AppendLine("Nature: " + card.Nature);
            sb.AppendLine("JobTitle: " + card.Jobtitle);
            sb.AppendLine("Phone: " + card.Phone);
            sb.AppendLine("Mobile: " + card.Mobile);
            sb.AppendLine("Email:" + card.Email);
            sb.AppendLine("Fax: " + card.Fax);
            sb.AppendLine("Street: " + card.Street);
            sb.AppendLine("City: " + card.City);
            sb.AppendLine("Date: " + card.Date);
            sb.AppendLine("Origin: " + card.Origin);
            sb.AppendLine("Extra: " + card.Extra);
            Debug.WriteLine(sb.ToString());
        }
    }
}
