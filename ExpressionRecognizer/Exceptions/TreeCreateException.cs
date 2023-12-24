using System;

namespace ExpRecognizer.Exceptions
{
    public class TreeCreateException : Exception 
    {
        public TreeCreateException(string message, Exception exception) : base(message, exception)
        { }
    }
}