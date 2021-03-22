using Businesscards.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xamarin.Essentials;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using System.Threading.Tasks;

namespace Businesscards.Views
{
    [QueryProperty(nameof(ItemId), nameof(ItemId))]
    public partial class BusinesscardEntryPage : ContentPage
    {
        private Dictionary<string, List<string>> ocrResults;
        private const string DestinationEmail = "jan.denul.vop@gmail.com";
        private const string Subject = "New Business Card";
        private string EmailContent = "This is an automated email from the Jan De Nul Business Card App.\n\nVisualization of the business card:\n" +
            "Name:\t\t{0} {1}\n" +
            "Job tilte:\t\t{2}\n" +
            "Organization:\t\t{3}\n" +
            "Nature:\t\t{4}\n\n" +
            "Mobile number:\t{5}\n" +
            "Phone number:\t{6}\n" +
            "Email adress:\t{7}\n" +
            "Fax:\t{8}\n" +
            "Adress:\t{9}\n\n" +
            "Creation date:\t{10}\nThe business card is also represented in BusinessCard.vcf";

        private int swapper = 0;

        public string ItemId
        {
            set
            {
                LoadBusinesscard(value);
            }
        }
        public BusinesscardEntryPage(Dictionary<string, List<string>> results)
        {
            InitializeComponent();
            this.ocrResults = results;
        }

        public BusinesscardEntryPage()
        {
            InitializeComponent();
        }

        async void LoadBusinesscard(string itemId)
        {
            try
            {
                int id = Convert.ToInt32(itemId);
                // Retrieve the businesscard and set it as the BindingContext of the page.
                Businesscard card = await App.Database.GetBusinesscardAsync(id);
                BindingContext = card;
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to load Businesscard.");
            }
        }


