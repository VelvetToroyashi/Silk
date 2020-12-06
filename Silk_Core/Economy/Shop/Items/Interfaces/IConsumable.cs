namespace SilkBot.Economy.Shop.Items.Interfaces
{
    public interface IConsumable
    {
        public void Consume(IEntity entity); //Pass 'this' from calling class.
    }
}