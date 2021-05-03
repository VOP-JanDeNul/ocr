using System;

namespace Businesscards.Services.OCR
{
    public class FormRecognizerException : Exception
    {
        public FormRecognizerException(string ex) : base(ex)
        {

        }
    }
}
