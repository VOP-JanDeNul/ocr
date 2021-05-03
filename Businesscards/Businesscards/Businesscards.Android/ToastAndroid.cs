using Android.App;
using Android.Widget;
using Businesscards.Droid;
using Businesscards.Services.Toast;

[assembly: Xamarin.Forms.Dependency(typeof(ToastAndroid))]
namespace Businesscards.Droid
{
    public class ToastAndroid : IToast
    {
        public void LongAlert(string message)
        {
            Toast.MakeText(Application.Context, message, ToastLength.Long).Show();
        }

        public void ShortAlert(string message)
        {
            Toast.MakeText(Application.Context, message, ToastLength.Short).Show();
        }
    }
}