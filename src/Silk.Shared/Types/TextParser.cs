using System;

namespace Silk.Shared.Types
{
    public abstract class TextParser
    {
        public const char EOT = '\0';

        protected int _currentPosition;

        int _nextWhitespaceEnd;
        protected string _source;

        public TextParser(string currentString)
        {
            _source = currentString;
        }

        protected char ReadChar()
        {
            if (!TryConsumeWhitespace()) return EOT;
            return _source[_currentPosition++];
        }

        protected char PeekChar()
        {
            if (!TryConsumeWhitespace()) return EOT;
            return _source[_currentPosition];
        }

        protected bool TryConsumeWhitespace()
        {
            if (_currentPosition >= _source.Length) return false;

            while (char.IsWhiteSpace(_source[_currentPosition])) _currentPosition++;
            return true;
        }

        public bool ReadIf(char c)
        {
            var next = PeekChar();

            if (next == c)
            {
                ReadChar();
                return true;
            }

            return false;
        }

        public int ReadNumber()
        {
            if (!char.IsNumber(PeekChar())) throw new Exception($"Expected integer at position {_currentPosition}!");

            int startPos = _currentPosition;

            // Read the numerical characters.
            while (char.IsNumber(PeekChar())) ReadChar();

            // Convert the number.
            return int.Parse(_source[startPos.._currentPosition]);
        }
    }
}