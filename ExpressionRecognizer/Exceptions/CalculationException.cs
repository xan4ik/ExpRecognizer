using System;

namespace ExpRecognizer.Exceptions
{
    public class CalculationException : Exception 
    {
        public CalculationException(string message) : base(message)
        { }


        public CalculationException(string message, Exception exception) : base(message, exception)
        { }
    }


}