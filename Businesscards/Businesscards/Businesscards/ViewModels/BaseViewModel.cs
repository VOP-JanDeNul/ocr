using System.Threading.Tasks;

namespace Businesscards.ViewModels
{
    public abstract class BaseViewModel : ExtendedBindableObject
    {

        private bool _isBusy;

        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }

            set
            {
                _isBusy = value;
                RaisePropertyChanged(() => IsBusy);
            }
        }

        public BaseViewModel()
        {
        }

        public virtual Task InitializeAsync(object navigationData)
        {
            return Task.FromResult(false);
        }
    }
}
