using Businesscards.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace Businesscards.Validations
{
    public class ValidatableObject<T> : ExtendedBindableObject, IValidity
    {
        private readonly List<IValidationRule<T>> _validations;
        private List<string> _errors;
        private T _value;
        private bool _isValid;
        private bool _isVisible;
        private int _heightRequestLabel;

        public List<IValidationRule<T>> Validations => _validations;

        public List<string> Errors
        {
            get
            {
                return _errors;
            }
            set
            {
                _errors = value;
                RaisePropertyChanged(() => Errors);
            }
        }

        public T Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                RaisePropertyChanged(() => Value);
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

        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                _isVisible = value;
                RaisePropertyChanged(() => IsVisible);
            }
        }

        public int HeightRequestLabel
        {
            get
            {
                return _heightRequestLabel;
            }
            set
            {
                _heightRequestLabel = value;
                RaisePropertyChanged(() => HeightRequestLabel);
            }
        }

        public ValidatableObject()
        {
            _isValid = true;
            _errors = new List<string>();
            _validations = new List<IValidationRule<T>>();
        }

        public bool Validate()
        {
            Errors.Clear();

            IEnumerable<string> errors = _validations.Where(v => !v.Check(Value))
                .Select(v => v.ValidationMessage);

            Errors = errors.ToList();
            IsValid = !Errors.Any();

            IsVisible = !IsValid;
            if (IsVisible)
            {
                HeightRequestLabel = 20;
            }
            else
            {
                HeightRequestLabel = 0;
            }


            return this.IsValid;
        }
    }
}
