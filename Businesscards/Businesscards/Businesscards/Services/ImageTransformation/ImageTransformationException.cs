using System;
using System.Collections.Generic;
using System.Text;

namespace Businesscards.Services.ImageTransformation
{
    public class ImageTransformationException : Exception
    {
        public ImageTransformationException(string ex) : base(ex)
        {

        }
    }
}
