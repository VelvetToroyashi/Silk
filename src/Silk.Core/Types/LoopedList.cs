using System;
using System.Collections.Generic;

namespace Silk.Core.Types
{
    public sealed class LoopedList<T> : List<T>
    {
        private int _pos = -1;

        public T Next() => unchecked(this[++_pos]);

        public new T this[int index]
        {
            get => Count is 0 ? throw new ArgumentOutOfRangeException(nameof(index), "Colletion must be non-empty.") : base[index % Count];
            set
            {
                if (Count is 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), "Colletion must be non-empty.");
                }
                else
                {
                    base[index % Count] = value;
                }
            }
        }
    }
}