using Businesscards.Models;

namespace Businesscards.Services.ImageTransformation
{
    public interface IImageTransformationService
    {
        // Simple interface for cleanliness of the code, this defines the task of the image transformation service
        // Makes it easy to later define extra functionality such as compression
        string ConvertImageToBase64(string path);
        string GetImagePath(string base64string);

        void PrintCard(Businesscard card);
    }
}
