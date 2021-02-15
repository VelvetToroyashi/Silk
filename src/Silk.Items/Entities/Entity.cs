#region

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Silk.Items.Components;

#endregion

namespace Silk.Items.Entities
{
    public abstract class Entity : IEnumerable<IComponent>
    {
        [JsonInclude]
        private readonly List<IComponent> Components = new();

        public IEnumerator<IComponent> GetEnumerator() => Components.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool Has<C>() where C : struct =>
            this.Count(c => c.GetType().Name == typeof(C).Name) > 1;

        public bool Add(IComponent comp)
        {
            if (!this.Contains(comp))
            {
                Components.Add(comp);
                return true;
            }
            return false;
        }

        public void Remove(IComponent comp) => Components.Remove(comp);

        public TComp? Get<TComp>() where TComp : class => this.SingleOrDefault(c => c.GetType().Name == typeof(TComp).Name) as TComp;
    }
}