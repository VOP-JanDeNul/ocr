using System;

namespace Businesscards.Services.Mail
{
    public class MailException : Exception
    {
        public MailException(string ex) : base(ex)
        {

        }
    }
}
