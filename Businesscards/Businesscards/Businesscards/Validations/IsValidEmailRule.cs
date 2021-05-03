using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Businesscards.Validations
{
    public class IsValidEmailRule<T> : IValidationRule<T>
    {
        public string ValidationMessage { get; set; }

        public bool Check(T value)
        {
            try
            {
                //var addr = new System.Net.Mail.MailAddress($"{value}");
                //return addr.Address == $"{value}";
                var validation = new EmailAddressAttribute();
                return validation.IsValid(value);
            }
            catch
            {
                return false;
            }
        }
    }
}
