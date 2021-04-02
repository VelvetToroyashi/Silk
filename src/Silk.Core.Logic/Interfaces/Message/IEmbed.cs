using System.Drawing;

namespace Silk.Core.Logic.Interfaces.Message
{
    public interface IEmbed
    {
        public string? Title { get; }
        public string? Description { get; }
        public (string, string)[] Fields { get; }
        public virtual Color Color => Color.DimGray;
        public string? Footer { get; }
    }
}