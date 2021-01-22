#region

using Silk.Items.Components;

#endregion

namespace Silk.Items.Entities
{
    public class Sword : Entity
    {
        public Sword() => Add(new DamageIcomponent());
    }
}