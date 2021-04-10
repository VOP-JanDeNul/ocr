using System;
using System.Collections.Generic;
using System.Text;

namespace Businesscards.Services.Mail
{
    public class MailException : Exception
    {
        public MailException(string ex) : base(ex)
        {

        }
    }
}
