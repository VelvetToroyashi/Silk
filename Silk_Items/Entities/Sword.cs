using Silk_Items.Components;

namespace Silk_Items.Entities
{
    public class Sword : Entity<IComponent>
    {
        public Sword() => this.Add(new DamageComponent());
    }
}