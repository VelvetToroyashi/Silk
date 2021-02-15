#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Silk.Items.Components;

#endregion

namespace Silk.Items.Tools
{
    public static class ComponentHandler
    {
        private static readonly ConcurrentDictionary<string, Type> componenents = new();

        public static bool Exists(string name, out Type type)
        {
            if (componenents.IsEmpty)
                throw new InvalidOperationException("Init must be called before accessing component dictionary!");
            return componenents.TryGetValue(name.ToLower(), out type!);
        }

        /// <summary>
        /// Builds a dictionary of components to prevent using reflection every time checking a component exists is needed.
        /// </summary>
        public static void Init()
        {
            IEnumerable<Type>? comps = Assembly.GetAssembly(typeof(IComponent))
                ?.GetTypes()
                .Where(c => c.IsSubclassOf(typeof(IComponent)));
            foreach (Type c in comps!)
                componenents.TryAdd(c.Name.ToLower(), c.GetType());
        }

    }
}