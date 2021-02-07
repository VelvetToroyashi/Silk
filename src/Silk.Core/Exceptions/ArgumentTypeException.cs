using System;

namespace Silk.Core.Exceptions
{
    public sealed class ArgumentTypeException : Exception
    {
        public ArgumentTypeException(string? message) : base(message) { }
    }
}