        async void OnSaveButtonClicked(object sender, EventArgs e)
        {
            try
            {
                var card = (Businesscard)BindingContext;
                card.Date = DateTime.Now.ToLocalTime();

                if (!string.IsNullOrWhiteSpace(card.Name))      // enkel naam???
                {
                    await App.Database.SaveBusinesscardAsync(card);
                }

                if (checkBox.IsChecked)
                {
                    string FirstName = "";
                    string LastName = card.Name;
                    string Company = card.Company;
                    string JobTitle = card.Jobtitle;
                    string Nature = card.Nature;
                    string MobileNumber = card.Mobile;
                    string PhoneNumber = card.Phone;
                    string EmailAdress = card.Email;
                    string Fax = card.Fax;
                    string Adress = card.Address;
                    string CreationDate = card.Date.ToString();
                    string data = card.FileName; // this is base64 of imageiVBORw0KGgoAAAANSUhEUgAAAcIAAAENC...................


                    EmailContent = string.Format(EmailContent, FirstName, LastName, JobTitle, Company, Nature, MobileNumber, PhoneNumber, EmailAdress, Fax, Adress, CreationDate);
                    var vcf = new StringBuilder();
                    vcf.AppendLine("BEGIN:VCARD");
                    vcf.AppendLine("VERSION:4.0");
                    vcf.AppendLine($"N:{FirstName};{LastName};;;");
                    vcf.AppendLine($"FN:{LastName} {FirstName}");
                    vcf.AppendLine($"ORG:{Company}");
                    vcf.AppendLine($"TITLE:{JobTitle}");
                    vcf.AppendLine($"PHOTO;;TYPE=PNG:{data}");
                    vcf.AppendLine($"TEL;TYPE=work,voice;VALUE=uri:tel:{PhoneNumber}");
                    vcf.AppendLine($"TEL;TYPE=work,mobile;VALUE=uri:tel:{MobileNumber}");
                    vcf.AppendLine($"TEL;TYPE=work,fax;VALUE=uri:fax:{Fax}");
                    vcf.AppendLine($"ADR; TYPE = WORK; PREF = 1; LABEL = {Adress} :;; {Adress} ");
                    vcf.AppendLine($"EMAIL:{EmailAdress}");
                    vcf.AppendLine($"REV:{CreationDate}");
                    vcf.AppendLine($"NOTE:{EmailContent}");
                    vcf.AppendLine("END:VCARD");

                    EmailMessage message = new EmailMessage(Subject, EmailContent, DestinationEmail);

                    var fn = "BusinessCard.vcf";
                    var file = Path.Combine(FileSystem.CacheDirectory, fn);
                    File.WriteAllText(file, vcf.ToString());

                    message.Attachments.Add(new EmailAttachment(file));

                    await Email.ComposeAsync(message);

                }

                // Navigate backwards
                //await Shell.Current.GoToAsync("..");
                //await Shell.Current.GoToAsync(nameof(BusinesscardsPage));
            }
            catch (FeatureNotSupportedException featureEx)
            {
                Console.WriteLine("OnSaveButtonClicked error" + featureEx.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("OnSaveButtonClicked error" + ex.ToString());
            }
            finally
            {
                await Navigation.PopModalAsync();
            }
        }

        async void OnDeleteButtonClicked(object sender, EventArgs e)
        {
            try
            {
                var card = (Businesscard)BindingContext;

                await App.Database.DeleteBusinesscardAsync(card);



                // Navigate backwards
                //await Shell.Current.GoToAsync("..");
                //await Shell.Current.GoToAsync(nameof(BusinesscardsPage));
                //await Navigation.PopModalAsync();
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to delete card");
            }
            finally
            {
                await Navigation.PopModalAsync();
            }
        }

        string namefirst, namesecond;
        Button firstButton;

        private void OnSwapClicked(object sender, EventArgs e)
        {

            if (swapper == 0)
            {
                firstButton = (Button)sender;
                firstButton.BackgroundColor = Color.FromHex("2D2A29");
                string idfirst = firstButton.ClassId;
                namefirst = idfirst.Substring(0, idfirst.Length - 4);  // Swap eraf bij bvb. testSwap
                Console.Write(namefirst);
                swapper++;
            }
            else
            {
                Button secondButton = (Button)sender;
                string idsecond = secondButton.ClassId;
                namesecond = idsecond.Substring(0, idsecond.Length - 4);
                Console.Write(namesecond);

                swap(namefirst, namesecond);

                swapper = 0;
                firstButton.BackgroundColor = Color.FromHex("BA0C2F");
            }

        }


        private void swap(string first, string second)
        {
            Entry firstEntry = (Entry)FindByName(first + "Entry");
            Entry secondEntry = (Entry)FindByName(second + "Entry");

            string a = firstEntry.Text;
            string b = secondEntry.Text;

            firstEntry.Text = b;
            secondEntry.Text = a;
        }



        /*
         ****************************************************************************************************************************
         ----------------------------------------------------------------------------------------------------------------------------
         ****************************************************************************************************************************
         */
        // testfase: floating label

        int _placeholderFontSize = 18;
        int _titleFontSize = 14;
        int _topMargin = -32;


        public static readonly BindableProperty TextProperty = BindableProperty.Create("Text", typeof(string), typeof(string), string.Empty, BindingMode.TwoWay, null);//, HandleBindingPropertyChangedDelegate);

        public static readonly BindableProperty TitleProperty = BindableProperty.Create("Title", typeof(string), typeof(string), string.Empty, BindingMode.TwoWay, null);

        public static readonly BindableProperty TextProperty2 = BindableProperty.Create("Text2", typeof(string), typeof(string), string.Empty, BindingMode.TwoWay, null);//, HandleBindingPropertyChangedDelegate);

        public static readonly BindableProperty TitleProperty2 = BindableProperty.Create("Title", typeof(string), typeof(string), string.Empty, BindingMode.TwoWay, null);


        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public string Text2
        {
            get => (string)GetValue(TextProperty2);
            set => SetValue(TextProperty2, value);
        }



        
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Title2
        {
            get => (string)GetValue(TitleProperty2);
            set => SetValue(TitleProperty2, value);
        }

        //nieuwe methodes
        //zo krijg je het label dat bij de entry past of omgekeerd

        //lijst van entries beter zou zijn array (optimalisatie array)
        private Entry GetEntry(Label l)
        {
            if (l.Equals(LabelTitle))
            {
                return EntryField;
            }
            if (l.Equals(LabelTitle2))
            {
                return EntryField2;
            }
            return null;
        }

        //lijst van labels beter zou zijn array (optimalisatie array)
        private Label GetLabel(Entry e)
        {
            if (e.Equals(EntryField))
            {
                return LabelTitle;
            }
            if (e.Equals(EntryField2))
            {
                return LabelTitle2;
            }
            return null;
        }

        private string GetText(Entry e)
        {
            if (e.Equals(EntryField))
            {
                return Text;
            }
            if (e.Equals(EntryField2))
            {
                return Text;
            }
            return null;
        }

        //opgeroepen methodes

        //sender is entry
        async void Handle_Focused(object sender, FocusEventArgs e)
        {

            if (string.IsNullOrEmpty(GetText((Entry)sender)))
            {
                await TransitionToTitle( GetLabel((Entry)sender));
            }
        }

        //sender is entry
        async void Handle_Unfocused(object sender, FocusEventArgs e)
        {

            if (string.IsNullOrEmpty(GetText((Entry)sender)))
            {
                await TransitionToPlaceholder( GetLabel((Entry)sender));
            }
        }

        //sender is label
        void Handle_Tapped(object sender, EventArgs e)
        {
            if (IsEnabled)
            {
                GetEntry((Label) sender).Focus();
            }

        }


        //gebruikte functies

        async Task TransitionToTitle(Label l)
        {
            var t1 = l.TranslateTo(0, _topMargin, 100);
            var t2 = SizeTo(_titleFontSize, l);
            await Task.WhenAll(t1, t2);

        }

        /*
        async Task TransitionToTitle2(Label l)
        {
            l.TranslationX = 0;
            l.TranslationY = -30;
            l.FontSize = 14;
        }*/

        async Task TransitionToPlaceholder( Label l)
        {
                var t1 = l.TranslateTo(10, 0, 100);
                var t2 = SizeTo(_placeholderFontSize, l);
                await Task.WhenAll(t1, t2);
        }

        /*
        async Task TransitionToPlaceholder2(Label l)
        {
            l.TranslationX = 10;
            l.TranslationY = 0;
            l.FontSize = 14;

        }*/


        //tekst veranderen van grootte
        Task SizeTo(int fontSize, Label l)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            // setup information for animation
            Action<double> callback = input => { l.FontSize = input; };
            double startingHeight = l.FontSize;
            double endingHeight = fontSize;
            uint rate = 5;
            uint length = 100;
            Easing easing = Easing.Linear;

            // now start animation with all the setup information
            l.Animate("invis", callback, startingHeight, endingHeight, rate, length, easing, (v, c) => taskCompletionSource.SetResult(c));

            return taskCompletionSource.Task;
        }

        /* momenteel nog niet dynamisch
         * 
        static async void HandleBindingPropertyChangedDelegate(BindableObject bindable, object oldValue, object newValue)
        {
            var control = bindable as BusinesscardEntryPage;
            
            if (!control.EntryField.IsFocused)
            {
                if (!string.IsNullOrEmpty((string)newValue))
                {
                    await control.TransitionToTitle2(control.LabelTitle);
                }
                else
                {
                    await control.TransitionToPlaceholder2(control.LabelTitle);
                }
            }
        }*/



        /*
         ****************************************************************************************************************************
         ----------------------------------------------------------------------------------------------------------------------------
         ****************************************************************************************************************************
         */


    }
}

