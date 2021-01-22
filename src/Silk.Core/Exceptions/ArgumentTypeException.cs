using System;

namespace Silk.Core.Exceptions
{
    public sealed class ArgumentTypeException : Exception
    {
        public ArgumentTypeException() { }
        public ArgumentTypeException(string? message) : base(message) { }
        public ArgumentTypeException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}