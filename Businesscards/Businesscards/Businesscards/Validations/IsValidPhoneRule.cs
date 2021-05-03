namespace Businesscards.Validations
{
    public class IsValidPhoneRule<T> : IValidationRule<T>
    {
        public string ValidationMessage { get; set; }

        public bool Check(T value)
        {
            if (value == null)
            {
                return true;
            }
            var str = value as string;
            string pattern = "[a-z]";
            
            if (System.Text.RegularExpressions.Regex.IsMatch(str, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                return false;
            }
            return true;
        }
    }
}
