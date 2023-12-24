using System;

namespace ExpRecognizer.Exceptions

{
    public class BadSyntaxException : Exception
    {
        public BadSyntaxException(string message) : base(message)
        { }

        public BadSyntaxException(string message, Exception innerException) : base(message, innerException)
        { }
    }


}