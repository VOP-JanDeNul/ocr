using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Businesscards.Services.Camera
{
    public interface ICameraService
    {
        // Simple interface for cleanliness of the code, this defines the task of the camera service
        // Makes it easy to later define extra functionality
        Task GaleryPhotoAsync();
        Task TakePhotoAsync();
        Task LoadPhotoAsync(FileResult photo);
    }
}
