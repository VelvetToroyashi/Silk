using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SilkBot.Utilities
{
    public interface ICopyable<T> where  T : class
    {

        public T Copy<TBase>(TBase from, T to)
        {
            var tProperties = typeof(T)
                              .GetProperties()
                              .Where(p => p.CanWrite);
            var bProperties = typeof(TBase)
                              .GetProperties()
                              .Where(p => p.CanWrite);

            IEnumerable<PropertyInfo> propertyInfos = tProperties.ToList();
            if (propertyInfos
                .All(p => !bProperties
                    .Any(b => b.Name == p.Name)))
                throw new InvalidOperationException($"Cannot copy from {typeof(TBase).Name} to {typeof(T).Name}");


            var props = propertyInfos
                        .Concat(bProperties)
                        .GroupBy(p => p.Name)
                        .Where(p => p.Count() > 1);
            return to;
        }
        
    }
}