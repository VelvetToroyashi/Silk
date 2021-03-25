
using System;
using System.Collections;
using System.Collections.Generic;
using Silk.Core.Data.MediatR;

namespace Silk.Core.Types
{
    public sealed class LoopedList<T> : IEnumerable<T>
    {
        private int _pos = -1;
        private readonly List<T> _list = new();
        
        public T Next() => unchecked(this[++_pos]);
        
        public T this[int index]
        {
            get
            {
                if (_list.Count is 0)
                    throw new ArgumentOutOfRangeException(nameof(_list), "Collection size must be greater than zero.");
                
                return _list[index % _list.Count];
            }
            set
            {
                if (_list.Count is 0)
                    throw new ArgumentOutOfRangeException(nameof(_list), "Collection size must be greater than zero.");
                
                _list[index % _list.Count] = value;
            }
        }

        public int Count => _list.Count;

        public void Add(T value) => _list.Add(value);
        
        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}