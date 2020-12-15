using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Silk_Items.Components;

namespace Silk_Items.Tools
{
    public static class ComponentHandler
    {
        private static ConcurrentDictionary<string, Type> componenents = new();

        public static bool Exists(string name, out Type type)
        {
            if (componenents.IsEmpty)
                throw new InvalidOperationException("Init must be called before accessing component dictionary!");
            else return componenents.TryGetValue(name.ToLower(), out type);
        }
        
        /// <summary>
        /// Builds a dictionary of components to prevent using reflection every time checking a component exists is needed.
        /// </summary>
        public static void Init()
        {
            IEnumerable<Type> comps = Assembly.GetAssembly(typeof(Icomponent))?.GetTypes()
                                              .Where(c => c.GetType().IsSubclassOf(typeof(Icomponent)));
            foreach (Type c in comps!)
                componenents.TryAdd(c.Name.ToLower(), c.GetType());
        }
        
    }
}