#region

using Silk_Items.Components;

#endregion

namespace Silk_Items.Entities
{
    public class Sword : Entity
    {
        public Sword() => Add(new DamageIcomponent());
    }
}