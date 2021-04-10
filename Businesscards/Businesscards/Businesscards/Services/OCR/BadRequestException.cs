using System;

namespace Businesscards.Services.OCR
{
    public class BadRequestException : Exception
    {
        public BadRequestException(string ex) : base(ex)
        {

        }
    }
}
