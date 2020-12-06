using System.Collections.Generic;

namespace SilkBot.Economy.Shop.Items.Interfaces
{
    public interface IEntity
    {
        public int Health { get; private protected set; }


        // Used to not only calculate any health boosts, but how much damage should be taken. //
        public IBaseItem[] Armor { get; set; } // TODO: Change this to IBaseArmor instead. //



        public List<IBaseItem> Items { get; private protected set; } // Inventory, essentially. //


    }
}