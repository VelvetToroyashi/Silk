using System;

namespace SilkBot.Exceptions
{
    public class InvalidDiceRollException : Exception
    {
        public InvalidDiceRollException() : base()
        {
        }

        public InvalidDiceRollException(string message) : base(message)
        {
        }

        public InvalidDiceRollException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}