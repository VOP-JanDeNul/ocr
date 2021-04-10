using System;

namespace Businesscards.Services.Rest
{
    public class RestException : Exception
    {
        public RestException(string ex) : base(ex)
        {
        }
    }
}
