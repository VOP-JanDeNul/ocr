using Businesscards.Models;
using Businesscards.Services.ImageTransformation;
using Businesscards.Services.Mail;
using Businesscards.Services.Rest;
using Businesscards.Services.Toast;
using Businesscards.Validations;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Businesscards.ViewModels
{
    public class BusinesscardEntryViewModel : BaseViewModel
    {
        private ValidatableObject<string> _company;
        private ValidatableObject<string> _name;
        private string _nature;
        private string _jobtitle;
        private ValidatableObject<string> _phone;
        private ValidatableObject<string> _mobile;
        private ValidatableObject<string> _email;
        private ValidatableObject<string> _fax;
        private string _street;
        private string _city;
        private string _filename;
        private string _extra;
        private string _base64;
        private string _origin;
        private bool _isMailChecked;
        private bool _isValid;
        private ValidatableObject<bool> _isVisible;
        private ValidatableObject<int> _heightRequestLabel;
        private Businesscard _card;

        private IRestService restService;
        private IImageTransformationService imageService;
        private IMailService mailService;

        public ValidatableObject<string> Company
        {
            get { return _company; }
            set
            {
                _company = value;
                _card.Company = _company.Value;
                RaisePropertyChanged(() => Company);
            }
        }

        public ValidatableObject<string> Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged(() => Name);
            }
        }

        public string Nature
        {
            get { return _nature; }
            set
            {
                _nature = value;
                Card.Nature = value;
                RaisePropertyChanged(() => Nature);
            }
        }

        public string JobTitle
        {
            get { return _jobtitle; }
            set
            {
                _jobtitle = value;
                Card.Jobtitle = value;
                RaisePropertyChanged(() => JobTitle);
            }
        }
        public ValidatableObject<string> Phone
        {
            get { return _phone; }
            set
            {
                _phone = value;
                RaisePropertyChanged(() => Phone);
            }
        }
        public ValidatableObject<string> Mobile
        {
            get { return _mobile; }
            set
            {
                _mobile = value;
                RaisePropertyChanged(() => Mobile);
            }
        }
        public ValidatableObject<string> Email
        {
            get { return _email; }
            set
            {
                _email = value;
                RaisePropertyChanged(() => Email);
            }
        }
        public ValidatableObject<string> Fax
        {
            get { return _fax; }
            set
            {
                _fax = value;
                RaisePropertyChanged(() => Fax);
            }
        }
        public string Street
        {
            get { return _street; }
            set
            {
                _street = value;
                Card.Street = value;
                RaisePropertyChanged(() => Street);
            }
        }

        public string City
        {
            get { return _city; }
            set
            {
                _city = value;
                Card.City = value;
                RaisePropertyChanged(() => City);
            }
        }

        public string Filename
        {
            get { return _filename; }
            set
            {
                _filename = value;
                RaisePropertyChanged(() => Filename);
            }
        }

        public string Extra
        {
            get { return _extra; }
            set
            {
                _extra = value;
                Card.Extra = value;
                RaisePropertyChanged(() => Extra);
            }
        }
        public string Base64
        {
            get { return _base64; }
            set
            {
                _base64 = value;
                Card.Base64 = value;
                RaisePropertyChanged(() => Base64);
            }
        }

        public string Origin
        {
            get { return _origin; }
            set
            {
                _origin = value;
                Card.Origin = value;
                RaisePropertyChanged(() => Origin);
            }
        }


        public bool IsMailChecked
        {
            get { return _isMailChecked; }
            set
            {
                _isMailChecked = value;
                RaisePropertyChanged(() => IsMailChecked);
            }
        }

        public bool IsValid
        {
            get
            {
                return _isValid;
            }
            set
            {
                _isValid = value;
                RaisePropertyChanged(() => IsValid);
            }
        }


        public ValidatableObject<bool> IsVisible
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value;
                RaisePropertyChanged(() => IsVisible);
            }
        }

        public ValidatableObject<int> HeightRequestLabel
        {
            get { return _heightRequestLabel; }
            set
            {
                _heightRequestLabel = value;
                RaisePropertyChanged(() => HeightRequestLabel);
            }
        }


        public Businesscard Card
        {
            get { return _card; }
            set
            {
                _card = value;
                Company.Value = value.Company;
                Name.Value = value.Name;
                Nature = value.Nature;
                JobTitle = value.Jobtitle;
                Phone.Value = value.Phone;
                Mobile.Value = value.Mobile;
                Email.Value = value.Email;
                Fax.Value = value.Fax;
                Street = value.Street;
                City = value.City;
                Extra = value.Extra;
                Base64 = value.Base64;
                RaisePropertyChanged(() => Card);
            }
        }

        public INavigation Navigation { get; set; }

        // Initialize ViewModel, navigation and commands
        public BusinesscardEntryViewModel(INavigation navigation)
        {
            Navigation = navigation;
            restService = new RestService();
            imageService = new ImageTransformationService();
            mailService = new MailService();

            _company = new ValidatableObject<string>();
            _name = new ValidatableObject<string>();
            _email = new ValidatableObject<string>();
            _phone = new ValidatableObject<string>();
            _mobile = new ValidatableObject<string>();
            _fax = new ValidatableObject<string>();

            Card = new Businesscard();

            SaveBusinessCard = new Command(async () => await OnSave());
            DeleteBusinessCard = new Command(async () => await OnDelete());

            AddValidations();
        }

        // Initialize ViewModel, navigation and commands using an existing businesscard
        public BusinesscardEntryViewModel(INavigation navigation, Businesscard card)
        {
            Navigation = navigation;
            restService = new RestService();
            imageService = new ImageTransformationService();
            mailService = new MailService();

            _company = new ValidatableObject<string>();
            _name = new ValidatableObject<string>();
            _email = new ValidatableObject<string>();
            _phone = new ValidatableObject<string>();
            _mobile = new ValidatableObject<string>();
            _fax = new ValidatableObject<string>();

            Card = card;
            _filename = imageService.GetImagePath(Card.Base64);

            SaveBusinessCard = new Command(async () => await OnSave());
            DeleteBusinessCard = new Command(async () => await OnDelete());

            AddValidations();
        }

        // Command for saving a businesscard to local sqlite database
        public ICommand SaveBusinessCard { get; }


        User user = User.InstanceUser;

        private async Task OnSave()
        {
            IsBusy = true;
            IsValid = true;
            bool isValid = Validate();

            if (isValid)
            {
                try
                {
                    Card.Company = _company.Value;
                    Card.Name = _name.Value;
                    Card.Email = _email.Value;
                    Card.Phone = _phone.Value;
                    Card.Mobile = _mobile.Value;
                    Card.Fax = _fax.Value;
                    Card.Date = DateTime.Now.ToLocalTime();
                    Card.Origin = user.OriginUser;

                    //Uncomment this when testing endpoint(endpoint code)
                    try
                    {
                        //DependencyService.Get<IToast>().ShortAlert("Card was sent succesfully.");
                        MessagingCenter.Send(this, "endpoint");
                        // try to send the card to the endpoint
                        imageService.PrintCard(Card);
                        await restService.SendCardsAsync(Card);

                        //DependencyService.Get<IToast>().ShortAlert("Card was sent succesfully.");


                        // if succesfull delete is from the database
                        await App.Database.DeleteBusinesscardAsync(Card);
                        MessagingCenter.Send(this, "endpoint_done");
                        DependencyService.Get<IToast>().ShortAlert("Card was sent succesfully.");
                    }
                    catch (RestException)
                    {
                        // if not succesfull add to the database
                        await App.Database.SaveBusinesscardAsync(_card);
                        DependencyService.Get<IToast>().LongAlert("Something went wrong sending the card.\nPlease check your internet connection and try again.");
                    }

                    await Task.Delay(2000);
                    await Navigation.PopAsync();

                    if (IsMailChecked)
                    {
                        await mailService.ComposeEmailAsync(_card);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("OnSave error" + ex.ToString());
                }
            }
            else
            {
                IsValid = false;
            }
            IsBusy = false;
        }

        // Command for deleting a businesscard stored in local sqlite database
        public ICommand DeleteBusinessCard { get; }
        private async Task OnDelete()
        {
            try
            {
                await App.Database.DeleteBusinesscardAsync(_card);
                await Navigation.PopAsync();

            }
            catch (Exception)
            {
                Debug.WriteLine("Failed to delete card");
            }
        }

        // code for validation of company, name and email
        public ICommand ValidateCompanyCommand => new Command(() => ValidateCompany());
        public ICommand ValidateNameCommand => new Command(() => ValidateName());
        public ICommand ValidateEmailCommand => new Command(() => ValidateEmail());
        public ICommand ValidatePhoneCommand => new Command(() => ValidatePhone());
        public ICommand ValidateMobileCommand => new Command(() => ValidateMobile());
        public ICommand ValidateFaxCommand => new Command(() => ValidateFax());
        private bool Validate()
        {
            bool isValidCompany = ValidateCompany();
            bool isValidName = ValidateName();
            bool isValidEmail = ValidateEmail();
            bool isValidPhone = ValidatePhone();
            bool isValidMobile = ValidateMobile();
            bool isValidFax = ValidateFax();

            return isValidCompany && isValidName && isValidEmail && isValidPhone && isValidMobile && isValidFax;
        }

        private bool ValidateCompany()
        {
            return _company.Validate();
        }
        private bool ValidateName()
        {
            return _name.Validate();
        }
        private bool ValidateEmail()
        {
            return _email.Validate();
        }
        private bool ValidatePhone()
        {
            return _phone.Validate();
        }
        private bool ValidateMobile()
        {
            return _mobile.Validate();
        }
        private bool ValidateFax()
        {
            return _fax.Validate();
        }
        private void AddValidations()
        {
            _company.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "Company is required." });
            _name.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "Name is required." });
            _email.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "Email address is required." });
            _email.Validations.Add(new IsValidEmailRule<string> { ValidationMessage = "Not a valid email address." });
            _phone.Validations.Add(new IsValidPhoneRule<string> { ValidationMessage = "Not a valid phone number." });
            _mobile.Validations.Add(new IsValidPhoneRule<string> { ValidationMessage = "Not a valid mobile number." });
            _fax.Validations.Add(new IsValidPhoneRule<string> { ValidationMessage = "Not a valid fax number." });
        }
    }
}
