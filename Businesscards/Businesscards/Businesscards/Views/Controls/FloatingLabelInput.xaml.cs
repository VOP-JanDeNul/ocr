using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Businesscards.Views.Controls
{
    // Based on: https://github.com/vecalion/FloatingLabels
    // Explanation: https://trailheadtechnology.com/building-a-floating-label-entry-with-xamarin-forms/

    public partial class FloatingLabelInput : ContentView
    {
        // Declares the fontsize when the label is down, over the enty
        int _placeholderFontSize = 18;
        // Declares the fontsize when the label is up, above the entry
        int _titleFontSize = 14;
        // Declares the margin when the label is a title, above the entry. 
        int _topMargin = -30;

        public event EventHandler Completed;

        // Necessary to work with bindings in the FloatingLabelInput.xaml file. Gets the values from the BusinesscardEntryPage file.
        // Text is the content of the entry field.
        public static readonly BindableProperty TextProperty = BindableProperty.Create("Text", typeof(string), typeof(string), string.Empty, BindingMode.TwoWay, null, HandleBindingPropertyChangedDelegate);

        // Title is the value of the label.
        public static readonly BindableProperty TitleProperty = BindableProperty.Create("Title", typeof(string), typeof(string), string.Empty, BindingMode.TwoWay, null);

        // ReturnType gives the value of the green button (bottom right) of the keyboard. 'Next' jumps to next entry field, 'Done' closes the keyboard.
        public static readonly BindableProperty ReturnTypeProperty = BindableProperty.Create(nameof(ReturnType), typeof(ReturnType), typeof(FloatingLabelInput), ReturnType.Default);

        // Keyboard declares the type of keyboard for each entry. 'Telephone' gives a numerical keyboard, 'Email' shows an at-tail symbol (@).
        public static readonly BindableProperty KeyboardProperty = BindableProperty.Create("Keyboard", typeof(Keyboard), typeof(FloatingLabelInput), Keyboard.Default, coerceValue: (o, v) => (Keyboard)v ?? Keyboard.Default);


        // Text filled in the Entry from the control FloatingLabelInput => user input
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        // Text attribute of the Label from the control FloatingLabelInput => fixed value, given in BusinesscardEntryPage
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        // Attribute ReturnType sets the action of the green button (bottom right) of the keyboard.
        // 'Next' jumps to next entry field, 'Done' closes the keyboard.
        public ReturnType ReturnType
        {
            get => (ReturnType)GetValue(ReturnTypeProperty);
            set => SetValue(ReturnTypeProperty, value);
        }

        // Attribute Keyboard gives the right type of keyboard to fill in the entries of the control.
        // 'Telephone' gives a numerical keyboard, 'Email' shows an at-tail symbol (@).
        public Keyboard Keyboard
        {
            get { return (Keyboard)GetValue(KeyboardProperty); }
            set { SetValue(KeyboardProperty, value); }
        }

        // Constructor
        public FloatingLabelInput()
        {
            InitializeComponent();
            LabelTitle.TranslationX = 10;
            LabelTitle.FontSize = _placeholderFontSize;
        }

        // Makes it possible to put focus on the Entry field of the FloatingLabelInput programmatically
        public new void Focus()
        {
            if (IsEnabled)
            {
                EntryField.Focus();
            }
        }

        // Each time when the Entry is empty and gets focus, the placeholder will move up.
        // Used in the Entry control of the FloatingLabelInput.xaml file
        async void Handle_Focused(object sender, FocusEventArgs e)
        {
            if (string.IsNullOrEmpty(Text))
            {
                await TransitionToTitle(true);
            }
        }

        // Each time when the Entry is empty and loses focus, the placeholder will move back down.
        // Used in the Entry control of the FloatingLabelInput.xaml file
        async void Handle_Unfocused(object sender, FocusEventArgs e)
        {
            if (string.IsNullOrEmpty(Text))
            {
                await TransitionToPlaceholder(true);
            }
        }


        // To make from the placeholder in the Entry a Title above the Entry
        // Moves the label above the entry and decreases the font size, JDN color.
        async Task TransitionToTitle(bool animated)
        {
            LabelTitle.TextColor = Color.FromRgb(186, 12, 47);          // #BA0C2F
            if (animated)       // If tapped, with animation
            {
                var t1 = LabelTitle.TranslateTo(0, _topMargin, 100);
                var t2 = SizeTo(_titleFontSize);
                await Task.WhenAll(t1, t2);
            }
            else        // No tapping, just set the right parameters
            {
                LabelTitle.TranslationX = 0;
                LabelTitle.TranslationY = -30;
                LabelTitle.FontSize = 14;
            }
        }

        // To make from the Title above the Entry a placeholder in the Entry
        // Moves the label in the entry and increases the font size, gray color.
        // bool animated: possible to skip the animation, needed to set the initial position of the label
        // No animation when an filled form is opened or when the text is changed programmatically.
        async Task TransitionToPlaceholder(bool animated)
        {
            LabelTitle.TextColor = Color.Gray;
            if (animated)       // If tapped, with animation
            {
                var t1 = LabelTitle.TranslateTo(10, 0, 100);
                var t2 = SizeTo(_placeholderFontSize);
                await Task.WhenAll(t1, t2);
            }
            else    // No tapping, just set the right parameters
            {
                LabelTitle.TranslationX = 10;
                LabelTitle.TranslationY = 0;
                LabelTitle.FontSize = _placeholderFontSize;
            }
        }

        // Delegate to give the label the correct format. Title (above Entry) or placeholder (in Entry).
        // That depends on whether the entry contains text or not (null, empty). Without animation (false)
        static async void HandleBindingPropertyChangedDelegate(BindableObject bindable, object oldValue, object newValue)
        {
            var control = bindable as FloatingLabelInput;
            if (!control.EntryField.IsFocused)
            {
                if (!string.IsNullOrEmpty((string)newValue))
                {
                    await control.TransitionToTitle(false);
                }
                else
                {
                    await control.TransitionToPlaceholder(false);
                }
            }
        }

        // Used in the FloatingLabelInput.xaml file to pass the focus from the placeholder/label to the entry
        void Handle_Tapped(object sender, EventArgs e)
        {
            if (IsEnabled)
            {
                EntryField.Focus();
            }
        }

        // Used in TransitionToTitle(...) and TransitionToPlaceholder(...) for changing all the parameters to do that actions.
        Task SizeTo(int fontSize)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            // setup information for animation
            Action<double> callback = input => { LabelTitle.FontSize = input; };
            double startingHeight = LabelTitle.FontSize;
            double endingHeight = fontSize;
            uint rate = 5;
            uint length = 100;
            Easing easing = Easing.Linear;

            // now start animation with all the setup information
            LabelTitle.Animate("invis", callback, startingHeight, endingHeight, rate, length, easing, (v, c) => taskCompletionSource.SetResult(c));

            return taskCompletionSource.Task;
        }

        // Occurs when the user finalizes the text in the Entry with the return key.
        // Used in the Entry control of the FloatingLabelInput.xaml file
        void Handle_Completed(object sender, EventArgs e)
        {
            Completed?.Invoke(this, e);
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == nameof(IsEnabled))
            {
                EntryField.IsEnabled = IsEnabled;
            }
        }
    }
}