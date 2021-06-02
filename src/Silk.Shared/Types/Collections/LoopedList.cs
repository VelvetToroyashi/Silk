using System;
using System.Collections.Generic;

namespace Silk.Shared.Types.Collections
{
    public sealed class LoopedList<T> : List<T>
    {
        private uint _pos;

        public new T this[int index]
        {
            get => Count is 0 ? throw new ArgumentOutOfRangeException(nameof(index), "Colletion must be non-empty.") : base[index % Count];
            set
            {
                if (Count is 0)
                    throw new ArgumentOutOfRangeException(nameof(index), "Colletion must be non-empty.");
                base[index % Count] = value;
            }
        }

        public T Next()
        {
            return unchecked(this[(int) _pos++]);
        }
    }
}