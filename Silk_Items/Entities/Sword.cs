using Silk_Items.Components;

namespace Silk_Items.Entities
{
    public class Sword : Entity
    {
        public Sword() => this.Add(new DamageIcomponent());
    }
}