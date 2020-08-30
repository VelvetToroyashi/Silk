using System;
using System.Collections.Generic;
using System.Text;

namespace SilkBot.Exceptions
{
    public class UnauthorizedUserException : Exception
    {
        public UnauthorizedUserException() : base() { }
        public UnauthorizedUserException(string message) : base(message) { }
        public UnauthorizedUserException(string message, Exception innerException) : base(message, innerException) { }
    }
}
