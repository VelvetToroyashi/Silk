using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Silk_Items.Components;

namespace Silk_Items.Entities
{
    public abstract class Entity<T> : IEnumerable<T> where T : IComponent

    {
    private List<T> Components = new();
    public IEnumerator<T> GetEnumerator() => Components.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool Has<C>() where C : struct =>
        this.Count(c => c.GetType().Name == typeof(C).Name) > 1;

    public bool Add(T comp)
    {
        if (!this.Contains(comp))
        {
            Components.Add(comp);
            return true;
        }
        else return false;
    }

    public void Remove(T comp) => Components.Remove(comp);

    public TComp Get<TComp>() where TComp : class => this.SingleOrDefault(c => c.GetType().Name == typeof(TComp).Name) as TComp;
    }
}