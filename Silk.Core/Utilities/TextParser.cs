#region

using System;

#endregion

namespace Silk.Core.Utilities
{
    public abstract class TextParser
    {
        public const char EOT = '\0';

        protected int CurrentPosition;
        protected string Source;

        int _nextWhitespaceEnd;

        public TextParser(string currentString) => Source = currentString;

        protected char Read()
        {
            if (!TryConsumeWhitespace()) return EOT;
            return Source[CurrentPosition++];
        }

        protected char Peek()
        {
            if (!TryConsumeWhitespace()) return EOT;
            return Source[CurrentPosition];
        }

        protected bool TryConsumeWhitespace()
        {
            if (CurrentPosition >= Source.Length) return false;

            while (char.IsWhiteSpace(Source[CurrentPosition])) CurrentPosition++;
            return true;
        }

        public bool ReadIf(char c)
        {
            var next = Peek();

            if (next == c)
            {
                Read();
                return true;
            }

            return false;
        }

        public int ReadNumber()
        {
            if (!char.IsNumber(Peek())) throw new Exception($"Expected integer at position {CurrentPosition}!");

            int startPos = CurrentPosition;

            // Read the numerical characters.
            while (char.IsNumber(Peek())) Read();

            // Convert the number.
            return int.Parse(Source[startPos..CurrentPosition]);
        }
    }
}
